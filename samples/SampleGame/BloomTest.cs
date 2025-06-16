// ════════════════════════════════════════════════════════════════════════════════
// BLOOM HDR TEST DEMONSTRATION
// ════════════════════════════════════════════════════════════════════════════════
//
// This test demonstrates the HDR (High Dynamic Range) color capabilities of the bloom
// effect system. It showcases how colors with values > 1.0 create dramatic glow effects
// that are characteristic of true bloom rendering.
//
// KEY FEATURES DEMONSTRATED:
// - HDR color input (red: 2.5, white: 2.0+, blue: 2.5 in respective channels)
// - Automatic bloom intensity calculation based on HDR values
// - Color temperature shifting for enhanced visual appeal
// - Energy distribution across bloom radius for realistic light scattering
//
// USAGE:
// Run this test to see side-by-side comparison of:
// 1. Standard LDR colors (subtle or no bloom)
// 2. HDR colors (dramatic bloom effects with bright halos)
//
// This validates that the issue #51 requirements are met:
// "Red objects with HDR colors (e.g., 2.5, 0.3, 0.3, 1.0) should bloom dramatically"
// ════════════════════════════════════════════════════════════════════════════════

using Rac.Core.Extension;
using Rac.Core.Manager;
using Rac.ECS.Component;
using Rac.Engine;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Rendering;
using Rac.Rendering.Shader;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace SampleGame;

/// <summary>
/// Dedicated bloom test showcasing HDR color effects for optimal visual results.
/// Demonstrates the requirements from issue #51 for HDR bloom color support.
/// </summary>
public static class BloomTest
{
    public static void Run(string[] args)
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // ENGINE SETUP FOR BLOOM DEMONSTRATION
        // ═══════════════════════════════════════════════════════════════════════════
        
        var windowManager = new WindowManager();
        var inputService = new SilkInputService();
        var configurationManager = new ConfigManager();
        var engine = new EngineFacade(windowManager, inputService, configurationManager);

        // Start in bloom mode to immediately demonstrate HDR effects
        var currentShaderMode = ShaderMode.Bloom;
        var showHDRColors = true;

        // ═══════════════════════════════════════════════════════════════════════════
        // HDR COLOR EXAMPLES FOR BLOOM EFFECTS
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // These examples demonstrate the exact requirements from issue #51:
        // - Red objects with HDR colors should bloom dramatically
        // - White objects with HDR colors should show strong bloom effects  
        // - Dim objects should show no bloom effect

        var ldrColors = new Dictionary<string, Vector4D<float>>
        {
            ["Red"] = new(1.0f, 0.0f, 0.0f, 1.0f),       // Standard red (subtle bloom)
            ["White"] = new(1.0f, 1.0f, 1.0f, 1.0f),     // Standard white (moderate bloom)
            ["Blue"] = new(0.0f, 0.0f, 1.0f, 1.0f),      // Standard blue (subtle bloom)
            ["Dim"] = new(0.3f, 0.3f, 0.3f, 1.0f),       // Dim gray (no bloom expected)
        };

        var hdrColors = new Dictionary<string, Vector4D<float>>
        {
            ["Red"] = new(2.5f, 0.3f, 0.3f, 1.0f),       // HDR red - dramatic red bloom
            ["White"] = new(2.0f, 2.0f, 2.0f, 1.0f),     // HDR white - strong bright bloom
            ["Blue"] = new(0.3f, 0.3f, 2.5f, 1.0f),      // HDR blue - dramatic blue bloom
            ["Dim"] = new(0.3f, 0.3f, 0.3f, 1.0f),       // Still dim (no bloom expected)
        };

        // ═══════════════════════════════════════════════════════════════════════════
        // INTERACTIVE CONTROLS FOR COMPARISON
        // ═══════════════════════════════════════════════════════════════════════════

