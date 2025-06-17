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
using Rac.ECS.System;
using Rac.Engine;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Rendering;
using Rac.Rendering.Shader;
using Rac.Rendering.VFX;
using Silk.NET.Input;
using Silk.NET.Maths;
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
        // Settings Entity: Holds global configuration data (boundary constraints, interaction rules)

        var world = engine.World;
        engine.AddSystem(new BoidSystem(world));
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
        // INPUT HANDLING FOR SHADER MODE SWITCHING
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // Interactive demonstration of different rendering modes.
        // Press 'S' to cycle through Normal → SoftGlow → Bloom → Normal...

        engine.KeyEvent += (key, keyEvent) =>
        {
            if (key == Key.S && keyEvent == KeyboardKeyState.KeyEvent.Pressed)
            {
                CycleShaderMode();
            }
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
            // Render each species separately to apply different visual effects
            foreach (string id in speciesIds)
                DrawSpecies(id);
            DrawObstacles(new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f));
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
            Console.WriteLine("   'S' - Cycle through shader modes (Normal → SoftGlow → Bloom)");
            Console.WriteLine("");

            Console.WriteLine("🌈 SHADER MODES & VISUAL EFFECTS:");
            Console.WriteLine("   • Normal:   Standard rendering, no glow effects");
            Console.WriteLine("   • SoftGlow: Gentle halos around all boids");
            Console.WriteLine("   • Bloom:    HDR bloom effects with dramatic glowing! (tested when accessed)");
            Console.WriteLine("");

            Console.WriteLine("🦋 BOID SPECIES & EFFECTS:");
            Console.WriteLine("   • All boids use the currently selected shader mode consistently");
            Console.WriteLine("   • White Boids (Small):  Standard flocking, smallest size");
            Console.WriteLine("   • Blue Boids (Medium):  Neutral species, medium size");
            Console.WriteLine("   • Red Boids (Large):    Predator species, largest size");
            Console.WriteLine("   • All species demonstrate the same shader effects for clear comparison");
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
                    // Create new entity in ECS world
                    var e = world.CreateEntity();

                    // Random position in NDC space (-1 to +1)
                    world.SetComponent(
                        e,
                        new PositionComponent(
                            (float)(random.NextDouble() * 2.0 - 1.0), // X: -1 to +1
                            (float)(random.NextDouble() * 2.0 - 1.0)  // Y: -1 to +1
                        )
                    );

                    // Start with zero velocity (boids will accelerate naturally)
                    world.SetComponent(e, new VelocityComponent(0f, 0f));

                    // Species identifier and visual scale
                    world.SetComponent(e, new BoidSpeciesComponent(id, scale));
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

            var e = world.CreateEntity();
            world.SetComponent(e, new PositionComponent(0f, 0f));     // Center of screen
            world.SetComponent(e, new ObstacleComponent(0.2f));       // Radius in NDC units
        }

        void DrawSpecies(string filterId)
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
                    Math.Min(baseColor.X * 1.6f, 1.0f),
                    Math.Min(baseColor.Y * 1.6f, 1.0f),
                    Math.Min(baseColor.Z * 1.6f, 1.0f),
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
                var (_, pos, vel, spec) in world.Query<
                    PositionComponent,
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

                var wp = (Vector2D<float>)pos;                    // World position
                var normV = ((Vector2D<float>)vel).Normalize();   // Normalized velocity (direction)

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

        void DrawObstacles(Vector4D<float> color)
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

            foreach (var (_, pos, obs) in world.Query<PositionComponent, ObstacleComponent>())
            {
                var center = (Vector2D<float>)pos;
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
