using Rac.Assets;
using Rac.Assets.FileSystem;
using Rac.Engine;
using Rac.Assets.Types;
using Rac.Rendering;
using Rac.Rendering.Shader;
using Rac.Core.Manager;
using Rac.Input.Service;
using Rac.Input.State;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using PrimitiveType = Silk.NET.OpenGL.PrimitiveType;

namespace SampleGame;

/// <summary>
/// Enhanced Asset Demo showcasing texture and audio loading using the Engine facade with new 2D primitive drawing capabilities.
///
/// EDUCATIONAL PURPOSE:
/// This demo demonstrates the enhanced asset loading and drawing patterns using the Engine facade:
/// - Layer 1 asset loading (engine.LoadTexture, engine.LoadAudio)
/// - New simplified 2D quad drawing methods (DrawTexturedQuad, DrawSolidColorQuad)
/// - Advanced rendering features (color tinting, custom UV coordinates, texture atlasing)
/// - Visual feedback with multiple texture rendering examples
/// - Interactive audio playback on mouse clicks
/// - Camera zoom controls for interactive viewing
/// - Error handling and graceful fallbacks
///
/// NEW FEATURES DEMONSTRATED:
/// - DrawTexturedQuad with default and custom UV coordinates
/// - DrawSolidColorQuad for UI elements and fallbacks
/// - Color tinting effects on textured quads
/// - Dynamic animation using quad drawing parameters
/// - Multiple quad rendering for complex scenes
/// - Clean API replacing manual vertex array creation
///
/// INTERACTION:
/// - Displays multiple textured and colored quads showcasing different techniques
/// - Click anywhere in the window to play SampleAudio.wav
/// - Q/- keys to zoom out, E/+ keys to zoom in
/// - Press ESC to exit the demo
/// </summary>
public class AssetDemo
{
    private IEngineFacade? _engine;
    private Texture? _sampleTexture;
    private AudioClip? _sampleAudio;

    // Camera zoom settings (matching PupperQuest pattern)
    private const float ZoomSpeed = 0.5f;            // Zoom change per second
    private const float MinZoom = 0.01f;              // Minimum zoom level (zoomed out)
    private const float MaxZoom = 5.0f;              // Maximum zoom level (zoomed in)

    // Input state tracking for real-time zoom
    private readonly HashSet<Key> _pressedKeys = new();

    // Demo state for enhanced examples
    private float _animationTime = 0f;

    public static void Run(string[] args)
    {
        Console.WriteLine("RACEngine Asset System Demo");
        Console.WriteLine("===========================");
        Console.WriteLine("Loading texture and audio assets using Engine facade...");
        Console.WriteLine("Click in the window to play audio, Q/- to zoom out, E/+ to zoom in, ESC to exit.");
        Console.WriteLine();

        var demo = new AssetDemo();
        demo.Initialize();
        demo.RunLoop();
        demo.Cleanup();
    }

    private void Initialize()
    {
        try
        {
            // Initialize engine with required dependencies (same pattern as other samples)
            var windowManager = new WindowManager();
            var inputService = new SilkInputService();
            var configurationManager = new ConfigManager();

            // Configure window settings via ConfigManager
            configurationManager.Window.Title = "RACEngine Asset Demo";
            configurationManager.Window.Size = "800,600";
            _engine = new EngineFacade(windowManager, inputService, configurationManager);


            Console.WriteLine("✓ Engine initialized successfully");

            // Set up event handlers
            SetupEventHandlers();

            // Load assets using Engine facade (Layer 1 - Beginner API)
            LoadAssets();

            Console.WriteLine("✓ Demo initialization completed");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Initialization failed: {ex.Message}");
            throw;
        }
    }

    private void SetupEventHandlers()
    {
        if (_engine == null) return;

        // Handle mouse clicks to play audio
        _engine.LeftClickEvent += OnMouseClick;

        // Handle key presses for exit and zoom controls
        _engine.KeyEvent += OnKeyEvent;

        // Handle render events
        _engine.RenderEvent += OnRender;

        // Handle load event for renderer setup (after renderer is initialized)
        _engine.LoadEvent += OnLoad;

        Console.WriteLine("✓ Event handlers configured");
    }

