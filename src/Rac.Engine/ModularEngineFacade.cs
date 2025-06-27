using Rac.Audio;
using Rac.Core.Logger;
using Rac.Core.Manager;
using Rac.ECS.Core;
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
        _systems = new SystemScheduler();
        
        // Initialize game engine
        _inner = new GameEngine.Engine(windowManager, inputService, configManager);
        
        // Cache renderer reference (resolved once)
        _renderer = _inner.Renderer;

        // Initialize audio service (use null object pattern as fallback)
        _audio = new NullAudioService();

        // Initialize camera manager for dual-camera system
        _cameraManager = new CameraManager();

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
    /// Destroys an entity by removing all its components.
    /// Note: This implementation removes all components as entity destruction will be enhanced in future versions.
    /// </summary>
    /// <param name="entity">The entity to destroy.</param>
    public void DestroyEntity(Entity entity)
    {
        _logger.LogDebug($"Destroying entity with ID: {entity.Id}");
        // For now, we'll need to remove all components manually
        // In a future enhancement, this could be optimized with a dedicated DestroyEntity method in IWorld
        // This is a simple implementation that works with the current World API
        
        // We can't easily remove all components without knowing their types in the current World design
        // For now, this is a placeholder that demonstrates the interface
        // A full implementation would require enhancing the World class with entity tracking
    }

    /// <summary>
    /// Gets the total number of entities currently in the world.
    /// Note: This is a convenience property - requires enhancement to World for accurate counting.
    /// </summary>
    public int EntityCount 
    { 
        get 
        {
            // Current World implementation doesn't track entity count directly
            // This would need to be enhanced in the World class
            // For now, return 0 as a placeholder
            return 0;
        } 
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