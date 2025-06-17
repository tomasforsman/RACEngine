// ════════════════════════════════════════════════════════════════════════════════
// GAME ENGINE
// ════════════════════════════════════════════════════════════════════════════════
//
// Central game loop orchestrator providing complete application lifecycle management
// with integrated rendering pipeline, input handling, and ECS coordination.
//
// CORE CAPABILITIES:
// - Window lifecycle management with configuration-driven setup
// - Multi-pass rendering pipeline with clear/render/finalize phases
// - Comprehensive input handling (mouse, keyboard) with event propagation
// - ECS update coordination with frame timing
// - Dynamic vertex management with type safety and legacy compatibility
// - Advanced renderer integration (shader modes, post-processing)
// - Resource management with graceful initialization and cleanup
//
// RENDERING SYSTEM:
// - Flexible vertex upload supporting both raw float arrays and typed structures
// - Automatic renderer state management and frame synchronization
// - Buffered vertex loading for early initialization scenarios
// - Direct access to advanced rendering features through typed renderer
//
// EVENT ARCHITECTURE:
// - Structured lifecycle events (Load, Update, Render)
// - Input event propagation (clicks, keyboard) to application layer
// - Configuration-driven window and rendering setup
// - Clean separation between engine concerns and application logic

using Rac.Core.Manager;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Rendering;
using Rac.Rendering.Shader;

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Rac.GameEngine;

