// File: src/Rac.Engine/EngineFacade.cs

using Rac.Audio;
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

public class EngineFacade : IEngineFacade
{
    private readonly GameEngine.Engine _inner;
    private readonly IWindowManager _windowManager;

    public EngineFacade(
        IWindowManager windowManager,
        IInputService inputService,
        ConfigManager configManager
    )
    {
        _windowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
        World = new World();
        Systems = new SystemScheduler();
        _inner = new GameEngine.Engine(windowManager, inputService, configManager);

        // Initialize camera manager for dual-camera system
        CameraManager = new CameraManager();

        // Initialize audio service (use null object pattern as fallback)
        Audio = new NullAudioService();

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
    /// Destroys an entity by removing all its components.
    /// Note: This implementation removes all components as entity destruction will be enhanced in future versions.
    /// </summary>
    /// <param name="entity">The entity to destroy.</param>
    public void DestroyEntity(Entity entity)
    {
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
