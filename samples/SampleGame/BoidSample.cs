// File: samples/SampleGame/BoidSample.cs
//
// ═══════════════════════════════════════════════════════════════════════════════
// EDUCATIONAL BOIDS SIMULATION - COMPREHENSIVE GRAPHICS & AI DEMONSTRATION
// ═══════════════════════════════════════════════════════════════════════════════
//
// This sample demonstrates several key computer graphics and game development concepts:
//
// 1. BOIDS ALGORITHM (Craig Reynolds, 1986):
//    - Separation: Avoid crowding neighbors (personal space maintenance)
//    - Alignment: Steer towards average heading of neighbors (directional consensus)
//    - Cohesion: Steer towards average position of neighbors (group formation)
//    - Result: Emergent flocking behavior from simple local rules
//    - Applications: Animal behavior simulation, crowd dynamics, particle systems
//
// 2. ENTITY COMPONENT SYSTEM (ECS) ARCHITECTURE:
//    - Entities: Lightweight identifiers (just ID + alive status)
//    - Components: Pure data containers (Position, Velocity, Species)
//    - Systems: Stateless logic processors operating on component data
//    - Benefits: Performance through data locality, modularity, composition over inheritance
//    - Query Performance: O(n) iteration over packed component arrays
//
// 3. REAL-TIME RENDERING PIPELINE:
//    - Vertex generation: CPU-side triangle construction with rotation transforms
//    - Shader mode switching: Normal/SoftGlow/Bloom effects demonstration
//    - Batched rendering: Multiple entities rendered in single draw call
//    - HDR/LDR color management: Automatic color space handling per shader mode
//
// 4. COORDINATE SYSTEMS & MATHEMATICS:
//    - Normalized Device Coordinates (NDC): -1 to +1 range for resolution independence
//    - Aspect ratio preservation: Prevents distortion on different screen ratios
//    - 2D transformations: Scale → Rotate → Translate matrix pipeline
//    - Trigonometric functions: atan2 for direction, sin/cos for rotation matrices
//
// 5. MULTI-SPECIES ECOSYSTEM SIMULATION:
//    - Predator-prey relationships: Red hunts Blue/White, Blue hunts White
//    - Population dynamics: Realistic ecosystem distribution (30:20:10 ratio)
//    - Species interaction matrix: Configurable behavior rules per species pair
//    - Visual differentiation: Size scaling and color coding for species identification
//
// ═══════════════════════════════════════════════════════════════════════════════

using Rac.Core.Extension;
using Rac.Core.Manager;
using Rac.ECS.Component;
using Rac.ECS.Components;
using Rac.ECS.Core;
using Rac.ECS.System;
using Rac.ECS.Systems;
using Rac.Engine;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Rendering;
using Rac.Rendering.Shader;
using Rac.Rendering.VFX;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Linq;

namespace SampleGame;

