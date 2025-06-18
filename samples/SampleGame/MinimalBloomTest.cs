using Rac.Core.Extension;
using Rac.Core.Manager;
using Rac.Engine;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Rendering;
using Rac.Rendering.Shader;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace SampleGame;

/// <summary>
/// Minimal bloom test to debug bloom effect issues with camera system.
/// Tests both UI camera (identity-like) and game camera to isolate the problem.
/// </summary>
public static class MinimalBloomTest
{
    public static void Run(string[] args)
    {
        var windowManager = new WindowManager();
        var inputService = new SilkInputService();
        var configurationManager = new ConfigManager();
        var engine = new EngineFacade(windowManager, inputService, configurationManager);

        bool useUICamera = true; // Toggle between UI and Game camera
        bool bloomEnabled = true;

        engine.KeyEvent += (key, keyEvent) =>
        {
            if (keyEvent == KeyboardKeyState.KeyEvent.Pressed)
            {
                switch (key)
                {
                    case Key.C:
                        useUICamera = !useUICamera;
                        Console.WriteLine($"Switched to {(useUICamera ? "UI Camera (identity-like)" : "Game Camera (with transformations)")}");
                        break;
                    case Key.B:
                        bloomEnabled = !bloomEnabled;
                        Console.WriteLine($"Bloom: {(bloomEnabled ? "ON" : "OFF")}");
                        break;
                }
            }
        };

        engine.RenderEvent += deltaSeconds =>
        {
            // Clear screen
            engine.Renderer.Clear();
            
            // Choose camera based on toggle
            if (useUICamera)
            {
                engine.Renderer.SetActiveCamera(engine.CameraManager.UICamera);
            }
            else
            {
                // Reset game camera to known state for testing
                var gameCamera = engine.CameraManager.GameCamera;
                gameCamera.Position = Vector2D<float>.Zero;
                gameCamera.Zoom = 1.0f;
                gameCamera.Rotation = 0.0f;
                engine.Renderer.SetActiveCamera(gameCamera);
            }
            
            // Set shader mode
            engine.Renderer.SetShaderMode(bloomEnabled ? ShaderMode.Bloom : ShaderMode.Normal);
            
            // Draw simple bright shape at origin
            engine.Renderer.SetColor(new Vector4D<float>(3.0f, 1.0f, 1.0f, 1.0f)); // Bright red HDR
            
            // Simple quad at origin - same coordinates as original bloom test
            var vertices = new float[]
            {
                -0.15f, -0.15f,  // Bottom-left
                 0.15f, -0.15f,  // Bottom-right
                 0.15f,  0.15f,  // Top-right
                -0.15f,  0.15f   // Top-left
            };
            
            engine.Renderer.UpdateVertices(vertices);
            engine.Renderer.SetPrimitiveType(PrimitiveType.TriangleFan);
            engine.Renderer.Draw();
            
            // Apply post-processing
            engine.Renderer.FinalizeFrame();
        };

        Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                            MINIMAL BLOOM TEST                               ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("This test isolates bloom effect issues by comparing:");
        Console.WriteLine("• UI Camera (identity-like matrix) vs Game Camera (with transformations)");
        Console.WriteLine("• Bloom ON vs Bloom OFF to see if glow/bleeding occurs");
        Console.WriteLine();
        Console.WriteLine("🎮 CONTROLS:");
        Console.WriteLine("   C: Toggle between UI Camera and Game Camera");
        Console.WriteLine("   B: Toggle Bloom ON/OFF");
        Console.WriteLine("   ESC: Exit");
        Console.WriteLine();
        Console.WriteLine($"🔧 Current: Camera={{{(useUICamera ? "UI" : "Game")}}}, Bloom={{{(bloomEnabled ? "ON" : "OFF")}}}");
        Console.WriteLine();
        Console.WriteLine("📋 EXPECTED RESULTS:");
        Console.WriteLine("   • Bloom ON: Should see bright red square with glowing halo/bleeding edges");
        Console.WriteLine("   • Bloom OFF: Should see normal bright red square without glow");
        Console.WriteLine("   • If camera type affects bloom, we've found the issue!");
        Console.WriteLine();

        engine.Run();
    }
}