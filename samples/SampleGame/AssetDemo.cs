using Rac.Assets;
using Rac.Assets.FileSystem;
using Rac.Engine;
using Rac.Assets.Types;
using Rac.Rendering;
using Rac.Rendering.Shader;
using Rac.Core.Manager;
using Rac.Input.Service;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace SampleGame;

/// <summary>
/// Asset Demo showcasing texture and audio loading using the Engine facade.
///
/// EDUCATIONAL PURPOSE:
/// This demo demonstrates the simplest asset loading patterns using the Engine facade:
/// - Layer 1 asset loading (engine.LoadTexture, engine.LoadAudio)
/// - Visual feedback with textured rendering
/// - Interactive audio playback on mouse clicks
/// - Error handling and graceful fallbacks
///
/// INTERACTION:
/// - Displays a textured square using SampleTexture.png
/// - Click anywhere in the window to play SampleAudio.wav
/// - Press ESC to exit the demo
/// </summary>
public class AssetDemo
{
    private IEngineFacade? _engine;
    private Texture? _sampleTexture;
    private AudioClip? _sampleAudio;

    // Square geometry for texture rendering with proper UV coordinates
    private readonly TexturedVertex[] _squareVertices = new[]
    {
        new TexturedVertex(new Vector2D<float>(-100, -100), new Vector2D<float>(0, 1)), // Bottom-left (UV: 0,1)
        new TexturedVertex(new Vector2D<float>( 100, -100), new Vector2D<float>(1, 1)), // Bottom-right (UV: 1,1)
        new TexturedVertex(new Vector2D<float>( 100,  100), new Vector2D<float>(1, 0)), // Top-right (UV: 1,0)
        new TexturedVertex(new Vector2D<float>(-100,  100), new Vector2D<float>(0, 0))  // Top-left (UV: 0,0)
    };

    public static void Run(string[] args)
    {
        Console.WriteLine("RACEngine Asset System Demo");
        Console.WriteLine("===========================");
        Console.WriteLine("Loading texture and audio assets using Engine facade...");
        Console.WriteLine("Click in the window to play audio, press ESC to exit.");
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

        // Handle key presses for exit
        _engine.KeyEvent += (key, keyEvent) =>
        {
            if (key == Silk.NET.Input.Key.Escape && keyEvent == Rac.Input.State.KeyboardKeyState.KeyEvent.Pressed)
            {
                Console.WriteLine("ESC pressed - exiting demo...");
                _engine.WindowManager.NativeWindow.Close();
            };
        };

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
            // Clear the screen
            _engine.Renderer.Clear();

            // Set shader mode for rendering (this was the missing piece!)
            _engine.Renderer.SetShaderMode(ShaderMode.Normal);

            // Render textured square in the center of the screen
            if (_sampleTexture != null)
            {
                // Set texture for rendering
                _engine.Renderer.SetTexture(_sampleTexture);
                _engine.Renderer.SetColor(new Vector4D<float>(1.0f, 1.0f, 1.0f, 1.0f)); // Use white to not tint the texture
            }
            else
            {
                // Fallback color if texture failed to load
                _engine.Renderer.SetColor(new Vector4D<float>(0.8f, 0.2f, 0.2f, 1.0f)); // Red color to indicate missing texture
            }

            // Render the square
            _engine.Renderer.UpdateVertices(_squareVertices);
            _engine.Renderer.Draw();

            // Display status text (this would typically use a text rendering system)
            // For now, the console output provides the information
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Rendering error: {ex.Message}");
        }
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
}
