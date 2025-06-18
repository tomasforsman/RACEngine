// File: samples/SampleGame/ShooterSample.cs
//
// ═══════════════════════════════════════════════════════════════════════════════
// EDUCATIONAL SHOOTER SAMPLE - COMPREHENSIVE GAME ENGINE DEMONSTRATION
// ═══════════════════════════════════════════════════════════════════════════════
//
// This interactive sample demonstrates fundamental game development and graphics
// programming concepts through a simple but complete shooter mechanic:
//
// 1. REAL-TIME GAME LOOP ARCHITECTURE:
//    - Event-driven input system with immediate response
//    - Frame-rate independent update logic using delta time
//    - Separated update/render phases for clean architecture
//    - Resource lifecycle management and proper cleanup
//    - Engine facade pattern for simplified API access
//
// 2. INPUT HANDLING & STATE MANAGEMENT:
//    - Discrete input: Key press/release events for state changes
//    - Continuous input: Direction changes with immediate visual feedback
//    - State persistence: Auto-fire mode toggle with visual indication
//    - Input mapping: Multiple keys (WASD/Arrows) for same function
//    - Temporal control: Time-based firing intervals for balanced gameplay
//
// 3. 2D TRANSFORMATION MATHEMATICS:
//    - Rotation matrices: Converting direction enums to rotation angles
//    - Vector mathematics: Unit direction vectors for velocity calculation
//    - Coordinate systems: NDC space for resolution-independent rendering
//    - Trigonometric functions: sin/cos for rotation transformations
//    - Matrix operations: Scale → Rotate → Translate transformation pipeline
//
// 4. RENDERING PIPELINE & VISUAL EFFECTS:
//    - Geometry generation: Dynamic vertex buffer construction
//    - Batch rendering: Multiple objects in single draw call for performance
//    - Shader mode switching: Per-object visual effect demonstration
//    - Material differentiation: Ship vs bullet rendering with different modes
//    - Visual feedback: Immediate response to user input through rendering
//
// 5. GAME MECHANICS & PHYSICS:
//    - Projectile physics: Velocity-based movement with frame-rate independence
//    - Spatial updates: Position integration using Euler method
//    - Continuous spawning: Auto-fire system with precise timing intervals
//    - State machines: Direction/rotation state with enumerated values
//    - Collection management: Dynamic bullet list with real-time updates
//
// 6. SOFTWARE ARCHITECTURE PATTERNS:
//    - Facade pattern: Simplified engine interface hiding complexity
//    - Component composition: Separate systems for input, rendering, physics
//    - Event-driven design: Loose coupling through callback registration
//    - Data structures: Efficient collections for dynamic game objects
//    - Separation of concerns: Clear boundaries between subsystems
//
// CONTROLS & INTERACTION:
// - WASD/Arrow Keys: Immediate ship rotation (discrete directional control)
// - Space: Toggle auto-fire mode (persistent state change)
// - V: Cycle through visual effect combinations (educational demonstration)
//
// EDUCATIONAL OBJECTIVES:
// - Understand fundamental game loop structure and timing
// - Experience real-time input handling and state management
// - Observe 2D graphics transformations and coordinate systems
// - Learn performance optimization through batched rendering
// - Connect mathematical concepts to visual results
//
// ═══════════════════════════════════════════════════════════════════════════════

using Rac.Core.Manager;
using Rac.Engine;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Rendering.Shader;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace SampleGame;

public static class ShooterSample
{
    // ═══════════════════════════════════════════════════════════════════════════
    // GAME BALANCE & PHYSICS CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // These values control the game feel and physics behavior:
    // - BulletSpeed: Projectile velocity in NDC units per second
    // - FireInterval: Time between automatic shots (balances gameplay)

    private const float BulletSpeed = 0.75f;  // Fast projectile movement
    private const float FireInterval = 0.2f;  // 5 shots per second when auto-firing

