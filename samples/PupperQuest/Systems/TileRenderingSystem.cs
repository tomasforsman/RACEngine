using Rac.ECS.Core;
using Rac.ECS.Systems;
using Rac.ECS.Components;
using Rac.Engine;
using PupperQuest.Components;
using Rac.Rendering;
using Rac.Rendering.Geometry;
using Silk.NET.Maths;

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
        // Clear the screen
        _engine.Renderer.Clear();

        // Set game camera for world-space rendering
        _engine.Renderer.SetActiveCamera(_engine.CameraManager.GameCamera);

        // Render tiles first (background)
        RenderTiles();

        // Render items (middle layer)
        RenderItems();

        // Render enemies (foreground)
        RenderEnemies();

        // Render player (top layer)
        RenderPlayer();
    }

    private void RenderTiles()
    {
        foreach (var (entity, tile, transform, sprite) in _world.Query<TileComponent, TransformComponent, SpriteComponent>())
        {
            RenderSprite(transform, sprite);
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
        // Create a rectangle using the proper FullVertex structure
        var position = transform.LocalPosition;
        var size = sprite.Size;
        
        // Generate vertices using GeometryGenerators with proper FullVertex structure
        var vertices = GeometryGenerators.CreateRectangle(size.X, size.Y, sprite.Color);
        
        // Transform vertices to world position
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new FullVertex(
                vertices[i].Position + position,  // Apply position offset
                vertices[i].TexCoord,             // Keep original texture coordinates
                vertices[i].Color                 // Keep original color
            );
        }

        // Render using the structured vertex data
        _engine.Renderer.UpdateVertices(vertices);
        _engine.Renderer.Draw();
    }
}