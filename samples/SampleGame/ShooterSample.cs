// File: samples/SampleGame/ShooterSample.cs

using Rac.Core.Manager;
using Rac.Engine;
using Rac.Input.Service;
using Rac.Input.State;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace SampleGame;

public static class ShooterSample
{
    private const float BulletSpeed = 0.75f;

    private const float FireInterval = 0.2f;

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
        // ─── Setup Engine & Services ────────────────────────────
        var windowManager = new WindowManager();
        var inputService = new SilkInputService();
        var configurationManager = new ConfigManager();

        engineFacade = new EngineFacade(windowManager, inputService, configurationManager);

        // ─── Initial Draw when Renderer is ready ────────────────
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

        var vertexBuffer = new List<float>();

        // Draw the ship model, rotated by shipRotation
        foreach (var vertex in shipModel)
        {
            float x = vertex.X * MathF.Cos(shipRotation) - vertex.Y * MathF.Sin(shipRotation);
            float y = vertex.X * MathF.Sin(shipRotation) + vertex.Y * MathF.Cos(shipRotation);
            vertexBuffer.Add(x);
            vertexBuffer.Add(y);
        }

        // Draw each bullet as two triangles
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

        if (vertexBuffer.Count == 0)
            return;

        engineFacade.Renderer.SetColor(new Vector4D<float>(1f, 1f, 1f, 1f));
        engineFacade.Renderer.UpdateVertices(vertexBuffer.ToArray());
        engineFacade.Renderer.Draw();
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
