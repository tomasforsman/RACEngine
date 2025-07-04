using Rac.Audio;
using Rac.Core.Logger;
using Rac.Core.Manager;
using Rac.ECS.Core;
using Rac.ECS.Components;
using Rac.ECS.Systems;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Rendering;
using Rac.Rendering.Camera;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace Rac.Engine;

/// <summary>
/// Modular implementation of IEngineFacade with dependency injection support and service caching.
/// All services are resolved once during construction and cached for performance.
/// </summary>
public class ModularEngineFacade : IEngineFacade
{
    private readonly GameEngine.Engine _inner;
    private readonly ILogger _logger;
    private readonly IWindowManager _windowManager;
    private readonly IInputService _inputService;
    private readonly ConfigManager _configManager;

    // Cached service references (resolved once during construction)
    private readonly IWorld _world;
    private readonly SystemScheduler _systems;
    private readonly IRenderer _renderer;
    private readonly IAudioService _audio;
    private readonly ICameraManager _cameraManager;
    private readonly IContainerService _container;
    private readonly TransformSystem _transformSystem;

    public ModularEngineFacade(
        IWindowManager windowManager,
        IInputService inputService,
        ConfigManager configManager,
        ILogger logger)
    {
        // Cache all dependencies during construction
        _windowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
        _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("Initializing ModularEngineFacade with cached service references");

        // Initialize core services (never null due to Null Object pattern)
        _world = new World();
        _systems = new SystemScheduler(_world);
        
        // Initialize game engine
        _inner = new GameEngine.Engine(windowManager, inputService, configManager);
        
        // Cache renderer reference (resolved once)
        _renderer = _inner.Renderer;

        // Initialize audio service (use null object pattern as fallback)
        _audio = new NullAudioService();

        // Initialize camera manager for dual-camera system
        _cameraManager = new CameraManager();

        // Initialize transform system (required for container operations)
        _transformSystem = new TransformSystem();
        _systems.Add(_transformSystem);

        // Initialize container service with container system integration
        var containerSystem = new ContainerSystem();
        _container = containerSystem;
        _systems.Add(containerSystem);

        _logger.LogDebug("Setting up event pipeline");
        SetupEventPipeline();

        _logger.LogInfo("ModularEngineFacade initialization completed");
    }

    // Properties never return null (Null Object pattern)
    public IWorld World => _world;
    public SystemScheduler Systems => _systems;
    public IRenderer Renderer => _renderer;
    public IAudioService Audio => _audio;
    public ICameraManager CameraManager => _cameraManager;
    public IWindowManager WindowManager => _windowManager;
    public IContainerService Container => _container;
    public TransformSystem TransformSystem => _transformSystem;

    /// <summary>Fires once on init/load (before first UpdateEvent)</summary>
    public event Action? LoadEvent;

    /// <summary>Fires each frame after ECS updates.</summary>
    public event Action<float>? UpdateEvent;

    /// <summary>Fires each frame right before rendering.</summary>
    public event Action<float>? RenderEvent;

    /// <summary>Fires whenever a key is pressed or released.</summary>
    public event Action<Key, KeyboardKeyState.KeyEvent>? KeyEvent;

    /// <summary>Fires when the left mouse button is clicked, providing screen coordinates in pixels.</summary>
    public event Action<Vector2D<float>>? LeftClickEvent;

    /// <summary>Fires when the mouse wheel is scrolled, providing scroll delta.</summary>
    public event Action<float>? MouseScrollEvent;

    /// <summary>Register an ECS system.</summary>
    public void AddSystem(ISystem system)
    {
        if (system == null)
        {
            _logger.LogWarning("Attempted to add null system to engine");
            return;
        }

        _logger.LogDebug($"Adding ECS system: {system.GetType().Name}");
        Systems.Add(system);
    }

