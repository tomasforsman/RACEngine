using Rac.ECS.Core;
using Rac.ECS.Systems;
using Rac.ECS.Components;
using Rac.Engine;
using PupperQuest.Components;
using Rac.Rendering.Shader;
using Rac.Rendering;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace PupperQuest.Systems;

/// <summary>
/// Handles rendering of tiles and entities in the grid-based world.
/// Demonstrates basic sprite rendering using the RACEngine rendering pipeline.
/// </summary>
/// <remarks>
/// Educational Note: Rendering systems in ECS separate visual presentation from game logic.
/// This system queries for entities with visual components (SpriteComponent, TransformComponent)
/// and submits them to the renderer for display.
///
/// The rendering pipeline processes sprites as colored rectangles, which is perfect for
/// prototyping and demonstrates that compelling gameplay doesn't require complex graphics.
/// </remarks>
public class TileRenderingSystem : ISystem
{
    private IWorld _world = null!;
    private EngineFacade _engine = null!;
    private bool _shaderInitialized = false;

    public TileRenderingSystem(EngineFacade engine)
    {
        _engine = engine;
    }

    public void Initialize(IWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));

        // Subscribe to the engine's render event
        _engine.RenderEvent += OnRender;
    }

    public void Update(float deltaTime)
    {
        // Rendering happens in the RenderEvent callback, not in Update
    }

    public void Shutdown(IWorld world)
    {
        _engine.RenderEvent -= OnRender;
    }

    private void OnRender(float deltaSeconds)
    {
        // Initialize shader mode on first render call
        if (!_shaderInitialized)
        {
            // Check if shaders are available
            var isNormalAvailable = ShaderLoader.IsShaderModeAvailable(ShaderMode.Normal);
            var directoryStatus = ShaderLoader.ValidateShaderDirectory();

            Console.WriteLine($"🔍 Shader validation: Normal={isNormalAvailable}, Directory exists={directoryStatus.Exists}");
            Console.WriteLine($"🔍 Vertex shader exists={directoryStatus.HasVertexShader}, Fragment count={directoryStatus.FragmentShaderCount}");

            // Workaround: Force shader state refresh by switching modes
            _engine.Renderer.SetShaderMode(ShaderMode.DebugUV);
            _engine.Renderer.SetPrimitiveType(PrimitiveType.Triangles);
            _engine.Renderer.SetShaderMode(ShaderMode.Normal);
            _engine.Renderer.SetPrimitiveType(PrimitiveType.Triangles);
            Console.WriteLine("🎨 Shader mode initialized with workaround and set to Normal");
            _shaderInitialized = true;
        }

        // Clear the screen
        _engine.Renderer.Clear();

        // Set game camera for world-space rendering
        _engine.Renderer.SetActiveCamera(_engine.CameraManager.GameCamera);

        // Set shader mode every frame (like boid sample does)
        _engine.Renderer.SetShaderMode(ShaderMode.Normal);
        _engine.Renderer.SetPrimitiveType(PrimitiveType.Triangles);

        // Debug: Print camera settings on first few frames
        if (deltaSeconds < 1.0f) // Only during initial frames
        {
            var camera = _engine.CameraManager.GameCamera;
            //Console.WriteLine($"🎥 Camera: Position=({camera.Position.X:F2}, {camera.Position.Y:F2}), Zoom={camera.Zoom:F3}");
        }

        // Render tiles first (background)
        RenderTiles();

        // Render items (middle layer)
        RenderItems();

        // Render enemies (foreground)
        RenderEnemies();

        // Render player (top layer)
        RenderPlayer();

        // Finalize the frame to ensure all rendering commands are processed
        _engine.Renderer.FinalizeFrame();
    }

    private void RenderTiles()
    {
        int tileCount = 0;
        foreach (var (entity, tile, transform, sprite) in _world.Query<TileComponent, TransformComponent, SpriteComponent>())
        {
            RenderSprite(transform, sprite);
            tileCount++;
        }

        // Debug output for first few frames
        if (tileCount > 0)
        {
            //Console.WriteLine($"🎨 Rendered {tileCount} tiles");
        }
    }

    private void RenderItems()
    {
        foreach (var (entity, item, transform, sprite) in _world.Query<ItemComponent, TransformComponent, SpriteComponent>())
        {
            RenderSprite(transform, sprite);
        }
    }

    private void RenderEnemies()
    {
        foreach (var (entity, enemy, transform, sprite) in _world.Query<EnemyComponent, TransformComponent, SpriteComponent>())
        {
            RenderSprite(transform, sprite);
        }
    }

    private void RenderPlayer()
    {
        foreach (var (entity, puppy, transform, sprite) in _world.Query<PuppyComponent, TransformComponent, SpriteComponent>())
        {
            RenderSprite(transform, sprite);
        }
    }

    private void RenderSprite(TransformComponent transform, SpriteComponent sprite)
    {
        // Create a simple rectangle for the sprite
        var position = transform.LocalPosition;
        var size = sprite.Size;

        // Generate vertices using FullVertex - match ContainerSample triangle order
        var vertices = new FullVertex[]
        {
            // Triangle 1: Bottom-left → Bottom-right → Top-left
            new FullVertex(new Vector2D<float>(position.X - size.X/2, position.Y - size.Y/2), new Vector2D<float>(0f, 0f), sprite.Color),  // Bottom-left
            new FullVertex(new Vector2D<float>(position.X + size.X/2, position.Y - size.Y/2), new Vector2D<float>(1f, 0f), sprite.Color),  // Bottom-right
            new FullVertex(new Vector2D<float>(position.X - size.X/2, position.Y + size.Y/2), new Vector2D<float>(0f, 1f), sprite.Color),  // Top-left

            // Triangle 2: Bottom-right → Top-right → Top-left
            new FullVertex(new Vector2D<float>(position.X + size.X/2, position.Y + size.Y/2), new Vector2D<float>(0f, 0f), sprite.Color),  // Top-right
            new FullVertex(new Vector2D<float>(position.X + size.X/2, position.Y - size.Y/2), new Vector2D<float>(1f, 0f), sprite.Color),  // Bottom-right
            new FullVertex(new Vector2D<float>(position.X - size.X/2, position.Y + size.Y/2), new Vector2D<float>(0f, 1f), sprite.Color),  // Top-left
        };

        // Render using FullVertex array like BoidSample
        _engine.Renderer.UpdateVertices(vertices);
        _engine.Renderer.Draw();
    }
}
