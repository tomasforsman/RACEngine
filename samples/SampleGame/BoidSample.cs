// File: samples/SampleGame/BoidSample.cs
//
// ════════════════════════════════════════════════════════════════════════════════
// EDUCATIONAL BOIDS SIMULATION
// ════════════════════════════════════════════════════════════════════════════════
//
// This sample demonstrates several key computer graphics and game development concepts:
//
// 1. BOIDS ALGORITHM (Craig Reynolds, 1986):
//    - Separation: Avoid crowding neighbors
//    - Alignment: Steer towards average heading of neighbors
//    - Cohesion: Steer towards average position of neighbors
//    - Result: Emergent flocking behavior from simple rules
//
// 2. ENTITY COMPONENT SYSTEM (ECS) ARCHITECTURE:
//    - Entities: Unique IDs representing game objects
//    - Components: Pure data (Position, Velocity, Species)
//    - Systems: Logic that operates on components
//    - Benefits: Performance, modularity, data-oriented design
//
// 3. REAL-TIME RENDERING PIPELINE:
//    - Vertex data generation (CPU-side geometry creation)
//    - Shader mode switching (Normal vs Bloom effects)
//    - Batched rendering for performance
//
// 4. COORDINATE SYSTEMS:
//    - Normalized Device Coordinates (NDC): -1 to +1 range
//    - Aspect ratio handling for proper scaling
//    - 2D transformations (rotation, translation, scaling)
//
// ════════════════════════════════════════════════════════════════════════════════

using Rac.Core.Extension;
using Rac.Core.Manager;
using Rac.ECS.Component;
using Rac.ECS.System;
using Rac.Engine;
using Rac.Input.Service;
using Rac.Rendering;
using Rac.Rendering.Shader;
using Rac.Rendering.VFX;
using Silk.NET.Maths;

namespace SampleGame;

public static class BoidSample
{
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
            ["White"] = new(1f, 1f, 1f, 1f),  // Pure white
            ["Blue"] = new(0f, 0f, 1f, 1f),   // Pure blue
            ["Red"] = new(1f, 0f, 0f, 1f),    // Pure red
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
        // MAIN GAME LOOP HOOKS
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // Update: Physics, AI, game logic (ECS systems run automatically before this)
        // Render: Visual output, effects, UI drawing

        engine.UpdateEvent += deltaSeconds =>
        {
            // ECS systems (including BoidSystem) already executed by the engine facade.
            // Any additional per-frame logic would go here (UI updates, audio, etc.)
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

        engine.Run();



        // ═══════════════════════════════════════════════════════════════════════════
        // LOCAL HELPER FUNCTIONS
        // ═══════════════════════════════════════════════════════════════════════════

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

            // ───────────────────────────────────────────────────────────────────────
            // BOID PHYSICS PARAMETERS
            // ───────────────────────────────────────────────────────────────────────

            var boidSettings = new MultiSpeciesBoidSettingsComponent(
                0.4f,             // Neighbor detection radius
                0.02f,            // Maximum turning force (steering strength)
                0.2f,             // Maximum speed
                boundaryMin,      // World bounds minimum
                boundaryMax,      // World bounds maximum
                interactionMap,   // Species interaction rules
                1.5f              // Boundary avoidance strength
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

            // Triangle shape in local coordinates (pointing up in local space)
            var triangle = new[]
            {
                new Vector2D<float>(-0.008f, -0.008f), // Bottom left
                new Vector2D<float>(0.008f, -0.008f),  // Bottom right
                new Vector2D<float>(0.000f, 0.03f),    // Top point (forward direction)
            };

            // Vertex buffer for all triangles of this species
            var verts = new List<float>();

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
                foreach (var off in triangle)
                {
                    // 1. Scale the local vertex
                    var s = off * scale;

                    // 2. Rotate using 2D rotation matrix:
                    //    [cos -sin] [x]
                    //    [sin  cos] [y]
                    var r = new Vector2D<float>(s.X * cosA - s.Y * sinA, s.X * sinA + s.Y * cosA);

                    // 3. Translate to world position
                    var p = wp + r;

                    // Add to vertex buffer (x, y coordinates)
                    verts.Add(p.X);
                    verts.Add(p.Y);
                }
            }

            // Skip rendering if no boids of this species exist
            if (verts.Count == 0)
                return;

            // ───────────────────────────────────────────────────────────────────────
            // SHADER EFFECTS AND RENDERING
            // ───────────────────────────────────────────────────────────────────────
            //
            // Different species can use different visual effects:
            // - Normal shader: Standard rendering
            // - Bloom shader: Glow effect for dramatic visual impact

            if (filterId == "Red")
            {
                // Red boids get bloom effect (glowing predator appearance)
                engine.Renderer.SetShaderMode(ShaderMode.Normal);
            }
            else
            {
                // Standard rendering for other species
                engine.Renderer.SetShaderMode(ShaderMode.Normal);
            }

            // Set species color and upload vertex data to GPU
            engine.Renderer.SetColor(speciesColors[filterId]);
            engine.Renderer.UpdateVertices(verts.ToArray());
            engine.Renderer.Draw();
        }

        void DrawObstacles(Vector4D<float> color)
        {
            const int segments = 16;
            var verts = new List<float>();

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

                    // CENTER VERTEX - Position + TexCoord
                    verts.Add(center.X); verts.Add(center.Y);    // Position
                    verts.Add(0.0f); verts.Add(0.0f);            // TexCoord (0,0) = center

                    // FIRST EDGE VERTEX - Position + TexCoord
                    verts.Add(p0.X); verts.Add(p0.Y);            // Position
                    verts.Add(MathF.Cos(a0)); verts.Add(MathF.Sin(a0)); // TexCoord (-1 to +1)

                    // SECOND EDGE VERTEX - Position + TexCoord
                    verts.Add(p1.X); verts.Add(p1.Y);            // Position
                    verts.Add(MathF.Cos(a1)); verts.Add(MathF.Sin(a1)); // TexCoord (-1 to +1)
                }
            }

            if (verts.Count == 0) return;

            engine.Renderer.SetShaderMode(ShaderMode.SoftGlow);
            engine.Renderer.SetColor(color);
            engine.Renderer.UpdateVertices(verts.ToArray());
            engine.Renderer.Draw();
        }
    }
}