/// <summary>
/// Production-ready game engine providing comprehensive application lifecycle management.
///
/// DESIGN GOALS:
/// - Complete abstraction of windowing and rendering complexity
/// - Type-safe vertex management with backward compatibility
/// - Structured event-driven architecture for game logic integration
/// - Configuration-driven setup with sensible defaults
/// - Clean resource management and deterministic cleanup
///
/// CAPABILITIES:
/// - Window management with resize handling and configuration
/// - Multi-phase rendering pipeline (clear/render/finalize)
/// - Input service integration with event propagation
/// - ECS update coordination with precise frame timing
/// - Advanced renderer access for sophisticated graphics
/// </summary>
public class Engine
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CORE ENGINE INFRASTRUCTURE
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly ConfigManager _configManager;
    private readonly IInputService _inputService;
    private IWindow _window = null!;
    private OpenGLRenderer _renderer = null!;

    // ═══════════════════════════════════════════════════════════════════════════
    // VERTEX MANAGEMENT SYSTEM
    // ═══════════════════════════════════════════════════════════════════════════

    private float[]? _pendingVertices;

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC INTERFACE AND EVENT SYSTEM
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Standard renderer interface for basic rendering operations</summary>
    public IRenderer Renderer => _renderer;

    /// <summary>Advanced renderer access for shader modes and post-processing effects</summary>
    public OpenGLRenderer AdvancedRenderer => _renderer;

    /// <summary>Window management interface for resize and native window operations</summary>
    public IWindowManager WindowManager { get; }

    /// <summary>Fires once after OpenGL context and window initialization is complete</summary>
    public event Action? OnLoadEvent;

    /// <summary>Fires each frame before rendering: execute ECS systems and game logic here</summary>
    public event Action<float>? OnEcsUpdate;

    /// <summary>Fires during render pass: issue SetColor/UpdateVertices/Draw calls here</summary>
    public event Action<float>? OnRenderFrame;

    /// <summary>Fires when left mouse button is clicked with screen coordinates</summary>
    public event Action<Vector2D<float>>? OnLeftClick;

    /// <summary>Fires for all keyboard events with key and event type information</summary>
    public event Action<Key, KeyboardKeyState.KeyEvent>? OnKeyEvent;

    public Engine(
        IWindowManager windowManager,
        IInputService inputService,
        ConfigManager configManager
    )
    {
        WindowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
        _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
    }

    /// <summary>
    /// Initializes and starts the main game loop with configuration-driven window setup.
    /// This method blocks until the application terminates.
    /// </summary>
    public void Run()
    {
        CreateAndConfigureWindow();
        InitializeRenderer();
        SetupEventHandlers();

        _window.Run();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VERTEX DATA MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Upload raw float array vertex data with automatic layout detection.
    /// Maintains backward compatibility with existing rendering code.
    /// </summary>
    /// <param name="vertices">Float array containing vertex data in basic position format</param>
    public void UpdateVertices(float[] vertices)
    {
        if (_renderer != null)
        {
            _renderer.UpdateVertices(vertices);
        }
        else
        {
            _pendingVertices = vertices;
        }
    }

    /// <summary>
    /// Upload typed vertex data with compile-time type safety and automatic layout detection.
    /// Supports BasicVertex, TexturedVertex, FullVertex and other structured vertex types.
    /// </summary>
    /// <typeparam name="T">Vertex structure type implementing required layout methods</typeparam>
    /// <param name="vertices">Array of structured vertex data</param>
    public void UpdateVertices<T>(T[] vertices) where T : unmanaged
    {
        _renderer?.UpdateVertices(vertices);
    }

    /// <summary>
    /// Upload raw float array with explicit vertex layout specification.
    /// Enables fine-grained control over vertex attribute configuration.
    /// </summary>
    /// <param name="vertices">Raw float array containing vertex data</param>
    /// <param name="layout">Explicit vertex layout defining attribute structure</param>
    public void UpdateVertices(float[] vertices, VertexLayout layout)
    {
        _renderer?.UpdateVertices(vertices, layout);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ADVANCED RENDERING CONTROL
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Set active shader mode for subsequent rendering operations.
    /// Available modes include Normal, SoftGlow, Bloom, and other effects.
    /// </summary>
    /// <param name="mode">Shader mode to activate</param>
    public void SetShaderMode(ShaderMode mode)
    {
        _renderer?.SetShaderMode(mode);
    }

    /// <summary>
    /// Set uniform value in current shader program with type safety.
    /// Enables dynamic parameter control for custom shader effects.
    /// </summary>
    /// <param name="name">Uniform variable name in shader</param>
    /// <param name="value">Uniform value to set</param>
    public void SetUniform(string name, float value)
    {
        _renderer?.SetUniform(name, value);
    }

    public void SetUniform(string name, Vector2D<float> value)
    {
        _renderer?.SetUniform(name, value);
    }

    public void SetUniform(string name, Vector4D<float> value)
    {
        _renderer?.SetUniform(name, value);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INITIALIZATION AND CONFIGURATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates and configures window based on configuration manager settings
    /// </summary>
    private void CreateAndConfigureWindow()
    {
        var builder = WindowBuilder.Configure(WindowManager);
        var settings = _configManager.Window;

        // Apply configuration-driven window properties
        if (!string.IsNullOrEmpty(settings.Title))
            builder = builder.WithTitle(settings.Title);

        if (!string.IsNullOrEmpty(settings.Size))
        {
            if (TryParseWindowSize(settings.Size, out var size))
                builder = builder.WithSize(size.X, size.Y);
        }

        if (settings.VSync.HasValue)
            builder = builder.WithVSync(settings.VSync.Value);

        _window = builder.Create();
    }

    /// <summary>
    /// Initializes OpenGL renderer with window integration
    /// </summary>
    private void InitializeRenderer()
    {
        _renderer = new OpenGLRenderer();
        WindowManager.OnResize += newSize => _renderer.Resize(newSize);
    }

    /// <summary>
    /// Configures all window and input event handlers
    /// </summary>
    private void SetupEventHandlers()
    {
        // Window lifecycle events
        _window.Load += OnWindowLoad;
        _window.Render += OnWindowRender;
        _window.Update += OnWindowUpdate;
        _window.Update += delta => OnEcsUpdate?.Invoke((float)delta);
        _window.Closing += OnWindowClosing;

        // Input event propagation
        _inputService.OnLeftClick += pos => OnLeftClick?.Invoke(pos);
        _inputService.OnKeyEvent += (key, eventType) => OnKeyEvent?.Invoke(key, eventType);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // WINDOW EVENT HANDLERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Handles window load event: initializes OpenGL context and processes pending vertices
    /// </summary>
    private void OnWindowLoad()
    {
        _renderer.Initialize(_window);

        // Process any vertices that were uploaded before renderer initialization
        if (_pendingVertices is not null)
        {
            _renderer.UpdateVertices(_pendingVertices);
            _pendingVertices = null;
        }

        _inputService.Initialize(_window);
        OnLoadEvent?.Invoke();
    }

    /// <summary>
    /// Handles render frame: executes multi-pass rendering pipeline
    /// </summary>
    private void OnWindowRender(double deltaTime)
    {
        _renderer.Clear();
        OnRenderFrame?.Invoke((float)deltaTime);
        _renderer.Draw();
        _renderer.FinalizeFrame();
    }

    /// <summary>
    /// Handles update frame: processes input and maintains engine state
    /// </summary>
    private void OnWindowUpdate(double deltaTime)
    {
        _inputService.Update(deltaTime);
    }

    /// <summary>
    /// Handles window closing: ensures clean resource disposal
    /// </summary>
    private void OnWindowClosing()
    {
        _inputService.Shutdown();
        _renderer.Dispose();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Parse window size configuration string in "width,height" format
    /// </summary>
    /// <param name="sizeString">Size configuration string</param>
    /// <param name="size">Parsed size dimensions</param>
    /// <returns>True if parsing succeeded</returns>
    private static bool TryParseWindowSize(string sizeString, out Vector2D<int> size)
    {
        size = default;

        string[] parts = sizeString.Split(',');
        if (parts.Length != 2)
            return false;

        if (!int.TryParse(parts[0], out int width) || !int.TryParse(parts[1], out int height))
            return false;

        size = new Vector2D<int>(width, height);
        return true;
    }
}
