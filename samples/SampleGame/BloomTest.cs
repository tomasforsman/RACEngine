// ═══════════════════════════════════════════════════════════════════════════════
// EDUCATIONAL BLOOM HDR EFFECT DEMONSTRATION
// ═══════════════════════════════════════════════════════════════════════════════
//
// This educational demonstration showcases High Dynamic Range (HDR) bloom effects,
// a fundamental post-processing technique in modern computer graphics and game engines.
//
// CORE GRAPHICS CONCEPTS DEMONSTRATED:
//
// 1. HIGH DYNAMIC RANGE (HDR) COLOR THEORY:
//    - LDR (Low Dynamic Range): Color values clamped to [0.0, 1.0]
//    - HDR (High Dynamic Range): Color values can exceed 1.0 (real-world luminance)
//    - Tone mapping: HDR → LDR conversion for display devices
//    - Perceptual accuracy: HDR better represents human vision and real lighting
//
// 2. BLOOM EFFECT ALGORITHM (Multi-pass rendering):
//    - Bright extraction: Isolate pixels above luminance threshold
//    - Gaussian blur: Create smooth light scattering effect
//    - Additive blending: Composite bloom over original scene
//    - Result: Realistic camera lens glow and light bleeding
//
// 3. COLOR SPACE MATHEMATICS:
//    - Luminance calculation: Y = 0.299*R + 0.587*G + 0.114*B (ITU-R BT.601)
//    - RGB → HSV conversions for brightness analysis
//    - Gamma correction: Linear → sRGB color space conversion
//    - Energy conservation: Maintaining color relationships during processing
//
// 4. REAL-TIME GRAPHICS PIPELINE:
//    - Framebuffer objects: Render-to-texture for multi-pass effects
//    - Ping-pong rendering: Alternating textures for iterative processing
//    - Shader uniform management: Dynamic parameter passing to GPU
//    - Texture filtering: Bilinear/trilinear sampling for smooth results
//
// 5. COMPARATIVE VISUAL ANALYSIS:
//    - Side-by-side LDR vs HDR comparison
//    - Interactive mode switching for educational comparison
//    - Color intensity progression: Dim → Bright → Bloom threshold
//    - Scientific validation of graphics algorithms in real-time
//
// CONTROLS & INTERACTION:
// - 'B': Toggle bloom effect ON/OFF
// - 'H': Toggle HDR color mode (demonstrates dramatic vs subtle effects)
// - WASD: Camera movement (pan world view)
// - Q/E: Camera zoom out/in
// - R: Reset camera to origin
// - Tab: Toggle UI overlay visibility
// - Mouse Click: Spawn objects at world coordinates
//
// EXPECTED LEARNING OUTCOMES:
// - Understanding HDR color spaces and their visual impact
// - Recognizing bloom threshold behavior and intensity scaling
// - Appreciating the mathematical foundation of modern graphics effects
// - Connecting theory to practical implementation in game engines
//
// ═══════════════════════════════════════════════════════════════════════════════

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
using Silk.NET.OpenGL;

namespace SampleGame;