    /// <summary>Start the engine loop.</summary>
    public void Run()
    {
        _logger.LogInfo("Starting engine loop");
        _inner.Run();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ENTITY MANAGEMENT CONVENIENCE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new entity in the world.
    /// Convenience method that delegates to the underlying IWorld.
    /// </summary>
    /// <returns>A new Entity with a unique ID.</returns>
    public Entity CreateEntity()
    {
        var entity = World.CreateEntity();
        _logger.LogDebug($"Created entity with ID: {entity.Id}");
        return entity;
    }

    /// <summary>
    /// Creates a new entity with a specified name.
    /// Convenience method that creates an entity and assigns a NameComponent.
    /// </summary>
    /// <param name="name">Human-readable name for the entity</param>
    /// <returns>A new Entity with a unique ID and the specified name</returns>
    public Entity CreateEntity(string name)
    {
        var entity = World.CreateEntity();
        World.SetComponent(entity, new NameComponent(name));
        _logger.LogDebug($"Created named entity '{name}' with ID: {entity.Id}");
        return entity;
    }

    /// <summary>
    /// Destroys an entity by removing all its components.
    /// This implementation now properly destroys the entity using the enhanced World API.
    /// </summary>
    /// <param name="entity">The entity to destroy.</param>
    public void DestroyEntity(Entity entity)
    {
        _logger.LogDebug($"Destroying entity with ID: {entity.Id}");
        World.DestroyEntity(entity);
    }

    /// <summary>
    /// Gets the total number of entities currently in the world.
    /// This now returns the actual count of living entities.
    /// </summary>
    public int EntityCount 
    { 
        get 
        {
            return World.GetAllEntities().Count();
        } 
    }

    /// <summary>
    /// Finds entities that have the specified tag.
    /// </summary>
    /// <param name="tag">Tag to search for</param>
    /// <returns>Entities that have the specified tag</returns>
    public IEnumerable<Entity> GetEntitiesWithTag(string tag)
    {
        _logger.LogDebug($"Searching for entities with tag: {tag}");
        return World.Query<TagComponent>()
            .Where(result => result.Component1.HasTag(tag))
            .Select(result => result.Entity);
    }

    /// <summary>
    /// Finds the first entity with the specified name.
    /// </summary>
    /// <param name="name">Name to search for</param>
    /// <returns>Entity with the specified name, or null if not found</returns>
    public Entity? FindEntityByName(string name)
    {
        _logger.LogDebug($"Searching for entity with name: {name}");
        var result = World.Query<NameComponent>()
            .FirstOrDefault(result => result.Component1.Name == name);
        
        return result.Entity.Id != 0 ? result.Entity : null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTAINER MANAGEMENT CONVENIENCE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new container entity with the specified name.
    /// Convenience method that delegates to the container service.
    /// </summary>
    /// <param name="containerName">Human-readable name for the container</param>
    /// <returns>The newly created container entity</returns>
    public Entity CreateContainer(string containerName)
    {
        _logger.LogDebug($"Creating container: {containerName}");
        return Container.CreateContainer(containerName);
    }

    /// <summary>
    /// Places an item inside a container at the origin.
    /// Convenience method for the most common placement operation.
    /// </summary>
    /// <param name="item">The entity to place inside the container</param>
    /// <param name="container">The container entity (must have ContainerComponent)</param>
    /// <exception cref="ArgumentException">Thrown when target entity is not a container</exception>
    public void PlaceInContainer(Entity item, Entity container)
    {
        _logger.LogDebug($"Placing entity {item.Id} in container {container.Id}");
        // Use the extension method from ContainerExtensions
        item.PlaceIn(container, World, _transformSystem);
    }

    /// <summary>
    /// Places an item inside a container at the specified local position.
    /// Convenience method that provides positioning control.
    /// </summary>
    /// <param name="item">The entity to place inside the container</param>
    /// <param name="container">The container entity (must have ContainerComponent)</param>
    /// <param name="localPosition">Local position within the container</param>
    /// <exception cref="ArgumentException">Thrown when target entity is not a container</exception>
    public void PlaceInContainer(Entity item, Entity container, Vector2D<float> localPosition)
    {
        _logger.LogDebug($"Placing entity {item.Id} in container {container.Id} at position {localPosition}");
        // Use the extension method from ContainerExtensions
        item.PlaceIn(container, World, _transformSystem, localPosition);
    }

    /// <summary>
    /// Sets up the event pipeline with logging for debugging.
    /// </summary>
    private void SetupEventPipeline()
    {
        // Hook up core pipeline with logging
        _inner.OnLoadEvent += () =>
        {
            _logger.LogDebug("Engine load event triggered");
            LoadEvent?.Invoke();
        };

        _inner.OnEcsUpdate += dt =>
        {
            Systems.Update(dt);
            UpdateEvent?.Invoke(dt);
        };

        _inner.OnRenderFrame += dt =>
        {
            RenderEvent?.Invoke(dt);
        };

        // Forward key events with debugging
        _inner.OnKeyEvent += (key, evt) =>
        {
            _logger.LogDebug($"Key event: {key} - {evt}");
            KeyEvent?.Invoke(key, evt);
        };

        // Forward mouse events with debugging
        _inner.OnLeftClick += pos =>
        {
            _logger.LogDebug($"Mouse click at: {pos}");
            LeftClickEvent?.Invoke(pos);
        };

        _inner.OnMouseScroll += delta =>
        {
            _logger.LogDebug($"Mouse scroll delta: {delta}");
            MouseScrollEvent?.Invoke(delta);
        };
    }
}