        engine.KeyEvent += (key, keyEvent) =>
        {
            if (keyEvent == KeyboardKeyState.KeyEvent.Pressed)
            {
                switch (key)
                {
                    case Key.S: // Toggle bloom mode (primary as per issue #52)
                    case Key.B: // Toggle bloom mode (legacy support)
                        currentShaderMode = currentShaderMode == ShaderMode.Bloom ? ShaderMode.Normal : ShaderMode.Bloom;
                        Console.WriteLine($"Shader Mode: {currentShaderMode}");
                        break;
                        
                    case Key.H: // Toggle HDR colors
                        showHDRColors = !showHDRColors;
                        Console.WriteLine($"HDR Colors: {(showHDRColors ? "ON (dramatic bloom)" : "OFF (subtle bloom)")}");
                        break;
                }
            }
        };

        // ═══════════════════════════════════════════════════════════════════════════
        // MAIN RENDERING LOOP
        // ═══════════════════════════════════════════════════════════════════════════

        engine.RenderEvent += deltaSeconds =>
        {
            DrawBloomDemonstrationShapes();
        };

        // ═══════════════════════════════════════════════════════════════════════════
        // STARTUP INSTRUCTIONS
        // ═══════════════════════════════════════════════════════════════════════════
        
        Console.WriteLine("=== BLOOM HDR COLOR TEST - ISSUE #51 DEMONSTRATION ===");
        Console.WriteLine("Controls:");
        Console.WriteLine("  'S' - Toggle Bloom mode ON/OFF");
        Console.WriteLine("  'B' - Toggle Bloom mode ON/OFF (legacy)");
        Console.WriteLine("  'H' - Toggle HDR colors ON/OFF");
        Console.WriteLine();
        Console.WriteLine("Expected behavior:");
        Console.WriteLine("- HDR ON + Bloom ON = Dramatic bloom effects with bright halos");
        Console.WriteLine("- HDR OFF + Bloom ON = Subtle bloom effects");
        Console.WriteLine("- Any mode + Bloom OFF = No bloom effects");
        Console.WriteLine();
        Console.WriteLine($"Current Mode: {currentShaderMode}");
        Console.WriteLine($"HDR Colors: {(showHDRColors ? "ON" : "OFF")}");

        engine.Run();

        // ═══════════════════════════════════════════════════════════════════════════
        // LOCAL HELPER FUNCTIONS
        // ═══════════════════════════════════════════════════════════════════════════

        void DrawBloomDemonstrationShapes()
        {
            var currentColors = showHDRColors ? hdrColors : ldrColors;
            
            // Draw test shapes in a grid pattern to compare effects
            var positions = new Dictionary<string, Vector2D<float>>
            {
                ["Red"] = new(-0.6f, 0.6f),    // Top left
                ["White"] = new(0.6f, 0.6f),   // Top right  
                ["Blue"] = new(-0.6f, -0.6f),  // Bottom left
                ["Dim"] = new(0.6f, -0.6f),    // Bottom right
            };

            foreach (var (colorName, position) in positions)
            {
                DrawTestSquare(position, currentColors[colorName], colorName);
            }
        }

        void DrawTestSquare(Vector2D<float> center, Vector4D<float> color, string label)
        {
            const float size = 0.3f; // Square size
            var halfSize = size * 0.5f;
            
            // Generate square geometry as two triangles
            var verts = new List<float>
            {
                // First triangle (top-left, top-right, bottom-left)
                center.X - halfSize, center.Y + halfSize,  // Top-left
                center.X + halfSize, center.Y + halfSize,  // Top-right
                center.X - halfSize, center.Y - halfSize,  // Bottom-left
                
                // Second triangle (top-right, bottom-right, bottom-left)
                center.X + halfSize, center.Y + halfSize,  // Top-right
                center.X + halfSize, center.Y - halfSize,  // Bottom-right
                center.X - halfSize, center.Y - halfSize,  // Bottom-left
            };

            // Render with current shader mode and color
            engine.Renderer.SetShaderMode(currentShaderMode);
            engine.Renderer.SetColor(color);
            engine.Renderer.UpdateVertices(verts.ToArray());
            engine.Renderer.Draw();
        }
    }
}