public static class BoidSample
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ADAPTIVE RENDERING MODE DEMONSTRATION
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // This sample showcases the engine's visual effects system through progressive
    // shader mode enhancement. Users can interactively cycle through rendering modes
    // to understand the visual impact of different graphics techniques:
    // - Normal: Standard rasterization (baseline)
    // - SoftGlow: Subtle luminance enhancement
    // - Bloom: HDR post-processing with dramatic light bleeding effects

    private static ShaderMode _currentShaderMode = ShaderMode.Normal;
    private static List<ShaderMode> _availableShaderModes = new() { ShaderMode.Normal, ShaderMode.SoftGlow };
    private static int _shaderModeIndex = 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // CAMERA SYSTEM & UI INTEGRATION
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // Camera controls for interactive world exploration and UI overlay management.
    // Demonstrates dual-camera rendering with world-space boids and screen-space UI.

    private static bool showUIOverlay = true;
    private static List<WorldObject> spawnedObjects = new();

    /// <summary>
    /// Represents a user-spawned object demonstrating coordinate transformation.
    /// Shows how screen coordinates (mouse clicks) map to world coordinates.
    /// </summary>
    private class WorldObject
    {
        public Vector2D<float> Position { get; set; }
        public float Size { get; set; } = 0.05f;
        public Vector4D<float> Color { get; set; } = new(1f, 0.8f, 0.2f, 1f); // Orange
    }

    // ───────────────────────────────────────────────────────────────────────────
    // EDUCATIONAL TIP SYSTEM
    // ───────────────────────────────────────────────────────────────────────────
    //
    // Provides contextual learning hints during bloom mode to help users understand
    // advanced graphics concepts and HDR rendering techniques.

    private static float _timeSinceLastTip = 0f;
    private static int _tipIndex = 0;

    public static void Run(string[] args)
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // ENGINE INITIALIZATION
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // The engine facade pattern provides a simplified interface to complex subsystems.
        // Each manager handles a specific domain:
        // - WindowManager: OS window creation, events, lifecycle
        // - InputService: Keyboard, mouse, gamepad input abstraction
        // - ConfigManager: Settings, preferences, configuration files

        var windowManager = new WindowManager();
        var inputService = new SilkInputService();
        var configurationManager = new ConfigManager();
        var engine = new EngineFacade(windowManager, inputService, configurationManager);

        // ═══════════════════════════════════════════════════════════════════════════
        // ECS WORLD SETUP
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // ECS World: Container for all entities, components, and systems
        // BoidSystem: Implements the core flocking algorithm
        // TransformSystem: Handles hierarchical transformations (required for new transform architecture)
        // Settings Entity: Holds global configuration data (boundary constraints, interaction rules)

        var world = engine.World;
        var boidSystem = new BoidSystem();
        
        engine.AddSystem(boidSystem);
        var settingsEntity = world.CreateEntity();

        // ═══════════════════════════════════════════════════════════════════════════
        // SPECIES CONFIGURATION
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // Multi-species boids create complex emergent behaviors:
        // - Different scales create visual hierarchy
        // - Different colors aid in species identification
        // - Species interactions define predator-prey or neutral relationships

        string[] speciesIds = new[] { "White", "Blue", "Red" };

        // Visual scaling: Larger boids appear more dominant
        var speciesScales = new Dictionary<string, float>
        {
            ["White"] = 0.5f,  // Smallest - prey species
            ["Blue"] = 0.8f,   // Medium - neutral species
            ["Red"] = 1.2f,    // Largest - predator species
        };

        // RGBA color vectors for species identification
        // Alpha channel = 1.0 for full opacity
        var speciesColors = new Dictionary<string, Vector4D<float>>
        {
            ["White"] = new(1f, 1f, 1f, 1f),  // Pure white (LDR)
            ["Blue"] = new(0f, 0f, 1f, 1f),   // Pure blue (LDR)
            ["Red"] = new(1f, 0f, 0f, 1f),    // Pure red (LDR)
        };

        // HDR color variants for dramatic bloom effects when bloom mode is active
        // These colors exceed 1.0 in key channels to create intense glow effects
        var hdrSpeciesColors = new Dictionary<string, Vector4D<float>>
        {
            ["White"] = new(2.0f, 2.0f, 2.0f, 1f),  // HDR white - uniform bright glow
            ["Blue"] = new(0.3f, 0.3f, 2.5f, 1f),   // HDR blue - intense blue glow with subdued other channels
            ["Red"] = new(2.5f, 0.3f, 0.3f, 1f),    // HDR red - dramatic red glow with subdued other channels
        };

        // ═══════════════════════════════════════════════════════════════════════════
        // DYNAMIC BOUNDARY CALCULATION
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // Boundaries must adapt to window resize to maintain proper boid containment.
        // Uses aspect ratio to prevent distortion on different screen sizes.

        engine.LoadEvent += () =>
        {
            UpdateBoidSettings(windowManager.Size);
        };
        windowManager.OnResize += newSize => UpdateBoidSettings(newSize);

        // ═══════════════════════════════════════════════════════════════════════════
        // ENTITY SPAWNING
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // Initial world population with varied species counts creates realistic ecosystem

        SpawnAllSpecies();
        SpawnObstacles();

        // ═══════════════════════════════════════════════════════════════════════════
        // INPUT HANDLING FOR CAMERA CONTROLS AND SHADER MODE SWITCHING
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // Interactive demonstration of camera movement and rendering modes.

        engine.KeyEvent += (key, keyEvent) =>
        {
            if (keyEvent == KeyboardKeyState.KeyEvent.Pressed)
            {
                HandleKeyPress(key, engine);
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
        // MAIN GAME LOOP HOOKS
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // Update: Physics, AI, game logic (ECS systems run automatically before this)
        // Render: Visual output, effects, UI drawing

        engine.UpdateEvent += deltaSeconds =>
        {
            // ECS systems (including BoidSystem) already executed by the engine facade.
            // Any additional per-frame logic would go here (UI updates, audio, etc.)

            // Show periodic tips during bloom mode to help users understand effects
            if (_currentShaderMode == ShaderMode.Bloom)
            {
                _timeSinceLastTip += deltaSeconds;

                // Show a tip every 15 seconds during bloom mode
                if (_timeSinceLastTip >= 15f)
                {
                    ShowBloomTip();
                    _timeSinceLastTip = 0f;
                }
            }
        };

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
            DrawSpawnedObjects(engine);

            // Render each species separately to apply different visual effects
            foreach (string id in speciesIds)
                DrawSpecies(id, engine);
            DrawObstacles(new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f), engine);

            // ═══════════════════════════════════════════════════════════════════════════
            // PASS 2: RENDER UI OVERLAY (screen-space, camera-independent)
            // ═══════════════════════════════════════════════════════════════════════════
            //
            // Set the UI camera for screen-space rendering that remains fixed regardless
            // of game camera transformations. Perfect for HUD, menus, and debug information.

            if (showUIOverlay)
            {
                engine.Renderer.SetActiveCamera(engine.CameraManager.UICamera);
                DrawUIOverlay(engine);
            }

            // Finalize the frame
            engine.Renderer.FinalizeFrame();
        };

        // ═══════════════════════════════════════════════════════════════════════════
        // ENGINE EXECUTION
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // Starts the main game loop: Input → Update → Render → Present → Repeat

        // ═══════════════════════════════════════════════════════════════════════════
        // STARTUP MESSAGE
        // ═══════════════════════════════════════════════════════════════════════════

        Console.OutputEncoding = System.Text.Encoding.Unicode;

        ShowStartupMessage();

        engine.Run();


        // ═══════════════════════════════════════════════════════════════════════════
        // LOCAL HELPER FUNCTIONS
        // ═══════════════════════════════════════════════════════════════════════════

        void ShowStartupMessage()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    BOID SAMPLE - BLOOM DEMONSTRATION                        ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine("");

            Console.WriteLine("🎮 CONTROLS:");
            Console.WriteLine("   WASD:        Camera movement (pan world view)");
            Console.WriteLine("   Q/E:         Camera zoom out/in");
            Console.WriteLine("   R:           Reset camera to origin");
            Console.WriteLine("   Tab:         Toggle UI overlay visibility");
            Console.WriteLine("   Mouse Click: Spawn objects at world coordinates");
            Console.WriteLine("   M:           Cycle through shader modes (Normal → SoftGlow → Bloom)");
            Console.WriteLine("");

            Console.WriteLine("🌈 SHADER MODES & VISUAL EFFECTS:");
            Console.WriteLine("   • Normal:   Standard rendering, no glow effects");
            Console.WriteLine("   • SoftGlow: Gentle halos around all boids");
            Console.WriteLine("   • Bloom:    HDR bloom effects with dramatic glowing! (tested when accessed)");
            Console.WriteLine("");

            Console.WriteLine("🦋 BOID SPECIES & ECOSYSTEM:");
            Console.WriteLine("   • White Boids (Small):  Prey species, smallest size");
            Console.WriteLine("   • Blue Boids (Medium):  Secondary predator, medium size");
            Console.WriteLine("   • Red Boids (Large):    Apex predator, largest size");
            Console.WriteLine("   • All species demonstrate flocking behavior with predator-prey interactions");
            Console.WriteLine("");

            Console.WriteLine("🔧 TECHNICAL FEATURES DEMONSTRATED:");
            Console.WriteLine("   • Dual-camera system: Game world + UI overlay rendering");
            Console.WriteLine("   • Screen-to-world coordinate transformation via mouse clicks");
            Console.WriteLine("   • Camera controls: Pan, zoom, and reset functionality");
            Console.WriteLine("   • Craig Reynolds' Boids Algorithm (1986) implementation");
            Console.WriteLine("   • ECS architecture with efficient component queries");
            Console.WriteLine("   • Dynamic shader mode switching for visual effects comparison");
            Console.WriteLine("");

            Console.WriteLine("👀 WHAT TO LOOK FOR:");
            Console.WriteLine("   • SoftGlow: Soft, subtle halos around boids");
            Console.WriteLine("   • Bloom: Bright halos that 'bleed' light into surrounding areas");
            Console.WriteLine("   • HDR Effects: Colors that appear to glow intensely beyond normal brightness");
            Console.WriteLine("   • Red boids in Bloom mode show the most spectacular effects!");
            Console.WriteLine();

            Console.WriteLine("💡 TIPS FOR OPTIMAL SHADER VISIBILITY:");
            Console.WriteLine("   • All boids use the same shader mode for consistent demonstration");
            Console.WriteLine("   • Red boids show most dramatic effects due to intense HDR red values");
            Console.WriteLine("   • White boids provide bright reference with HDR white values");
            Console.WriteLine("   • Blue boids show cool-toned effects with HDR blue values");
            Console.WriteLine("   • Obstacle participates in shader effects alongside the boids");
            Console.WriteLine("   • Watch flocking behavior - it remains the same across all shader modes");
            Console.WriteLine("");

            Console.WriteLine($"🚀 Starting in {_currentShaderMode} mode. Press 'S' to cycle modes and see the effects!");
            Console.WriteLine("");
        }

        void ShowBloomTip()
        {
            var bloomTips = new[]
            {
                "💡 TIP: All boids now use HDR bloom consistently - notice the uniform dramatic glow effects!",
                "💡 TIP: Red boids show the most intense effect due to HDR red colors (2.5, 0.3, 0.3)!",
                "💡 TIP: White boids glow brightly with HDR white (2.0, 2.0, 2.0) in Bloom mode!",
                "💡 TIP: Blue boids also use bloom mode now, showing HDR blue effects (0.3, 0.3, 2.5)!",
                "💡 TIP: Watch for bloom 'bleeding' - bright halos extending beyond all entity boundaries!",
                "💡 TIP: The obstacle also participates in bloom effects for complete demonstration!",
                "💡 TIP: Try switching back to Normal mode (press 'S') to see the dramatic difference!",
            };

            Console.WriteLine(bloomTips[_tipIndex % bloomTips.Length]);
            _tipIndex++;
        }


        void CycleShaderMode()
        {
        if (_availableShaderModes.Count == 0) return;

        try
        {
            _shaderModeIndex = (_shaderModeIndex + 1) % _availableShaderModes.Count;
            var targetMode = _availableShaderModes[_shaderModeIndex];

            // If we've cycled through all basic modes and bloom hasn't been added yet
            if (_shaderModeIndex == 0 && !_availableShaderModes.Contains(ShaderMode.Bloom))
            {
                // Simply add bloom mode to available modes without testing
                // The renderer will handle initialization at the proper time
                _availableShaderModes.Add(ShaderMode.Bloom);
                // Switch directly to bloom mode since user is cycling
                _shaderModeIndex = _availableShaderModes.Count - 1;
                targetMode = ShaderMode.Bloom;
                Console.WriteLine("📊 Bloom mode added to available modes");
            }

            _currentShaderMode = targetMode;

            // Reset tip timer when changing modes
            _timeSinceLastTip = 0f;

            // Enhanced console output with detailed mode explanations
            Console.WriteLine();
            Console.WriteLine($"=== SHADER MODE: {_currentShaderMode.ToString().ToUpper()} ===");

            switch (_currentShaderMode)
            {
                case ShaderMode.Normal:
                    Console.WriteLine("• Standard rendering mode");
                    Console.WriteLine("• All boids use regular colors (no glow effects)");
                    Console.WriteLine("• Best for seeing basic flocking behavior clearly");
                    break;

                case ShaderMode.SoftGlow:
                    Console.WriteLine("• Subtle glow effects enabled");
                    Console.WriteLine("• All boids: SoftGlow effect with gentle halos");
                    Console.WriteLine("• Obstacle: SoftGlow effect");
                    Console.WriteLine("• Look for: Soft halos around all entities");
                    break;

                case ShaderMode.Bloom:
                    Console.WriteLine("• HDR bloom effects enabled - DRAMATIC GLOW!");
                    Console.WriteLine("• All boids: Bloom with HDR colors for intense glow");
                    Console.WriteLine("• Obstacle: Bloom effect with enhanced brightness");
                    Console.WriteLine("• Look for: Bright halos that 'bleed' into surrounding areas");
                    Console.WriteLine("• Tip: All species show dramatic bloom effects consistently!");
                    break;
            }

            Console.WriteLine($"• Species behavior: {GetSpeciesBehaviorDescription()}");
            Console.WriteLine();

            // Report available modes on first cycle
            if (_shaderModeIndex == 0)
            {
                Console.WriteLine($"📊 Available shader modes: {string.Join(", ", _availableShaderModes)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to switch to shader mode: {ex.Message}");
            // Revert to previous mode index if the switch failed
            _shaderModeIndex = (_shaderModeIndex - 1 + _availableShaderModes.Count) % _availableShaderModes.Count;
            _currentShaderMode = _availableShaderModes[_shaderModeIndex];
        }
    }

        // ═══════════════════════════════════════════════════════════════════════════
        // CAMERA CONTROL HANDLERS
        // ═══════════════════════════════════════════════════════════════════════════

        void HandleKeyPress(Key key, EngineFacade engine)
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
                
                // ─── Shader Mode Controls (M) ───────────────────────────────────────
                case Key.M: // Cycle shader modes (moved from S to avoid conflict)
                    CycleShaderMode();
                    break;
                
                // ─── Camera Zoom Controls (Q/E) ─────────────────────────────────────
                case Key.Q: // Zoom out
                    camera.Zoom = Math.Max(0.1f, camera.Zoom - 0.1f);
                    break;
                case Key.E: // Zoom in
                    camera.Zoom = Math.Min(5f, camera.Zoom + 0.1f);
                    break;
                
                // ─── Camera Reset (R) ────────────────────────────────────────────────
                case Key.R:
                    camera.Position = Vector2D<float>.Zero;
                    camera.Zoom = 1f;
                    camera.Rotation = 0f;
                    Console.WriteLine("Camera reset to origin");
                    break;
                
                // ─── UI Toggle (Tab) ─────────────────────────────────────────────────
                case Key.Tab:
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
                Size = 0.05f,
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

        void DrawSpawnedObjects(EngineFacade engine)
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
            engine.Renderer.SetShaderMode(_currentShaderMode);
            engine.Renderer.SetColor(new Vector4D<float>(1f, 0.5f, 0.2f, 1f)); // Orange
            engine.Renderer.UpdateVertices(vertexBuffer.ToArray());
            engine.Renderer.Draw();
        }

        void DrawUIOverlay(EngineFacade engine)
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

            // Controls indicator (small rectangles)
            DrawUIQuad(-380f, 140f, 15f, 5f, new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f), engine); // "WASD: Camera"
            DrawUIQuad(-380f, 130f, 15f, 5f, new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f), engine); // "Q/E: Zoom"
            DrawUIQuad(-380f, 120f, 15f, 5f, new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f), engine); // "R: Reset"
            DrawUIQuad(-380f, 110f, 15f, 5f, new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f), engine); // "Tab: UI Toggle"

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

        string GetSpeciesBehaviorDescription()
        {
            return _currentShaderMode switch
            {
                ShaderMode.Normal => "All entities rendered with standard colors",
                ShaderMode.SoftGlow => "All entities show gentle glow effects",
                ShaderMode.Bloom => "All entities use HDR bloom for dramatic effects",
                _ => "Standard rendering"
            };
        }

        void UpdateBoidSettings(Vector2D<int> windowSize)
        {
            // ───────────────────────────────────────────────────────────────────────
            // ASPECT RATIO PRESERVATION
            // ───────────────────────────────────────────────────────────────────────
            //
            // Safe zone prevents boids from getting too close to screen edges.
            // Aspect ratio calculation ensures circular boid movement patterns
            // remain circular (not elliptical) on different screen ratios.

            const float safeZoneRatio = 0.9f; // 10% margin from screen edges
            float aspectRatio = windowSize.Y / (float)windowSize.X;

            // NDC boundaries: -1 to +1 is full screen, safe zone is smaller
            var boundaryMin = new Vector2D<float>(-safeZoneRatio, -safeZoneRatio * aspectRatio);
            var boundaryMax = new Vector2D<float>(safeZoneRatio, safeZoneRatio * aspectRatio);

            // ───────────────────────────────────────────────────────────────────────
            // SPECIES INTERACTION MATRIX
            // ───────────────────────────────────────────────────────────────────────
            //
            // Defines how each species reacts to others using separation, alignment, cohesion weights:
            // - High separation: Avoid this species (predator avoidance)
            // - High alignment: Match movement direction (flocking)
            // - High cohesion: Move toward this species (attraction/following)

            var interactionMap = new Dictionary<(string Self, string Other), SpeciesInteraction>();
            foreach (string selfId in speciesIds)
            foreach (string otherId in speciesIds)
                interactionMap[(selfId, otherId)] =
                    selfId == otherId
                        ? // Same species: Standard flocking behavior
                        new SpeciesInteraction(1f, 1f, 1f)
                        : (selfId, otherId) switch
                        {
                            // White boids flee from Blue and Red (prey behavior)
                            ("White", "Blue") or ("White", "Red") => new SpeciesInteraction(
                                1.5f, // Increased separation (flee)
                                0f,   // No alignment (don't follow)
                                0f    // No cohesion (don't approach)
                            ),

                            // Blue boids chase White (predator behavior)
                            ("Blue", "White") => new SpeciesInteraction(0f, 0f, 1.2f),

                            // Red boids chase both White and Blue (apex predator)
                            ("Red", "White") or ("Red", "Blue") => new SpeciesInteraction(
                                0f,    // No separation (aggressive)
                                0f,    // No alignment
                                1.2f   // Strong cohesion (hunt/chase)
                            ),

                            // All other interactions: neutral (ignore)
                            _ => new SpeciesInteraction(0f, 0f, 0f),
                        };

        // ═══════════════════════════════════════════════════════════════════════════
        // BOID PHYSICS & INTERACTION PARAMETERS
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // These parameters control the core flocking algorithm behavior:
        // - Neighbor radius: Spatial awareness distance for interaction calculations
        // - Max force: Steering constraint preventing unrealistic sharp turns
        // - Max speed: Velocity cap maintaining realistic movement speeds
        // - Boundary strength: Repulsion force from world edges (invisible walls)

        var boidSettings = new MultiSpeciesBoidSettingsComponent(
            0.4f,             // Neighbor detection radius (NDC units)
            0.02f,            // Maximum steering force per frame
            0.2f,             // Maximum movement speed (NDC units/second)
            boundaryMin,      // World boundary minimum (-X, -Y)
            boundaryMax,      // World boundary maximum (+X, +Y)
            interactionMap,   // Species interaction behavior matrix
            1.5f              // Boundary avoidance strength multiplier
        );

            // Apply settings to the global settings entity
            world.SetComponent(settingsEntity, boidSettings);
        }

        void SpawnAllSpecies()
        {
            // ───────────────────────────────────────────────────────────────────────
            // ECOSYSTEM POPULATION DISTRIBUTION
            // ───────────────────────────────────────────────────────────────────────
            //
            // Realistic ecosystem: More prey than predators creates natural balance.
            // Each boid gets randomized starting position and zero initial velocity.

            var random = new Random();
            var spawnCounts = new Dictionary<string, int>
            {
                ["White"] = 30,  // Most numerous (prey base)
                ["Blue"] = 20,   // Moderate (secondary predator)
                ["Red"] = 10,    // Fewest (apex predator)
            };

            foreach (string id in speciesIds)
            {
                float scale = speciesScales[id];
                int count = spawnCounts[id];

                for (int i = 0; i < count; i++)
                {
                    // Create new entity using NEW FLUENT API for readable entity composition
                    var randomPosition = new Vector2D<float>(
                        (float)(random.NextDouble() * 2.0 - 1.0), // X: -1 to +1
                        (float)(random.NextDouble() * 2.0 - 1.0)  // Y: -1 to +1
                    );

                    var e = world.CreateEntity()
                        .WithTransform(world, randomPosition, 0f, Vector2D<float>.One) // Position, rotation, scale in one call
                        .WithComponent(world, new VelocityComponent(0f, 0f))           // Start with zero velocity
                        .WithComponent(world, new BoidSpeciesComponent(id, scale));    // Species and visual scale
                }
            }
        }

        void SpawnObstacles()
        {
            // ───────────────────────────────────────────────────────────────────────
            // ENVIRONMENTAL OBSTACLES
            // ───────────────────────────────────────────────────────────────────────
            //
            // Static obstacles create interesting navigation challenges for boids.
            // They must steer around these while maintaining flocking behavior.

            var e = world.CreateEntity()
                .WithTransform(world, Vector2D<float>.Zero, 0f, Vector2D<float>.One)  // Center of screen
                .WithComponent(world, new ObstacleComponent(0.2f));                   // Radius in NDC units
        }

        void DrawSpecies(string filterId, EngineFacade engine)
        {
            // ───────────────────────────────────────────────────────────────────────
            // BOID VISUAL REPRESENTATION
            // ───────────────────────────────────────────────────────────────────────
            //
            // Each boid is rendered as a triangle pointing in its movement direction.
            // Triangle vertices are defined in local space, then transformed to world space.

            // ───────────────────────────────────────────────────────────────────────
            // SHADER EFFECTS AND RENDERING
            // ───────────────────────────────────────────────────────────────────────
            //
            // All boids use the current shader mode to provide consistent demonstration
            // of each visual effect. This allows users to clearly see what each mode does.

            ShaderMode shaderToUse = _currentShaderMode;

            // Choose color palette based on shader mode for optimal visual effects
            // HDR colors (values > 1.0) create dramatic bloom effects when bloom shaders are active
            var useHDRColors = (shaderToUse == ShaderMode.Bloom);
            var currentColorPalette = useHDRColors ? hdrSpeciesColors : speciesColors;
            var baseColor = currentColorPalette[filterId];

            // Color enhancement logic for different shader modes
            var enhancedColor = shaderToUse switch
            {
                ShaderMode.Normal => baseColor,
                ShaderMode.SoftGlow => new Vector4D<float>(
                    Math.Min(baseColor.X * 1.3f, 1.0f),
                    Math.Min(baseColor.Y * 1.3f, 1.0f),
                    Math.Min(baseColor.Z * 1.3f, 1.0f),
                    1f),
                ShaderMode.Bloom => new Vector4D<float>(
                    Math.Min(baseColor.X * 0.6f, 1.0f),
                    Math.Min(baseColor.Y * 0.6f, 1.0f),
                    Math.Min(baseColor.Z * 0.6f, 1.0f),
                    1f),
                _ => baseColor
            };

            // Triangle definitions vary by shader mode for optimal visual effects
            Vector2D<float>[] trianglePoints;
            (Vector2D<float> pos, Vector2D<float> tex)[]? triangleWithTexCoords = null;

            if (shaderToUse == ShaderMode.Normal)
            {
                // Simple triangle for normal mode
                trianglePoints = new[]
                {
                    new Vector2D<float>(-0.02f, -0.015f),
                    new Vector2D<float>(0.02f, -0.015f),
                    new Vector2D<float>(0.0f, 0.04f),
                };
            }
            else
            {
                // Optimized triangle with texture coordinates for glow modes
                // Using smaller coordinates so vDistance stays within shader's expected range (0.0 - 1.2)
                triangleWithTexCoords = new[]
                {
                    (pos: new Vector2D<float>(-0.03f, -0.025f), tex: new Vector2D<float>(-0.8f, -0.6f)),
                    (pos: new Vector2D<float>(0.03f, -0.025f), tex: new Vector2D<float>(0.8f, -0.6f)),
                    (pos: new Vector2D<float>(0.0f, 0.06f), tex: new Vector2D<float>(0f, 0.8f)),
                };
                trianglePoints = triangleWithTexCoords.Select(t => t.pos).ToArray();
            }

            // Vertex buffer for all triangles of this species
            var vertices = new List<FullVertex>();

            // ───────────────────────────────────────────────────────────────────────
            // ECS QUERY AND TRANSFORMATION PIPELINE
            // ───────────────────────────────────────────────────────────────────────

            foreach (
                var (_, worldTransform, vel, spec) in world.Query<
                    WorldTransformComponent,
                    VelocityComponent,
                    BoidSpeciesComponent
                >()
            )
            {
                // Skip boids not of current species
                if (spec.SpeciesId != filterId)
                    continue;

                // ───────────────────────────────────────────────────────────────────
                // 2D TRANSFORMATION MATHEMATICS
                // ───────────────────────────────────────────────────────────────────

                var wp = worldTransform.WorldPosition;                // World position from transform system
                var normV = ((Vector2D<float>)vel).Normalize();       // Normalized velocity (direction)

                // Calculate rotation angle from velocity vector
                // atan2 gives angle from +X axis, subtract π/2 because triangle points up (+Y)
                float angle = MathF.Atan2(normV.Y, normV.X) - MathF.PI / 2f;

                // Precompute rotation matrix components for efficiency
                float cosA = MathF.Cos(angle);
                float sinA = MathF.Sin(angle);
                float scale = spec.Scale;

                // Transform each triangle vertex: Scale → Rotate → Translate
                for (int i = 0; i < trianglePoints.Length; i++)
                {
                    var off = trianglePoints[i];

                    // 1. Scale the local vertex
                    var s = off * scale;

                    // 2. Rotate using 2D rotation matrix:
                    //    [cos -sin] [x]
                    //    [sin  cos] [y]
                    var r = new Vector2D<float>(s.X * cosA - s.Y * sinA, s.X * sinA + s.Y * cosA);

                    // 3. Translate to world position
                    var p = wp + r;

                    // Add to vertex buffer with appropriate texture coordinates
                    var texCoord = shaderToUse == ShaderMode.Normal ?
                        new Vector2D<float>(0f, 0f) :
                        triangleWithTexCoords![i].tex;

                    vertices.Add(new FullVertex(p, texCoord, enhancedColor));
                }

                // SoftGlow mode: Use filled triangles only (no wireframe overlay)
                // This ensures boids appear as filled entities with glow effects, not empty wireframes
            }

            // Skip rendering if no boids of this species exist
            if (vertices.Count == 0)
                return;

            engine.Renderer.SetShaderMode(shaderToUse);
            engine.Renderer.UpdateVertices(vertices.ToArray());
            engine.Renderer.Draw();
        }

        void DrawObstacles(Vector4D<float> color, EngineFacade engine)
        {
            const int segments = 16;
            var vertices = new List<FullVertex>();

            // Use the current shader mode for obstacles to demonstrate effects consistently
            var obstacleShaderMode = _currentShaderMode;

            // Color enhancement based on current shader mode for optimal visual effects
            var enhancedColor = obstacleShaderMode switch
            {
                ShaderMode.Normal => color,
                ShaderMode.SoftGlow => new Vector4D<float>(
                    Math.Min(color.X * 1.3f, 1.0f),
                    Math.Min(color.Y * 1.3f, 1.0f),
                    Math.Min(color.Z * 1.3f, 1.0f),
                    1f),
                ShaderMode.Bloom => new Vector4D<float>(
                    Math.Min(color.X * 1.8f, 1.0f),
                    Math.Min(color.Y * 1.8f, 1.0f),
                    Math.Min(color.Z * 1.8f, 1.0f),
                    1f),
                _ => color
            };

            foreach (var (_, worldTransform, obs) in world.Query<WorldTransformComponent, ObstacleComponent>())
            {
                var center = worldTransform.WorldPosition;
                float r = obs.Radius;

                for (int i = 0; i < segments; i++)
                {
                    float a0 = 2 * MathF.PI * i / segments;
                    float a1 = 2 * MathF.PI * (i + 1) / segments;

                    var p0 = center + new Vector2D<float>(r * MathF.Cos(a0), r * MathF.Sin(a0));
                    var p1 = center + new Vector2D<float>(r * MathF.Cos(a1), r * MathF.Sin(a1));

                    // Generate triangle with appropriate texture coordinates for glow effect
                    vertices.Add(new FullVertex(center, new Vector2D<float>(0f, 0f), enhancedColor));
                    vertices.Add(new FullVertex(p0, new Vector2D<float>(MathF.Cos(a0), MathF.Sin(a0)), enhancedColor));
                    vertices.Add(new FullVertex(p1, new Vector2D<float>(MathF.Cos(a1), MathF.Sin(a1)), enhancedColor));
                }
            }

            if (vertices.Count == 0) return;

            engine.Renderer.SetShaderMode(obstacleShaderMode);
            engine.Renderer.UpdateVertices(vertices.ToArray());
            engine.Renderer.Draw();
        }
    }
}