    // ═══════════════════════════════════════════════════════════════════════════
    // ADAPTIVE VISUAL EFFECTS DEMONSTRATION SYSTEM
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // This system showcases different rendering modes applied to different object types,
    // demonstrating how visual effects can be mixed and matched for educational purposes.
    // Each effect mode combination illustrates different graphics programming concepts.

    private static ShaderMode _shipShaderMode = ShaderMode.Normal;
    private static ShaderMode _bulletShaderMode = ShaderMode.SoftGlow;
    private static readonly ShaderMode[] _availableShaderModes = { ShaderMode.Normal, ShaderMode.SoftGlow, ShaderMode.Bloom };
    private static int _effectModeIndex = 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // GAME STATE & OBJECT MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // Core game state variables demonstrating different aspects of game programming:
    // - Engine reference: Facade pattern for simplified API access
    // - Direction state: Enumerated ship orientation with deterministic behavior
    // - Boolean state: Auto-fire toggle with persistent behavior
    // - Time accumulation: Frame-rate independent timing for consistent gameplay
    // - Spatial state: Ship rotation as continuous angular value
    // - Dynamic collections: Runtime object management with efficient updates

    // Engine facade providing unified access to rendering, input, and world systems
    private static EngineFacade? engineFacade;

    // Current cardinal direction the ship is facing (discrete state)
    private static Direction shipDirection = Direction.Up;

    // Whether continuous auto-fire is currently enabled (persistent toggle)
    private static bool isAutoFireEnabled;

    // Time accumulator for precise auto-fire interval timing
    private static float timeSinceLastShot;

    // Current ship rotation angle in radians (continuous angular state)
    private static float shipRotation;

    // Dynamic collection of active projectiles in the game world
    private static readonly List<Bullet> activeBullets = new();

    // ───────────────────────────────────────────────────────────────────────────
    // SHIP GEOMETRY DEFINITION
    // ───────────────────────────────────────────────────────────────────────────
    //
    // Triangle model defined in local coordinate space (centered at origin).
    // This demonstrates basic 2D geometry construction for game objects.
    // The triangle points "up" in local space, requiring rotation for other directions.

    private static readonly Vector2D<float>[] shipModel = new[]
    {
        new Vector2D<float>(-0.05f, -0.05f),  // Left base vertex
        new Vector2D<float>(0.05f, -0.05f),   // Right base vertex
        new Vector2D<float>(0.00f, 0.10f),    // Top point (ship nose)
    };

    public static void Run(string[] args)
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // ENGINE INITIALIZATION
        // ═══════════════════════════════════════════════════════════════════════════
        var windowManager = new WindowManager();
        var inputService = new SilkInputService();
        var configurationManager = new ConfigManager();

        engineFacade = new EngineFacade(windowManager, inputService, configurationManager);