/// <summary>
/// Educational bloom effect demonstration showcasing HDR color theory and post-processing.
///
/// EDUCATIONAL OBJECTIVES:
/// - Demonstrate HDR vs LDR color space differences through interactive comparison
/// - Illustrate bloom algorithm stages (bright extraction, blur, composite)
/// - Provide hands-on experience with real-time graphics programming concepts
/// - Connect mathematical theory to practical visual results
///
/// TECHNICAL VALIDATION:
/// - Validates HDR bloom requirements from issue #51
/// - Tests color values exceeding 1.0 for dramatic bloom effects
/// - Demonstrates proper tone mapping and color space handling
/// - Ensures consistent behavior across different shader modes
///
/// GRAPHICS TECHNIQUES SHOWCASED:
/// - Multi-pass rendering pipeline
/// - Framebuffer object usage for render-to-texture
/// - Gaussian blur implementation through separable filters
/// - Additive blending for light accumulation effects
/// </summary>
public static class BloomTest
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CAMERA SYSTEM & UI INTEGRATION
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // Camera controls for interactive exploration and UI overlay management.
    // Demonstrates dual-camera rendering with world-space bloom shapes and screen-space UI.

    private static bool showUIOverlay = true;
    private static List<WorldObject> spawnedObjects = new();

    /// <summary>
    /// Represents a user-spawned object demonstrating coordinate transformation.
    /// Shows how screen coordinates (mouse clicks) map to world coordinates.
    /// </summary>
    private class WorldObject
    {
        public Vector2D<float> Position { get; set; }
        public float Size { get; set; } = 0.1f;
        public Vector4D<float> Color { get; set; } = new(1f, 0.8f, 0.2f, 1f); // Orange
    }

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
        // COMPARATIVE COLOR SCIENCE EXAMPLES
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // These color specifications demonstrate the fundamental difference between
        // LDR and HDR color spaces through carefully chosen values that highlight
        // bloom threshold behavior and luminance response curves.
        //
        // LDR Color Analysis:
        // - All values ≤ 1.0 (standard display range)
        // - Limited dynamic range for bright light representation
        // - Bloom effects minimal due to low luminance values
        // - Represents traditional (legacy) graphics rendering

        var ldrColors = new Dictionary<string, Vector4D<float>>
        {
            ["Red"] = new(1.0f, 0.0f, 0.0f, 1.0f),       // Peak red in LDR space
            ["White"] = new(1.0f, 1.0f, 1.0f, 1.0f),     // Peak white in LDR space
            ["Blue"] = new(0.0f, 0.0f, 1.0f, 1.0f),      // Peak blue in LDR space
            ["Dim"] = new(0.3f, 0.3f, 0.3f, 1.0f),       // Below bloom threshold
        };

        // HDR Color Analysis:
        // - Values > 1.0 represent super-bright light sources
        // - Extended dynamic range for realistic lighting simulation
        // - Dramatic bloom effects due to high luminance values
        // - Represents modern (physically-based) graphics rendering
        var hdrColors = new Dictionary<string, Vector4D<float>>
        {
            ["Red"] = new(2.5f, 0.3f, 0.3f, 1.0f),       // HDR red: Intense red with subdued other channels
            ["White"] = new(2.0f, 2.0f, 2.0f, 1.0f),     // HDR white: Uniform high intensity across all channels
            ["Blue"] = new(0.3f, 0.3f, 2.5f, 1.0f),      // HDR blue: Intense blue with subdued other channels
            ["Dim"] = new(0.3f, 0.3f, 0.3f, 1.0f),       // Still below bloom threshold (control case)
        };

        // ═══════════════════════════════════════════════════════════════════════════
        // INTERACTIVE CONTROLS FOR CAMERA AND COMPARISON
        // ═══════════════════════════════════════════════════════════════════════════

        engine.KeyEvent += (key, keyEvent) =>
        {
            if (keyEvent == KeyboardKeyState.KeyEvent.Pressed)
            {
                HandleKeyPress(key, engine, ref currentShaderMode, ref showHDRColors);
            }
        };

        // ─── Hook Mouse Input for Click-to-Spawn ────────────────
        engine.LeftClickEvent += screenPosition =>
        {
            HandleMouseClick(screenPosition, engine);
        };

        engine.MouseScrollEvent += delta =>
        {
            HandleMouseScroll(delta, engine);
        };

        // ═══════════════════════════════════════════════════════════════════════════
        // MAIN RENDERING LOOP
        // ═══════════════════════════════════════════════════════════════════════════

        engine.RenderEvent += deltaSeconds =>
        {
            // Clear the render target before rendering
            engine.Renderer.Clear();

            // ═══════════════════════════════════════════════════════════════════════════
            // PASS 1: RENDER GAME WORLD (with camera transformations)
            // ═══════════════════════════════════════════════════════════════════════════
            //
            // Set the game camera to apply world-space transformations (pan, zoom, rotate).
            // All objects rendered in this pass will be affected by camera movement.

            engine.Renderer.SetActiveCamera(engine.CameraManager.GameCamera);

            // Render background grid for visual reference
            DrawBackgroundGrid(engine);

            // Render user-spawned objects (click-to-spawn demonstration)
            DrawSpawnedObjects(engine, currentShaderMode, showHDRColors);

            // Render bloom demonstration shapes
            DrawBloomDemonstrationShapes(engine, currentShaderMode, showHDRColors);

            // ═══════════════════════════════════════════════════════════════════════════
            // PASS 2: RENDER UI OVERLAY (screen-space, camera-independent)
            // ═══════════════════════════════════════════════════════════════════════════
            //
            // Set the UI camera for screen-space rendering that remains fixed regardless
            // of game camera transformations. Perfect for HUD, menus, and debug information.

            if (showUIOverlay)
            {
                engine.Renderer.SetActiveCamera(engine.CameraManager.UICamera);
                DrawUIOverlay(engine, currentShaderMode, showHDRColors);
            }

            // Finalize the frame
            engine.Renderer.FinalizeFrame();
        };

        // ═══════════════════════════════════════════════════════════════════════════
        // STARTUP EDUCATIONAL GUIDANCE
        // ═══════════════════════════════════════════════════════════════════════════
        Console.OutputEncoding = System.Text.Encoding.Unicode;

        Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║               BLOOM HDR DEMONSTRATION - GRAPHICS EDUCATION                  ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("🎯 EDUCATIONAL OBJECTIVES:");
        Console.WriteLine("   • Understand HDR vs LDR color space differences");
        Console.WriteLine("   • Observe bloom algorithm effects in real-time");
        Console.WriteLine("   • Connect mathematical theory to visual results");
        Console.WriteLine("   • Experience interactive graphics programming");
        Console.WriteLine();
        Console.WriteLine("🎮 INTERACTIVE CONTROLS:");
        Console.WriteLine("   WASD:        Camera movement (pan world view)");
        Console.WriteLine("   Q/E:         Camera zoom out/in");
        Console.WriteLine("   R:           Reset camera to origin");
        Console.WriteLine("   Tab:         Toggle UI overlay visibility");
        Console.WriteLine("   Mouse Click: Spawn objects at world coordinates");
        Console.WriteLine("   B:           Toggle Bloom mode ON/OFF");
        Console.WriteLine("   H:           Toggle HDR colors ON/OFF (dramatic vs subtle)");
        Console.WriteLine();
        Console.WriteLine("🔬 TECHNICAL FEATURES DEMONSTRATED:");
        Console.WriteLine("   • Dual-camera system: Game world + UI overlay rendering");
        Console.WriteLine("   • Screen-to-world coordinate transformation via mouse clicks");
        Console.WriteLine("   • Camera controls: Pan, zoom, and reset functionality");
        Console.WriteLine("   • HDR vs LDR color space comparison");
        Console.WriteLine("   • Bloom algorithm with bright extraction and Gaussian blur");
        Console.WriteLine("   • Interactive real-time graphics programming concepts");
        Console.WriteLine();
        Console.WriteLine("📊 COLOR ANALYSIS:");
        Console.WriteLine("   • Red HDR (2.5, 0.3, 0.3): Demonstrates channel-specific intensity");
        Console.WriteLine("   • White HDR (2.0, 2.0, 2.0): Shows uniform high-intensity across all channels");
        Console.WriteLine("   • Blue HDR (0.3, 0.3, 2.5): Illustrates cool-tone HDR effects");
        Console.WriteLine("   • Dim LDR (0.3, 0.3, 0.3): Control case remaining below bloom threshold");
        Console.WriteLine();
        Console.WriteLine($"🚀 Current Configuration: Bloom={currentShaderMode}, HDR={showHDRColors}");
        Console.WriteLine();
        Console.WriteLine("💡 Watch for: Light bleeding, halo expansion, color saturation changes, and");
        Console.WriteLine("   intensity falloff patterns that demonstrate real-world optical phenomena!");
        Console.WriteLine();

        engine.Run();

        // ═══════════════════════════════════════════════════════════════════════════
        // LOCAL HELPER FUNCTIONS
        // ═══════════════════════════════════════════════════════════════════════════

        void DrawBloomDemonstrationShapes(EngineFacade engine, ShaderMode currentShaderMode, bool showHDRColors)
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
                DrawTestSquare(position, currentColors[colorName], colorName, engine, currentShaderMode);
            }
        }

        void DrawTestSquare(Vector2D<float> center, Vector4D<float> color, string label, EngineFacade engine, ShaderMode currentShaderMode)
        {
            // ───────────────────────────────────────────────────────────────────────
            // GEOMETRIC PRIMITIVE CONSTRUCTION
            // ───────────────────────────────────────────────────────────────────────
            //
            // Constructs a square using two triangles in counter-clockwise winding order.
            // This demonstrates basic 2D geometry tessellation for GPU rendering.

            const float size = 0.3f; // Square size in NDC coordinates
            var halfSize = size * 0.5f;

            // Triangle strip approach: Reuses vertices for efficiency
            // Triangle 1: Top-left → Top-right → Bottom-left
            // Triangle 2: Top-right → Bottom-right → Bottom-left
            var verts = new List<float>
            {
                // First triangle vertices (CCW winding)
                center.X - halfSize, center.Y + halfSize,  // Top-left vertex
                center.X + halfSize, center.Y + halfSize,  // Top-right vertex
                center.X - halfSize, center.Y - halfSize,  // Bottom-left vertex

                // Second triangle vertices (CCW winding)
                center.X + halfSize, center.Y + halfSize,  // Top-right vertex (shared)
                center.X + halfSize, center.Y - halfSize,  // Bottom-right vertex
                center.X - halfSize, center.Y - halfSize,  // Bottom-left vertex (shared)
            };

            // Apply current shader mode and color for demonstration
            engine.Renderer.SetShaderMode(currentShaderMode);
            engine.Renderer.SetColor(color);
            engine.Renderer.UpdateVertices(verts.ToArray());
            engine.Renderer.Draw();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // CAMERA CONTROL HANDLERS
        // ═══════════════════════════════════════════════════════════════════════════

        void HandleKeyPress(Key key, EngineFacade engine, ref ShaderMode currentShaderMode, ref bool showHDRColors)
        {
            // ─── Camera Movement Controls (WASD) ─────────────────────────────────────
            const float cameraSpeed = 0.1f;
            var camera = engine.CameraManager.GameCamera;
            
            switch (key)
            {
                case Key.W: // Move camera up
                    camera.Move(new Vector2D<float>(0f, cameraSpeed));
                    break;
                case Key.A: // Move camera left  
                    camera.Move(new Vector2D<float>(-cameraSpeed, 0f));
                    break;
                case Key.S: // Move camera down
                    camera.Move(new Vector2D<float>(0f, -cameraSpeed));
                    break;
                case Key.D: // Move camera right
                    camera.Move(new Vector2D<float>(cameraSpeed, 0f));
                    break;
                
                // ─── Camera Zoom Controls (Q/E) ─────────────────────────────────────
                case Key.Q: // Zoom out
                    camera.Zoom = Math.Max(0.1f, camera.Zoom - 0.1f);
                    break;
                case Key.E: // Zoom in
                    camera.Zoom = Math.Min(5f, camera.Zoom + 0.1f);
                    break;
                
                // ─── Bloom Toggle Controls (B) ──────────────────────────────────────
                case Key.B: // Toggle bloom mode
                    currentShaderMode = currentShaderMode == ShaderMode.Bloom ? ShaderMode.Normal : ShaderMode.Bloom;
                    Console.WriteLine($"Shader Mode: {currentShaderMode}");
                    break;
                
                // ─── Other Controls ─────────────────────────────────────────────────
                case Key.H: // Toggle HDR colors
                    showHDRColors = !showHDRColors;
                    Console.WriteLine($"HDR Colors: {(showHDRColors ? "ON (dramatic bloom)" : "OFF (subtle bloom)")}");
                    break;
                
                case Key.R: // Reset camera
                    camera.Position = Vector2D<float>.Zero;
                    camera.Zoom = 1f;
                    camera.Rotation = 0f;
                    Console.WriteLine("Camera reset to origin");
                    break;
                
                case Key.Tab: // Toggle UI overlay
                    showUIOverlay = !showUIOverlay;
                    Console.WriteLine($"UI overlay: {(showUIOverlay ? "ON" : "OFF")}");
                    break;
            }
        }

        void HandleMouseClick(Vector2D<float> screenPosition, EngineFacade engine)
        {
            // Convert screen coordinates to world coordinates using camera manager
            var windowSize = engine.WindowManager.Size;
            var worldPosition = engine.CameraManager.ScreenToGameWorld(
                screenPosition, 
                windowSize.X, 
                windowSize.Y
            );

            // Spawn a new object at the clicked world position
            spawnedObjects.Add(new WorldObject
            {
                Position = worldPosition,
                Size = 0.1f,
                Color = new Vector4D<float>(1f, 0.8f, 0.2f, 1f) // Orange
            });

            Console.WriteLine($"Spawned object at world position: ({worldPosition.X:F2}, {worldPosition.Y:F2})");
        }

        void HandleMouseScroll(float delta, EngineFacade engine)
        {
            var camera = engine.CameraManager.GameCamera;
            
            // Zoom with mouse wheel
            const float zoomSensitivity = 0.1f;
            float zoomDelta = delta * zoomSensitivity;
            
            // Apply zoom with limits
            camera.Zoom = Math.Max(0.1f, Math.Min(10f, camera.Zoom + zoomDelta));
        }

        void DrawBackgroundGrid(EngineFacade engine)
        {
            // ═══════════════════════════════════════════════════════════════════════════
            // BACKGROUND REFERENCE GRID
            // ═══════════════════════════════════════════════════════════════════════════
            //
            // Provides visual reference for camera movement and world coordinate system.
            // Grid remains in world space, so it moves with camera transformations.

            const float majorGridSize = 4f;
            const float majorGridSpacing = 1f;
            const float minorGridSpacing = 0.2f;
            var gridVertices = new List<float>();

            // Major grid lines (every 1 unit) - slightly more visible
            for (float x = -majorGridSize; x <= majorGridSize; x += majorGridSpacing)
            {
                gridVertices.AddRange(new[] { x, -majorGridSize, x, majorGridSize });
            }
            for (float y = -majorGridSize; y <= majorGridSize; y += majorGridSpacing)
            {
                gridVertices.AddRange(new[] { -majorGridSize, y, majorGridSize, y });
            }

            // Render major grid lines with subtle but visible color
            engine.Renderer.SetShaderMode(ShaderMode.Normal);
            engine.Renderer.SetPrimitiveType(PrimitiveType.Lines);
            engine.Renderer.SetColor(new Vector4D<float>(0.4f, 0.4f, 0.4f, 0.8f));
            engine.Renderer.UpdateVertices(gridVertices.ToArray());
            engine.Renderer.Draw();

            // Minor grid lines (every 0.2 units) - very subtle
            gridVertices.Clear();
            for (float x = -majorGridSize; x <= majorGridSize; x += minorGridSpacing)
            {
                if (x % majorGridSpacing != 0) // Skip major grid line positions
                {
                    gridVertices.AddRange(new[] { x, -majorGridSize, x, majorGridSize });
                }
            }
            for (float y = -majorGridSize; y <= majorGridSize; y += minorGridSpacing)
            {
                if (y % majorGridSpacing != 0) // Skip major grid line positions
                {
                    gridVertices.AddRange(new[] { -majorGridSize, y, majorGridSize, y });
                }
            }

            // Render minor grid lines with very subtle color
            engine.Renderer.SetColor(new Vector4D<float>(0.25f, 0.25f, 0.25f, 0.4f));
            engine.Renderer.UpdateVertices(gridVertices.ToArray());
            engine.Renderer.Draw();

            // Reset to triangles for other objects
            engine.Renderer.SetPrimitiveType(PrimitiveType.Triangles);
        }

        void DrawSpawnedObjects(EngineFacade engine, ShaderMode currentShaderMode, bool showHDRColors)
        {
            // ═══════════════════════════════════════════════════════════════════════════
            // CLICK-TO-SPAWN OBJECTS DEMONSTRATION
            // ═══════════════════════════════════════════════════════════════════════════
            //
            // These objects demonstrate coordinate transformation from screen space to world space.
            // Each object is positioned at the world coordinates corresponding to mouse click position.

            if (spawnedObjects.Count == 0)
                return;

            var vertexBuffer = new List<float>();

            foreach (var obj in spawnedObjects)
            {
                // Render each spawned object as a small quad
                float halfSize = obj.Size * 0.5f;
                vertexBuffer.AddRange(new[]
                {
                    // Triangle 1
                    obj.Position.X - halfSize, obj.Position.Y - halfSize,
                    obj.Position.X + halfSize, obj.Position.Y - halfSize,
                    obj.Position.X + halfSize, obj.Position.Y + halfSize,
                    
                    // Triangle 2
                    obj.Position.X - halfSize, obj.Position.Y - halfSize,
                    obj.Position.X + halfSize, obj.Position.Y + halfSize,
                    obj.Position.X - halfSize, obj.Position.Y + halfSize,
                });
            }

            // Render all spawned objects with current shader mode
            engine.Renderer.SetShaderMode(currentShaderMode);
            engine.Renderer.SetColor(new Vector4D<float>(1f, 0.5f, 0.2f, 1f)); // Orange
            engine.Renderer.UpdateVertices(vertexBuffer.ToArray());
            engine.Renderer.Draw();
        }

        void DrawUIOverlay(EngineFacade engine, ShaderMode currentShaderMode, bool showHDRColors)
        {
            // ═══════════════════════════════════════════════════════════════════════════
            // SCREEN-SPACE UI OVERLAY DEMONSTRATION
            // ═══════════════════════════════════════════════════════════════════════════
            //
            // This UI remains fixed in screen space regardless of camera transformations.
            // Uses geometric shapes as placeholders for text-based information display.

            var camera = engine.CameraManager.GameCamera;
            
            // UI Panel background (top-left corner)
            DrawUIQuad(-380f, 250f, 200f, 120f, new Vector4D<float>(0.1f, 0.1f, 0.3f, 0.8f), engine);

            // Camera position indicators (colored bars representing X and Y)
            float posX = Math.Clamp(camera.Position.X * 50f, -80f, 80f);
            float posY = Math.Clamp(camera.Position.Y * 50f, -80f, 80f);
            
            DrawUIQuad(-350f, 220f, posX, 10f, new Vector4D<float>(1f, 0f, 0f, 1f), engine); // X position (red)
            DrawUIQuad(-350f, 200f, posY, 10f, new Vector4D<float>(0f, 1f, 0f, 1f), engine); // Y position (green)

            // Zoom level indicator (horizontal bar)
            float zoomBarWidth = camera.Zoom * 60f;
            DrawUIQuad(-350f, 180f, zoomBarWidth, 8f, new Vector4D<float>(0f, 0f, 1f, 1f), engine); // Zoom (blue)

            // Shader mode indicator
            DrawUIQuad(-350f, 160f, 20f, 8f, currentShaderMode == ShaderMode.Bloom ? 
                new Vector4D<float>(1f, 1f, 0f, 1f) : new Vector4D<float>(0.5f, 0.5f, 0.5f, 1f), engine); // Yellow if bloom

            // HDR mode indicator  
            DrawUIQuad(-350f, 145f, 20f, 8f, showHDRColors ? 
                new Vector4D<float>(1f, 0.5f, 1f, 1f) : new Vector4D<float>(0.5f, 0.5f, 0.5f, 1f), engine); // Magenta if HDR

            // Controls indicator (small rectangles)
            DrawUIQuad(-380f, 125f, 15f, 5f, new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f), engine); // "WASD: Camera"
            DrawUIQuad(-380f, 115f, 15f, 5f, new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f), engine); // "Q/E: Zoom"
            DrawUIQuad(-380f, 105f, 15f, 5f, new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f), engine); // "S/B: Bloom"
            DrawUIQuad(-380f, 95f, 15f, 5f, new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f), engine); // "H: HDR"

            // Crosshair at screen center
            DrawUICrosshair(engine);
        }

        void DrawUIQuad(float x, float y, float width, float height, Vector4D<float> color, EngineFacade engine)
        {
            var vertices = new float[]
            {
                x, y,                    // Bottom-left
                x + width, y,           // Bottom-right  
                x + width, y + height,  // Top-right
                
                x, y,                   // Bottom-left
                x + width, y + height,  // Top-right
                x, y + height,          // Top-left
            };

            engine.Renderer.SetShaderMode(ShaderMode.Normal);
            engine.Renderer.SetColor(color);
            engine.Renderer.UpdateVertices(vertices);
            engine.Renderer.Draw();
        }

        void DrawUICrosshair(EngineFacade engine)
        {
            var crosshairVertices = new float[]
            {
                -20f, 0f, 20f, 0f,    // Horizontal line
                0f, -20f, 0f, 20f     // Vertical line
            };

            engine.Renderer.SetShaderMode(ShaderMode.Normal);
            engine.Renderer.SetColor(new Vector4D<float>(1f, 1f, 1f, 0.7f));
            engine.Renderer.UpdateVertices(crosshairVertices);
            engine.Renderer.Draw();
        }
    }
}
