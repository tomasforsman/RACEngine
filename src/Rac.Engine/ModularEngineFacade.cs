using Rac.Audio;
using Rac.Core.Logger;
using Rac.Core.Manager;
using Rac.ECS.Core;
using Rac.ECS.Systems;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Rendering;
using Silk.NET.Input;

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
    private readonly World _world;
    private readonly SystemScheduler _systems;
    private readonly IRenderer _renderer;
    private readonly IAudioService _audio;

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

        _logger.LogDebug("Setting up event pipeline");
        SetupEventPipeline();

        _logger.LogInfo("ModularEngineFacade initialization completed");
    }

    // Properties never return null (Null Object pattern)
    public World World => _world;
    public SystemScheduler Systems => _systems;
    public IRenderer Renderer => _renderer;
    public IAudioService Audio => _audio;

    /// <summary>Fires once on init/load (before first UpdateEvent)</summary>
    public event Action? LoadEvent;

    /// <summary>Fires each frame after ECS updates.</summary>
    public event Action<float>? UpdateEvent;

    /// <summary>Fires each frame right before rendering.</summary>
    public event Action<float>? RenderEvent;

    /// <summary>Fires whenever a key is pressed or released.</summary>
    public event Action<Key, KeyboardKeyState.KeyEvent>? KeyEvent;

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
    }
}