        // ═══════════════════════════════════════════════════════════════════════════
        // EDUCATIONAL STARTUP GUIDANCE
        // ═══════════════════════════════════════════════════════════════════════════
        Console.OutputEncoding = System.Text.Encoding.Unicode;

        Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    SHOOTER SAMPLE - GAME ENGINE EDUCATION                   ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("🎯 EDUCATIONAL OBJECTIVES:");
        Console.WriteLine("   • Experience real-time game loop architecture");
        Console.WriteLine("   • Understand input handling and state management");
        Console.WriteLine("   • Observe 2D transformation mathematics in action");
        Console.WriteLine("   • Learn performance optimization through batched rendering");
        Console.WriteLine();
        Console.WriteLine("🎮 INTERACTIVE CONTROLS:");
        Console.WriteLine("   WASD/Arrows: Rotate ship direction (immediate visual feedback)");
        Console.WriteLine("   Space:       Toggle auto-fire mode (persistent state)");
        Console.WriteLine("   V:           Cycle visual effects (educational demonstration)");
        Console.WriteLine();
        Console.WriteLine("🔧 TECHNICAL FEATURES DEMONSTRATED:");
        Console.WriteLine("   • Frame-rate independent physics using delta time");
        Console.WriteLine("   • Mixed shader modes: Ship vs Bullet rendering separation");
        Console.WriteLine("   • Dynamic geometry generation with rotation transforms");
        Console.WriteLine("   • Efficient batch rendering for multiple similar objects");
        Console.WriteLine();
        Console.WriteLine("📊 CURRENT VISUAL CONFIGURATION:");
        Console.WriteLine($"   Ship Mode:   {_shipShaderMode} (geometric rendering)");
        Console.WriteLine($"   Bullet Mode: {_bulletShaderMode} (projectile effects)");
        Console.WriteLine($"   Auto-Fire:   {(isAutoFireEnabled ? "ENABLED" : "DISABLED")} (use Space to toggle)");
        Console.WriteLine();
        Console.WriteLine("💡 OBSERVATION TIPS:");
        Console.WriteLine("   • Notice immediate response to direction changes");
        Console.WriteLine("   • Watch for consistent timing in auto-fire intervals");
        Console.WriteLine("   • Observe smooth rotation transforms applied to ship geometry");
        Console.WriteLine("   • Compare different visual effects on ship vs bullets");
        Console.WriteLine();
        Console.WriteLine("🚀 Ready to demonstrate core game engine concepts!");
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════════════════
        // EVENT REGISTRATION
        // ═══════════════════════════════════════════════════════════════════════════

        // Initial Draw when Renderer is ready
        engineFacade.LoadEvent += () => RedrawScene();

        // ─── Hook Input through the facade’s KeyEvent ───────────
        engineFacade.KeyEvent += OnKeyPressed;

        // ─── Hook Game Loop ─────────────────────────────────────
        engineFacade.UpdateEvent += OnUpdate;
        engineFacade.RenderEvent += _ => RedrawScene();

