// File: samples/SampleGame/ShooterSample.cs
//
// ════════════════════════════════════════════════════════════════════════════════
// ENHANCED SHOOTER SAMPLE - ENGINE FEATURE DEMONSTRATION
// ════════════════════════════════════════════════════════════════════════════════
//
// This sample demonstrates several key engine features through an interactive shooter:
//
// 1. CORE ENGINE INTEGRATION:
//    - EngineFacade usage for simplified engine access
//    - Event-driven architecture (KeyEvent, UpdateEvent, RenderEvent)
//    - Proper resource and lifecycle management
//
// 2. VISUAL EFFECTS SHOWCASE:
//    - Multiple shader modes (Normal, SoftGlow, Bloom)
//    - Separate rendering paths for different object types
//    - Real-time visual effect switching
//
// 3. INPUT AND GAME MECHANICS:
//    - Responsive keyboard input handling
//    - Continuous movement and shooting mechanics
//    - Time-based game logic and physics
//
// 4. RENDERING TECHNIQUES:
//    - Dynamic vertex buffer construction
//    - 2D rotation mathematics
//    - Batched rendering for performance
//
// CONTROLS:
// - WASD/Arrow Keys: Rotate ship direction
// - Space: Toggle auto-fire mode
// - V: Cycle through visual effect modes
//
// ════════════════════════════════════════════════════════════════════════════════

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
    // GAME CONFIGURATION
    // ═══════════════════════════════════════════════════════════════════════════
    
    private const float BulletSpeed = 0.75f;
    private const float FireInterval = 0.2f;

    // ═══════════════════════════════════════════════════════════════════════════
    // VISUAL EFFECTS DEMONSTRATION
    // ═══════════════════════════════════════════════════════════════════════════
    
    private static ShaderMode _shipShaderMode = ShaderMode.Normal;
    private static ShaderMode _bulletShaderMode = ShaderMode.SoftGlow;
    private static readonly ShaderMode[] _availableShaderModes = { ShaderMode.Normal, ShaderMode.SoftGlow, ShaderMode.Bloom };
    private static int _effectModeIndex = 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // ENGINE AND GAME STATE
    // ═══════════════════════════════════════════════════════════════════════════

    // Facade providing World, Update/Render callbacks, KeyEvent, Renderer
    private static EngineFacade? engineFacade;

    // Current facing direction of the ship
    private static Direction shipDirection = Direction.Up;

    // Whether auto‐fire is toggled on
    private static bool isAutoFireEnabled;

    // Time accumulator for auto‐fire spacing
    private static float timeSinceLastShot;

    // Current rotation of the ship in radians
    private static float shipRotation;

    // Active bullets in the scene
    private static readonly List<Bullet> activeBullets = new();

    // Triangle model for the ship, centered at origin
    private static readonly Vector2D<float>[] shipModel = new[]
    {
        new Vector2D<float>(-0.05f, -0.05f),
        new Vector2D<float>(0.05f, -0.05f),
        new Vector2D<float>(0.00f, 0.10f),
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
        // STARTUP INFORMATION
        // ═══════════════════════════════════════════════════════════════════════════
        
        Console.WriteLine("=== SHOOTER SAMPLE - ENGINE FEATURE SHOWCASE ===");
        Console.WriteLine("Controls:");
        Console.WriteLine("  WASD/Arrows: Rotate ship");
        Console.WriteLine("  Space: Toggle auto-fire");
        Console.WriteLine("  V: Cycle visual effects");
        Console.WriteLine($"Ship Mode: {_shipShaderMode}, Bullet Mode: {_bulletShaderMode}");

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
        var vertexBuffer = new List<float>();

        // Build ship triangle with rotation applied
        foreach (var vertex in shipModel)
        {
            float x = vertex.X * MathF.Cos(shipRotation) - vertex.Y * MathF.Sin(shipRotation);
            float y = vertex.X * MathF.Sin(shipRotation) + vertex.Y * MathF.Cos(shipRotation);
            vertexBuffer.Add(x);
            vertexBuffer.Add(y);
        }

        if (vertexBuffer.Count == 0)
            return;

        // Apply ship-specific shader mode and color
        engineFacade!.Renderer.SetShaderMode(_shipShaderMode);
        engineFacade.Renderer.SetColor(new Vector4D<float>(0.8f, 0.8f, 1.0f, 1f)); // Light blue ship
        engineFacade.Renderer.UpdateVertices(vertexBuffer.ToArray());
        engineFacade.Renderer.Draw();
    }

    private static void DrawBullets()
    {
        if (activeBullets.Count == 0)
            return;

        var vertexBuffer = new List<float>();

        // Build all bullet quads (two triangles each)
        foreach (var bullet in activeBullets)
        {
            const float halfSize = 0.02f;
            vertexBuffer.AddRange(
                new[]
                {
                    bullet.Position.X - halfSize,
                    bullet.Position.Y - halfSize,
                    bullet.Position.X + halfSize,
                    bullet.Position.Y - halfSize,
                    bullet.Position.X + halfSize,
                    bullet.Position.Y + halfSize,
                    bullet.Position.X - halfSize,
                    bullet.Position.Y - halfSize,
                    bullet.Position.X + halfSize,
                    bullet.Position.Y + halfSize,
                    bullet.Position.X - halfSize,
                    bullet.Position.Y + halfSize,
                }
            );
        }

        // Apply bullet-specific shader mode and color
        engineFacade!.Renderer.SetShaderMode(_bulletShaderMode);
        engineFacade.Renderer.SetColor(new Vector4D<float>(1f, 1f, 0.5f, 1f)); // Yellow bullets
        engineFacade.Renderer.UpdateVertices(vertexBuffer.ToArray());
        engineFacade.Renderer.Draw();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VISUAL EFFECTS MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    private static void CycleVisualEffects()
    {
        _effectModeIndex = (_effectModeIndex + 1) % _availableShaderModes.Length;
        
        // Cycle through different effect combinations to showcase various modes
        switch (_effectModeIndex)
        {
            case 0: // Basic mode: Normal for both
                _shipShaderMode = ShaderMode.Normal;
                _bulletShaderMode = ShaderMode.Normal;
                Console.WriteLine("Effect Mode: Normal (ship + bullets)");
                break;
                
            case 1: // Mixed mode: Normal ship, SoftGlow bullets
                _shipShaderMode = ShaderMode.Normal;
                _bulletShaderMode = ShaderMode.SoftGlow;
                Console.WriteLine("Effect Mode: Normal ship, SoftGlow bullets");
                break;
                
            case 2: // Advanced mode: SoftGlow ship, Bloom bullets
                _shipShaderMode = ShaderMode.SoftGlow;
                _bulletShaderMode = ShaderMode.Bloom;
                Console.WriteLine("Effect Mode: SoftGlow ship, Bloom bullets");
                break;
        }
    }

    // Represents a bullet in flight
    private class Bullet
    {
        public Vector2D<float> Position { get; set; }
        public Vector2D<float> Velocity { get; set; }
    }

    private enum Direction
    {
        Up,
        Right,
        Down,
        Left,
    }
}
