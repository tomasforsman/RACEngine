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
// - WASD: Camera movement (pan world view for exploration)  
// - Arrow Keys: Ship rotation (discrete directional control)
// - Q/E: Camera zoom in/out (scale world view)
// - Space: Toggle auto-fire mode (persistent state change)
// - R: Reset camera to origin (return to default view)
// - Tab: Toggle UI overlay visibility (show/hide camera information)
// - Mouse Click: Spawn objects at world coordinates (coordinate transformation demo)
// - V or 1/2/3: Cycle through visual effect combinations (shader demonstration)
//
// EDUCATIONAL OBJECTIVES:
// - Understand fundamental game loop structure and timing
// - Experience real-time input handling and state management  
// - Observe 2D graphics transformations and coordinate systems
// - Learn dual-camera rendering with world/UI separation
// - Experience coordinate transformation between screen and world space
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

    // ═══════════════════════════════════════════════════════════════════════════
    // CAMERA SYSTEM INTEGRATION STATE
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // Camera-related state for demonstrating dual-camera rendering and world/UI separation.
    // These additions showcase the camera system without disrupting existing ship mechanics.

    // UI overlay visibility toggle (Tab key control)
    private static bool showUIOverlay = true;

    // Collection of world objects spawned by mouse clicks for demonstration
    private static readonly List<WorldObject> spawnedObjects = new();

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
        Console.WriteLine("   • Learn dual-camera rendering with world/UI separation");
        Console.WriteLine("   • Experience coordinate transformation (screen ↔ world space)");
        Console.WriteLine();
        Console.WriteLine("🎮 INTERACTIVE CONTROLS:");
        Console.WriteLine("   WASD:        Move camera (pan world view)");
        Console.WriteLine("   Arrow Keys:  Rotate ship direction (preserved functionality)");
        Console.WriteLine("   Q/E:         Zoom camera in/out");
        Console.WriteLine("   Space:       Toggle auto-fire mode (persistent state)");
        Console.WriteLine("   R:           Reset camera to origin");
        Console.WriteLine("   Tab:         Toggle UI overlay visibility");
        Console.WriteLine("   Mouse Click: Spawn objects at world coordinates");
        Console.WriteLine("   V or 1/2/3:  Cycle visual effects (shader demonstration)");
        Console.WriteLine();
        Console.WriteLine("🔧 TECHNICAL FEATURES DEMONSTRATED:");
        Console.WriteLine("   • Frame-rate independent physics using delta time");
        Console.WriteLine("   • Dual-camera system: Game world + UI overlay rendering");
        Console.WriteLine("   • Screen-to-world coordinate transformation");
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

        // ─── Hook Mouse Input for Click-to-Spawn ────────────────
        engineFacade.LeftClickEvent += OnMouseClick;

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

        if (engineFacade == null)
            return;

        // ═══════════════════════════════════════════════════════════════════════════
        // CAMERA MOVEMENT CONTROLS (WASD)
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // Camera movement controls using WASD keys for intuitive navigation.
        // Each key press moves the camera by a fixed increment for responsive control.
        
        const float cameraSpeed = 0.1f;
        var camera = engineFacade.CameraManager.GameCamera;
        
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
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // SHIP ROTATION CONTROLS (ARROW KEYS)
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // Ship rotation controls moved to arrow keys to preserve existing functionality
        // while freeing up WASD for camera movement as requested.

        shipDirection = key switch
        {
            Key.Up => Direction.Up,
            Key.Right => Direction.Right,
            Key.Down => Direction.Down,
            Key.Left => Direction.Left,
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

        // ═══════════════════════════════════════════════════════════════════════════
        // CAMERA ZOOM CONTROLS (Q/E)
        // ═══════════════════════════════════════════════════════════════════════════
        
        const float zoomSpeed = 0.1f;
        if (key == Key.Q) // Zoom out
        {
            camera.Zoom = Math.Max(0.1f, camera.Zoom - zoomSpeed);
        }
        else if (key == Key.E) // Zoom in
        {
            camera.Zoom = Math.Min(5f, camera.Zoom + zoomSpeed);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ADDITIONAL CONTROLS
        // ═══════════════════════════════════════════════════════════════════════════

        // Toggle auto-fire and spawn one bullet immediately (preserved functionality)
        if (key == Key.Space)
        {
            isAutoFireEnabled = !isAutoFireEnabled;
            SpawnBulletInCurrentDirection();
        }

        // Reset camera to origin (R key)
        if (key == Key.R)
        {
            camera.Position = Vector2D<float>.Zero;
            camera.Zoom = 1f;
            camera.Rotation = 0f;
            Console.WriteLine("Camera reset to origin");
        }

        // Toggle UI overlay visibility (Tab key)
        if (key == Key.Tab)
        {
            showUIOverlay = !showUIOverlay;
            Console.WriteLine($"UI overlay: {(showUIOverlay ? "ON" : "OFF")}");
        }

        // Shader mode controls (both V for legacy and 1/2/3 for new style)
        if (key == Key.V)
        {
            CycleVisualEffects();
        }
        else if (key == Key.Number1)
        {
            _shipShaderMode = ShaderMode.Normal;
            _bulletShaderMode = ShaderMode.Normal;
            Console.WriteLine("Shader Mode: Normal");
        }
        else if (key == Key.Number2)
        {
            _shipShaderMode = ShaderMode.Normal;
            _bulletShaderMode = ShaderMode.SoftGlow;
            Console.WriteLine("Shader Mode: Mixed (Ship: Normal, Bullets: SoftGlow)");
        }
        else if (key == Key.Number3)
        {
            _shipShaderMode = ShaderMode.SoftGlow;
            _bulletShaderMode = ShaderMode.Bloom;
            Console.WriteLine("Shader Mode: Advanced (Ship: SoftGlow, Bullets: Bloom)");
        }
    }

    private static void OnMouseClick(Vector2D<float> screenPosition)
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // COORDINATE TRANSFORMATION DEMONSTRATION
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // This demonstrates the essential coordinate transformation from screen space
        // (mouse cursor position) to world space (game object position), taking into
        // account the current camera transformation state.

        if (engineFacade == null)
            return;

        // Get current window size for coordinate transformation
        var windowSize = engineFacade.WindowManager.Size;
        
        // Convert screen coordinates to world coordinates using camera manager
        var worldPosition = engineFacade.CameraManager.ScreenToGameWorld(
            screenPosition, 
            windowSize.X, 
            windowSize.Y
        );

        // Spawn a new object at the clicked world position
        var newObject = new WorldObject
        {
            Position = worldPosition,
            Color = new Vector4D<float>(1f, 0.5f, 0.2f, 1f), // Orange color for visibility
            Size = 0.03f,
            CreationTime = 0f // Could be used for animations
        };

        spawnedObjects.Add(newObject);

        Console.WriteLine($"Spawned object at world: {worldPosition} (screen: {screenPosition})");
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

        // Clear the render target before rendering
        engineFacade.Renderer.Clear();

        // ═══════════════════════════════════════════════════════════════════════════
        // PASS 1: RENDER GAME WORLD (with camera transformations)
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // Set the game camera to apply world-space transformations (pan, zoom, rotate).
        // All objects rendered in this pass will be affected by camera movement.

        engineFacade.Renderer.SetActiveCamera(engineFacade.CameraManager.GameCamera);

        // Render background grid for visual reference
        DrawBackgroundGrid();

        // Render spawned world objects (click-to-spawn demonstration)
        DrawSpawnedObjects();

        // Render game objects with existing shader modes
        DrawShip();
        DrawBullets();

        // ═══════════════════════════════════════════════════════════════════════════
        // PASS 2: RENDER UI OVERLAY (screen-space, camera-independent)
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // Set the UI camera for screen-space rendering that remains fixed regardless
        // of game camera transformations. Perfect for HUD, menus, and debug information.

        if (showUIOverlay)
        {
            engineFacade.Renderer.SetActiveCamera(engineFacade.CameraManager.UICamera);
            DrawUIOverlay();
        }

        // Finalize the frame
        engineFacade.Renderer.FinalizeFrame();
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
    // CAMERA SYSTEM DEMONSTRATION RENDERING
    // ═══════════════════════════════════════════════════════════════════════════

    private static void DrawBackgroundGrid()
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // WORLD SPACE REFERENCE GRID
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // This grid demonstrates world-space rendering that moves with the camera.
        // It provides visual reference for camera transformations (pan, zoom, rotate).

        var gridVertices = new List<float>();
        const float majorGridSize = 8f;
        const float majorGridSpacing = 1f;
        const float minorGridSpacing = 0.2f;

        // Major grid lines (every 1 unit) - slightly more visible
        for (float y = -majorGridSize; y <= majorGridSize; y += majorGridSpacing)
        {
            gridVertices.AddRange(new[] { -majorGridSize, y, majorGridSize, y });
        }
        for (float x = -majorGridSize; x <= majorGridSize; x += majorGridSpacing)
        {
            gridVertices.AddRange(new[] { x, -majorGridSize, x, majorGridSize });
        }

        // Render major grid lines with subtle but visible color
        engineFacade!.Renderer.SetShaderMode(ShaderMode.Normal);
        engineFacade.Renderer.SetColor(new Vector4D<float>(0.4f, 0.4f, 0.4f, 0.8f));
        engineFacade.Renderer.UpdateVertices(gridVertices.ToArray());
        engineFacade.Renderer.Draw();

        // Minor grid lines (every 0.2 units) - very subtle
        gridVertices.Clear();
        for (float y = -majorGridSize; y <= majorGridSize; y += minorGridSpacing)
        {
            if (y % majorGridSpacing != 0) // Skip major grid line positions
            {
                gridVertices.AddRange(new[] { -majorGridSize, y, majorGridSize, y });
            }
        }
        for (float x = -majorGridSize; x <= majorGridSize; x += minorGridSpacing)
        {
            if (x % majorGridSpacing != 0) // Skip major grid line positions
            {
                gridVertices.AddRange(new[] { x, -majorGridSize, x, majorGridSize });
            }
        }

        // Render minor grid lines with very subtle color
        engineFacade.Renderer.SetColor(new Vector4D<float>(0.25f, 0.25f, 0.25f, 0.4f));
        engineFacade.Renderer.UpdateVertices(gridVertices.ToArray());
        engineFacade.Renderer.Draw();
    }

    private static void DrawSpawnedObjects()
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

        // Render all spawned objects
        engineFacade!.Renderer.SetShaderMode(ShaderMode.SoftGlow);
        engineFacade.Renderer.SetColor(new Vector4D<float>(1f, 0.5f, 0.2f, 1f)); // Orange
        engineFacade.Renderer.UpdateVertices(vertexBuffer.ToArray());
        engineFacade.Renderer.Draw();
    }

    private static void DrawUIOverlay()
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // SCREEN-SPACE UI OVERLAY DEMONSTRATION
        // ═══════════════════════════════════════════════════════════════════════════
        //
        // This UI remains fixed in screen space regardless of camera transformations.
        // Uses geometric shapes as placeholders for text-based information display.

        if (engineFacade == null)
            return;

        var camera = engineFacade.CameraManager.GameCamera;
        
        // UI Panel background (top-left corner)
        DrawUIQuad(-380f, 250f, 200f, 120f, new Vector4D<float>(0.1f, 0.1f, 0.3f, 0.8f));

        // Camera position indicators (colored bars representing X and Y)
        float posX = Math.Clamp(camera.Position.X * 50f, -80f, 80f);
        float posY = Math.Clamp(camera.Position.Y * 50f, -80f, 80f);
        
        DrawUIQuad(-350f, 220f, posX, 10f, new Vector4D<float>(1f, 0f, 0f, 1f)); // X position (red)
        DrawUIQuad(-350f, 200f, posY, 10f, new Vector4D<float>(0f, 1f, 0f, 1f)); // Y position (green)

        // Zoom level indicator (horizontal bar)
        float zoomBarWidth = camera.Zoom * 60f;
        DrawUIQuad(-350f, 180f, zoomBarWidth, 8f, new Vector4D<float>(0f, 0f, 1f, 1f)); // Zoom (blue)

        // Controls indicator (small rectangles)
        DrawUIQuad(-380f, 140f, 15f, 5f, new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f)); // "WASD: Camera"
        DrawUIQuad(-380f, 130f, 15f, 5f, new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f)); // "Q/E: Zoom"
        DrawUIQuad(-380f, 120f, 15f, 5f, new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f)); // "R: Reset"
        DrawUIQuad(-380f, 110f, 15f, 5f, new Vector4D<float>(0.8f, 0.8f, 0.8f, 1f)); // "Tab: UI Toggle"

        // Crosshair at screen center
        DrawUICrosshair();
    }

    private static void DrawUIQuad(float x, float y, float width, float height, Vector4D<float> color)
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

        engineFacade!.Renderer.SetShaderMode(ShaderMode.Normal);
        engineFacade.Renderer.SetColor(color);
        engineFacade.Renderer.UpdateVertices(vertices);
        engineFacade.Renderer.Draw();
    }

    private static void DrawUICrosshair()
    {
        var crosshairVertices = new float[]
        {
            -20f, 0f, 20f, 0f,    // Horizontal line
            0f, -20f, 0f, 20f     // Vertical line
        };

        engineFacade!.Renderer.SetShaderMode(ShaderMode.Normal);
        engineFacade.Renderer.SetColor(new Vector4D<float>(1f, 1f, 1f, 0.7f));
        engineFacade.Renderer.UpdateVertices(crosshairVertices);
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

    /// <summary>
    /// Represents objects spawned in the world via mouse clicks for camera system demonstration.
    ///
    /// DESIGN PRINCIPLES:
    /// - Minimal data: Essential state for rendering and lifetime management
    /// - World-space positioning: Affected by camera transformations unlike UI elements
    /// - Visual feedback: Provides clear demonstration of coordinate transformation
    /// - Educational value: Shows difference between world and screen space
    /// </summary>
    private class WorldObject
    {
        /// <summary>Position in world coordinate space (affected by camera transformations)</summary>
        public Vector2D<float> Position { get; set; }

        /// <summary>Visual color for rendering identification</summary>
        public Vector4D<float> Color { get; set; }

        /// <summary>Size for rendering (diameter of the object)</summary>
        public float Size { get; set; } = 0.05f;

        /// <summary>Creation time for potential animation or lifetime management</summary>
        public float CreationTime { get; set; }
    }
}