        // ─── Start the Engine Loop ──────────────────────────────
        engineFacade.Run();
    }

    private static void OnKeyPressed(Key key, KeyboardKeyState.KeyEvent keyEvent)
    {
        // Only respond on key-down
        if (keyEvent != KeyboardKeyState.KeyEvent.Pressed)
            return;

        // Rotate ship based on WASD or arrow keys
        shipDirection = key switch
        {
            Key.W or Key.Up => Direction.Up,
            Key.D or Key.Right => Direction.Right,
            Key.S or Key.Down => Direction.Down,
            Key.A or Key.Left => Direction.Left,
            _ => shipDirection,
        };

        // Map direction to rotation angle
        shipRotation = shipDirection switch
        {
            Direction.Up => 0f,
            Direction.Right => 3f * MathF.PI / 2f,
            Direction.Down => MathF.PI,
            Direction.Left => MathF.PI / 2f,
            _ => shipRotation,
        };

        // Toggle auto-fire and spawn one bullet immediately
        if (key == Key.Space)
        {
            isAutoFireEnabled = !isAutoFireEnabled;
            SpawnBulletInCurrentDirection();
        }

        // Cycle through visual effect modes
        if (key == Key.V)
        {
            CycleVisualEffects();
        }
    }

    private static void OnUpdate(float deltaTime)
    {
        // Handle continuous auto-fire
        if (isAutoFireEnabled)
        {
            timeSinceLastShot += deltaTime;
            if (timeSinceLastShot >= FireInterval)
            {
                timeSinceLastShot -= FireInterval;
                SpawnBulletInCurrentDirection();
            }
        }

        // Move all bullets
        foreach (var bullet in activeBullets)
            bullet.Position += bullet.Velocity * deltaTime;
    }

    private static void SpawnBulletInCurrentDirection()
    {
        // Compute unit vector for current shipDirection
        var directionVector = shipDirection switch
        {
            Direction.Up => new Vector2D<float>(0f, 1f),
            Direction.Right => new Vector2D<float>(1f, 0f),
            Direction.Down => new Vector2D<float>(0f, -1f),
            Direction.Left => new Vector2D<float>(-1f, 0f),
            _ => Vector2D<float>.Zero,
        };

        activeBullets.Add(
            new Bullet { Position = Vector2D<float>.Zero, Velocity = directionVector * BulletSpeed }
        );
    }

    private static void RedrawScene()
    {
        if (engineFacade == null)
            return;

        // ═══════════════════════════════════════════════════════════════════════════
        // SHIP RENDERING WITH DEDICATED SHADER MODE
        // ═══════════════════════════════════════════════════════════════════════════

        DrawShip();

        // ═══════════════════════════════════════════════════════════════════════════
        // BULLET RENDERING WITH SEPARATE SHADER MODE
        // ═══════════════════════════════════════════════════════════════════════════

        DrawBullets();
    }

    private static void DrawShip()
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // DYNAMIC GEOMETRY TRANSFORMATION PIPELINE
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // This demonstrates the standard 2D transformation pipeline used in computer
        // graphics for rotating objects. The process applies rotation mathematics
        // to each vertex of the ship model.

        var vertexBuffer = new List<float>();

        // Transform each vertex through the rotation matrix
        // Standard 2D rotation matrix: [cos(θ) -sin(θ)]
        //                              [sin(θ)  cos(θ)]
        foreach (var vertex in shipModel)
        {
            // Apply 2D rotation transformation to local vertex coordinates
            float x = vertex.X * MathF.Cos(shipRotation) - vertex.Y * MathF.Sin(shipRotation);
            float y = vertex.X * MathF.Sin(shipRotation) + vertex.Y * MathF.Cos(shipRotation);

            // Add transformed coordinates to vertex buffer for GPU upload
            vertexBuffer.Add(x);
            vertexBuffer.Add(y);
        }

        // Skip rendering if geometry construction failed
        if (vertexBuffer.Count == 0)
            return;

        // Configure ship-specific rendering state and submit to GPU
        engineFacade!.Renderer.SetShaderMode(_shipShaderMode);
        engineFacade.Renderer.SetColor(new Vector4D<float>(0.8f, 0.8f, 1.0f, 1f)); // Light blue for ship identification
        engineFacade.Renderer.UpdateVertices(vertexBuffer.ToArray());
        engineFacade.Renderer.Draw();
    }

    private static void DrawBullets()
    {
        // Early exit for performance: skip rendering if no bullets exist
        if (activeBullets.Count == 0)
            return;

        // ═══════════════════════════════════════════════════════════════════════════
        // EFFICIENT BATCH RENDERING SYSTEM
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // This demonstrates batch rendering optimization: instead of individual draw
        // calls per bullet, all bullets are combined into a single vertex buffer
        // and rendered in one GPU draw call for optimal performance.

        var vertexBuffer = new List<float>();

        // Construct all bullet geometries as quad primitives (two triangles each)
        foreach (var bullet in activeBullets)
        {
            // Define bullet as small square centered at bullet position
            const float halfSize = 0.02f;

            // Build quad as two triangles with shared vertices for efficiency
            // Triangle 1: Bottom-left → Top-left → Top-right
            // Triangle 2: Bottom-left → Top-right → Bottom-right
            vertexBuffer.AddRange(
                new[]
                {
                    // First triangle vertices
                    bullet.Position.X - halfSize, bullet.Position.Y - halfSize,  // Bottom-left
                    bullet.Position.X + halfSize, bullet.Position.Y - halfSize,  // Bottom-right
                    bullet.Position.X + halfSize, bullet.Position.Y + halfSize,  // Top-right

                    // Second triangle vertices (completing the quad)
                    bullet.Position.X - halfSize, bullet.Position.Y - halfSize,  // Bottom-left (shared)
                    bullet.Position.X + halfSize, bullet.Position.Y + halfSize,  // Top-right (shared)
                    bullet.Position.X - halfSize, bullet.Position.Y + halfSize,  // Top-left
                }
            );
        }

        // Configure bullet-specific rendering state and submit batch to GPU
        engineFacade!.Renderer.SetShaderMode(_bulletShaderMode);
        engineFacade.Renderer.SetColor(new Vector4D<float>(1f, 1f, 0.3f, 1f)); // Yellow projectiles for visibility
        engineFacade.Renderer.UpdateVertices(vertexBuffer.ToArray());
        engineFacade.Renderer.Draw();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ADAPTIVE VISUAL EFFECTS MANAGEMENT SYSTEM
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // This system demonstrates how different shader modes can be applied to different
    // object types within the same scene, showcasing the flexibility of modern
    // graphics engines for educational and artistic purposes.

    private static void CycleVisualEffects()
    {
        _effectModeIndex = (_effectModeIndex + 1) % _availableShaderModes.Length;

        // Cycle through carefully chosen effect combinations to highlight different
        // graphics programming concepts and visual techniques
        switch (_effectModeIndex)
        {
            case 0: // Baseline mode: Standard rasterization for both objects
                _shipShaderMode = ShaderMode.Normal;
                _bulletShaderMode = ShaderMode.Normal;
                Console.WriteLine("📊 Effect Mode: Normal rendering (baseline - standard rasterization)");
                Console.WriteLine("   • Ship: Standard triangle rasterization");
                Console.WriteLine("   • Bullets: Standard quad rasterization");
                Console.WriteLine("   • Demonstrates: Basic GPU geometry processing");
                break;

            case 1: // Mixed mode: Demonstrates selective effect application
                _shipShaderMode = ShaderMode.Normal;
                _bulletShaderMode = ShaderMode.SoftGlow;
                Console.WriteLine("📊 Effect Mode: Selective enhancement (mixed rendering)");
                Console.WriteLine("   • Ship: Normal rendering (geometric clarity)");
                Console.WriteLine("   • Bullets: SoftGlow effect (projectile emphasis)");
                Console.WriteLine("   • Demonstrates: Per-object shader mode assignment");
                break;

            case 2: // Advanced mode: Full post-processing demonstration
                _shipShaderMode = ShaderMode.SoftGlow;
                _bulletShaderMode = ShaderMode.Bloom;
                Console.WriteLine("📊 Effect Mode: Advanced effects (post-processing showcase)");
                Console.WriteLine("   • Ship: SoftGlow effect (subtle enhancement)");
                Console.WriteLine("   • Bullets: Bloom effect (dramatic HDR glow)");
                Console.WriteLine("   • Demonstrates: Multi-pass post-processing pipeline");
                break;
        }
        Console.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GAME OBJECT DATA STRUCTURES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Represents a projectile in flight with position and velocity state.
    ///
    /// DESIGN PRINCIPLES:
    /// - Minimal data: Only essential state for physics simulation
    /// - Value semantics: Immutable position updates through assignment
    /// - Performance oriented: Lightweight structure for efficient collection storage
    /// - Physics integration: Velocity-based movement for frame-rate independence
    /// </summary>
    private class Bullet
    {
        /// <summary>Current position in NDC coordinate space (-1 to +1 range)</summary>
        public Vector2D<float> Position { get; set; }

        /// <summary>Movement velocity in NDC units per second</summary>
        public Vector2D<float> Velocity { get; set; }
    }

    /// <summary>
    /// Cardinal directions for discrete ship orientation state.
    ///
    /// DESIGN RATIONALE:
    /// - Simplified controls: Reduces input complexity for educational focus
    /// - Predictable behavior: Deterministic rotation angles for each direction
    /// - Clear mapping: Direct correspondence between input keys and directions
    /// - Educational value: Demonstrates enumeration usage in game state management
    /// </summary>
    private enum Direction
    {
        Up,    // Default pointing direction (+Y axis)
        Right, // 90° clockwise rotation (+X axis)
        Down,  // 180° rotation (-Y axis)
        Left,  // 270° clockwise rotation (-X axis)
    }
}