    private void LoadAssets()
    {
        if (_engine == null) return;

        try
        {
            // Load texture using Engine facade (recommended approach)
            Console.WriteLine("Loading SampleTexture.png...");
            _sampleTexture = _engine.LoadTexture("SampleTexture.png");
            Console.WriteLine($"✓ Texture loaded: {_sampleTexture.Width}x{_sampleTexture.Height} pixels, {_sampleTexture.MemorySize / 1024}KB");

            // Load audio using Engine facade (recommended approach)
            Console.WriteLine("Loading SampleAudio.wav...");
            _sampleAudio = _engine.LoadAudio("SampleAudio.wav");
            Console.WriteLine($"✓ Audio loaded: {_sampleAudio.Duration:F2}s, {_sampleAudio.SampleRate}Hz, {_sampleAudio.Channels} channels, {_sampleAudio.MemorySize / 1024}KB");

            // Demonstrate asset service integration
            Console.WriteLine($"✓ Assets cached: {_engine.Assets.CachedAssetCount} items");
            Console.WriteLine($"✓ Cache memory usage: {_engine.Assets.CacheMemoryUsage / 1024}KB");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"✗ Asset not found: {ex.Message}");
            Console.WriteLine("Make sure SampleTexture.png and SampleAudio.wav exist in the assets directory.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Asset loading failed: {ex.Message}");
            throw;
        }
    }

    private void OnLoad()
    {
        if (_engine?.Renderer == null) return;

        try
        {
            // Set up basic rendering state (called after renderer is initialized)
            _engine.Renderer.SetColor(new Vector4D<float>(1.0f, 1.0f, 1.0f, 1.0f)); // White color for texture
            Console.WriteLine("✓ Rendering configured");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Rendering setup failed: {ex.Message}");
        }
    }

    private void RunLoop()
    {
        if (_engine == null) return;

        try
        {
            // Start the engine loop
            // This will handle the main game loop, events, and rendering
            _engine.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Runtime error: {ex.Message}");
            throw;
        }
    }

    private void OnRender(float deltaTime)
    {
        if (_engine?.Renderer == null) return;

        try
        {
            // Update animation time for enhanced examples
            _animationTime += deltaTime;

            // Update camera zoom based on input (real-time)
            UpdateCameraZoom(deltaTime);

            // Clear the screen
            _engine.Renderer.Clear();

            // Set active camera for world rendering
            _engine.Renderer.SetActiveCamera(_engine.CameraManager.GameCamera);

            // ENHANCED DEMO: Multiple quad examples showcasing the new drawing API
            RenderQuadExamples();

            // Finalize frame
            _engine.Renderer.FinalizeFrame();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Rendering error: {ex.Message}");
        }
    }

    /// <summary>
    /// Renders multiple quad examples showcasing the flexibility of the new DrawTexturedQuad and DrawSolidColorQuad methods.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// This method demonstrates various use cases for 2D quad rendering:
    /// - Simple textured quad rendering (replacing manual vertex arrays)
    /// - Color tinting effects on textures
    /// - Custom UV coordinate mapping for texture atlasing/cropping
    /// - Solid color quads for UI elements and fallbacks
    /// - Dynamic positioning and sizing
    /// - Animation through parameter changes
    /// </summary>
    private void RenderQuadExamples()
    {
        if (_engine == null) return;

        // Example 1: Main textured quad (center) - Replaces the original manual vertex rendering
        if (_sampleTexture != null)
        {
            Console.WriteLine($"🎨 Rendering textured quad using new API: {_sampleTexture.Width}x{_sampleTexture.Height}");
            
            // Simple textured quad at center - this replaces the original _squareVertices rendering
            _engine.DrawTexturedQuad(
                centerPosition: new Vector2D<float>(0f, 0f),
                size: new Vector2D<float>(200f, 200f),
                texture: _sampleTexture
            );
        }
        else
        {
            Console.WriteLine("❌ No texture loaded - rendering red fallback using new API");
            
            // Fallback solid color quad - much cleaner than manual shader/vertex setup
            _engine.DrawSolidColorQuad(
                centerPosition: new Vector2D<float>(0f, 0f),
                size: new Vector2D<float>(200f, 200f),
                color: new Vector4D<float>(0.8f, 0.2f, 0.2f, 1.0f) // Red fallback
            );
        }

        // Example 2: Color tinted quads (left side) - Shows tinting capabilities
        if (_sampleTexture != null)
        {
            // Red tinted quad
            _engine.DrawTexturedQuad(
                centerPosition: new Vector2D<float>(-300f, 100f),
                size: new Vector2D<float>(100f, 100f),
                texture: _sampleTexture,
                colorTint: new Vector4D<float>(1f, 0.5f, 0.5f, 1f) // Red tint
            );

            // Green tinted quad
            _engine.DrawTexturedQuad(
                centerPosition: new Vector2D<float>(-300f, 0f),
                size: new Vector2D<float>(100f, 100f),
                texture: _sampleTexture,
                colorTint: new Vector4D<float>(0.5f, 1f, 0.5f, 1f) // Green tint
            );

            // Blue tinted quad
            _engine.DrawTexturedQuad(
                centerPosition: new Vector2D<float>(-300f, -100f),
                size: new Vector2D<float>(100f, 100f),
                texture: _sampleTexture,
                colorTint: new Vector4D<float>(0.5f, 0.5f, 1f, 1f) // Blue tint
            );
        }

        // Example 3: Custom UV coordinates (right side) - Shows texture cropping/atlasing
        if (_sampleTexture != null)
        {
            // Top-left quarter of texture
            var topLeftUVs = new Vector2D<float>[]
            {
                new Vector2D<float>(0f, 0f),    // bottom-left
                new Vector2D<float>(0.5f, 0f),  // bottom-right
                new Vector2D<float>(0f, 0.5f),  // top-left
                new Vector2D<float>(0.5f, 0.5f) // top-right
            };

            _engine.DrawTexturedQuad(
                centerPosition: new Vector2D<float>(300f, 100f),
                size: new Vector2D<float>(100f, 100f),
                texture: _sampleTexture,
                textureCoordinates: topLeftUVs
            );

            // Bottom-right quarter of texture
            var bottomRightUVs = new Vector2D<float>[]
            {
                new Vector2D<float>(0.5f, 0.5f), // bottom-left
                new Vector2D<float>(1f, 0.5f),   // bottom-right
                new Vector2D<float>(0.5f, 1f),   // top-left
                new Vector2D<float>(1f, 1f)      // top-right
            };

            _engine.DrawTexturedQuad(
                centerPosition: new Vector2D<float>(300f, -100f),
                size: new Vector2D<float>(100f, 100f),
                texture: _sampleTexture,
                textureCoordinates: bottomRightUVs
            );
        }

        // Example 4: Solid color UI elements (top) - Shows UI/debugging use cases
        var panelColor = new Vector4D<float>(0.2f, 0.2f, 0.3f, 0.8f); // Semi-transparent dark blue
        _engine.DrawSolidColorQuad(
            centerPosition: new Vector2D<float>(0f, 250f),
            size: new Vector2D<float>(600f, 60f),
            color: panelColor
        );

        // Example 5: Animated quads (bottom) - Shows dynamic parameter usage
        var animOffset = (float)Math.Sin(_animationTime * 2.0) * 50f;
        var animColor = new Vector4D<float>(
            0.5f + (float)Math.Sin(_animationTime) * 0.3f,        // Animated red
            0.5f + (float)Math.Cos(_animationTime * 1.5) * 0.3f, // Animated green  
            0.7f,                                                 // Constant blue
            1f                                                    // Full alpha
        );

        _engine.DrawSolidColorQuad(
            centerPosition: new Vector2D<float>(animOffset, -250f),
            size: new Vector2D<float>(80f, 80f),
            color: animColor
        );

        // Static comparison quad next to animated one
        _engine.DrawSolidColorQuad(
            centerPosition: new Vector2D<float>(150f, -250f),
            size: new Vector2D<float>(80f, 80f),
            color: new Vector4D<float>(0.5f, 0.5f, 0.7f, 1f) // Static color
        );
    }

    private void OnMouseClick(Vector2D<float> position)
    {
        if (_engine?.Audio == null) return;

        try
        {
            if (_sampleAudio != null)
            {
                // Play audio using Engine facade
                Console.WriteLine($"🔊 Playing audio at mouse position: {position.X:F0}, {position.Y:F0}");

                // This demonstrates the interface even if audio doesn't actually play
                _engine.Audio.PlaySound(_sampleAudio.SourcePath);

                Console.WriteLine($"✓ Audio playback requested (Duration: {_sampleAudio.Duration:F2}s)");
            }
            else
            {
                Console.WriteLine("✗ No audio loaded - cannot play sound");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Audio playback failed: {ex.Message}");
        }
    }

    private void Cleanup()
    {
        try
        {
            // Dispose resources
            _sampleTexture?.Dispose();
            _sampleAudio?.Dispose();

            Console.WriteLine();
            Console.WriteLine("✓ Resources cleaned up");
            Console.WriteLine("Demo completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Cleanup error: {ex.Message}");
        }
    }

    private void OnKeyEvent(Key key, KeyboardKeyState.KeyEvent keyEvent)
    {
        // Handle ESC for exit
        if (key == Key.Escape && keyEvent == KeyboardKeyState.KeyEvent.Pressed)
        {
            Console.WriteLine("ESC pressed - exiting demo...");
            _engine?.WindowManager.NativeWindow.Close();
            return;
        }

        // Track key state for real-time zoom
        switch (keyEvent)
        {
            case KeyboardKeyState.KeyEvent.Pressed:
                _pressedKeys.Add(key);
                break;
            case KeyboardKeyState.KeyEvent.Released:
                _pressedKeys.Remove(key);
                break;
        }
    }

    private void UpdateCameraZoom(float deltaTime)
    {
        if (_engine?.CameraManager?.GameCamera == null) return;

        var camera = _engine.CameraManager.GameCamera;
        var currentZoom = camera.Zoom;
        var zoomChange = 0f;

        // Handle zoom controls (matching PupperQuest pattern)
        if (_pressedKeys.Contains(Key.E) || _pressedKeys.Contains(Key.KeypadAdd) || _pressedKeys.Contains(Key.Equal))
            zoomChange += ZoomSpeed * deltaTime;  // E/+ zooms in
        if (_pressedKeys.Contains(Key.Q) || _pressedKeys.Contains(Key.KeypadSubtract) || _pressedKeys.Contains(Key.Minus))
            zoomChange -= ZoomSpeed * deltaTime;  // Q/- zooms out

        if (zoomChange != 0f)
        {
            var newZoom = Math.Clamp(currentZoom + zoomChange, MinZoom, MaxZoom);
            camera.Zoom = newZoom;
            Console.WriteLine($"📷 Camera Zoom: {camera.Zoom:F2}x (Q/- to zoom out, E/+ to zoom in)");
        }
    }
}
