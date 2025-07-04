// File: src/Rac.Engine/EngineFacade.cs

using Rac.Audio;
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

public class EngineFacade : IEngineFacade
{
    private readonly GameEngine.Engine _inner;
    private readonly IWindowManager _windowManager;
    private readonly TransformSystem _transformSystem;

    public EngineFacade(
        IWindowManager windowManager,
        IInputService inputService,
        ConfigManager configManager
    )
    {
        _windowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
        World = new World();
        Systems = new SystemScheduler(World);
        _inner = new GameEngine.Engine(windowManager, inputService, configManager);

        // Initialize camera manager for dual-camera system
        CameraManager = new CameraManager();

        // Initialize audio service (use null object pattern as fallback)
        Audio = new NullAudioService();

        // Initialize transform system (required for container operations)
        _transformSystem = new TransformSystem();
        Systems.Add(_transformSystem);

        // Initialize container service with container system integration
        var containerSystem = new ContainerSystem();
        Container = containerSystem;
        Systems.Add(containerSystem);

        // Set up camera system integration
        SetupCameraIntegration();

        // hook up core pipeline
        _inner.OnLoadEvent += () => LoadEvent?.Invoke();
        _inner.OnEcsUpdate += dt =>
        {
            Systems.Update(dt);
            UpdateEvent?.Invoke(dt);
        };
        _inner.OnRenderFrame += dt => 
        {
            // Update camera matrices before rendering
            UpdateCameraMatrices();
            RenderEvent?.Invoke(dt);
        };

        // forward key events
        _inner.OnKeyEvent += (key, evt) => KeyEvent?.Invoke(key, evt);
        
        // forward mouse events
        _inner.OnLeftClick += pos => LeftClickEvent?.Invoke(pos);
        _inner.OnMouseScroll += delta => MouseScrollEvent?.Invoke(delta);
    }

    public IWorld World { get; }
    public SystemScheduler Systems { get; }
    public IRenderer Renderer => _inner.Renderer;
    public IAudioService Audio { get; }
    public ICameraManager CameraManager { get; }
    public IWindowManager WindowManager => _windowManager;
    public IContainerService Container { get; }

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
        Systems.Add(system);
    }

    /// <summary>Start the engine loop.</summary>
    public void Run()
    {
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
        return World.CreateEntity();
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
        return entity;
    }

    /// <summary>
    /// Destroys an entity by removing all its components.
    /// This implementation now properly destroys the entity using the enhanced World API.
    /// </summary>
    /// <param name="entity">The entity to destroy.</param>
    public void DestroyEntity(Entity entity)
    {
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
        var result = World.Query<NameComponent>()
            .FirstOrDefault(result => result.Component1.Name == name);
        
        return result.Entity.Id != 0 ? result.Entity : null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTAINER MANAGEMENT CONVENIENCE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new container entity with the specified name.
    /// Convenience method that delegates to the container service for the most common use case.
    /// </summary>
    /// <param name="containerName">Human-readable name for the container</param>
    /// <returns>The newly created container entity</returns>
    public Entity CreateContainer(string containerName)
    {
        return Container.CreateContainer(containerName);
    }

    /// <summary>
    /// Places an item inside a container at the origin.
    /// Convenience method for the most common placement operation using extension methods.
    /// </summary>
    /// <param name="item">The entity to place inside the container</param>
    /// <param name="container">The container entity (must have ContainerComponent)</param>
    /// <exception cref="ArgumentException">Thrown when target entity is not a container</exception>
    public void PlaceInContainer(Entity item, Entity container)
    {
        // Use the extension method from ContainerExtensions
        item.PlaceIn(container, World, _transformSystem);
    }

    /// <summary>
    /// Places an item inside a container at the specified local position.
    /// Convenience method that provides positioning control using extension methods.
    /// </summary>
    /// <param name="item">The entity to place inside the container</param>
    /// <param name="container">The container entity (must have ContainerComponent)</param>
    /// <param name="localPosition">Local position within the container</param>
    /// <exception cref="ArgumentException">Thrown when target entity is not a container</exception>
    public void PlaceInContainer(Entity item, Entity container, Vector2D<float> localPosition)
    {
        // Use the extension method from ContainerExtensions
        item.PlaceIn(container, World, _transformSystem, localPosition);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CAMERA SYSTEM INTEGRATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets up camera system integration with window events and renderer.
    /// </summary>
    private void SetupCameraIntegration()
    {
        // Update camera system when window is resized
        _windowManager.OnResize += newSize =>
        {
            CameraManager.UpdateViewport(newSize.X, newSize.Y);
        };
    }

    /// <summary>
    /// Updates camera matrices and applies them to renderer.
    /// Called before each render frame to ensure proper transformations.
    /// </summary>
    private void UpdateCameraMatrices()
    {
        var windowSize = _windowManager.Size;
        
        // Ensure cameras have current viewport dimensions
        CameraManager.UpdateViewport(windowSize.X, windowSize.Y);
        
        // Set game camera matrix as default for world rendering
        // Applications can switch to UI camera matrix for UI rendering passes
        Renderer.SetCameraMatrix(CameraManager.GameCamera.CombinedMatrix);
    }
}
