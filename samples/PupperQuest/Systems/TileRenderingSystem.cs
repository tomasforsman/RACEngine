using Rac.ECS.Core;
using Rac.ECS.Systems;
using Rac.ECS.Components;
using Rac.Engine;
using PupperQuest.Components;
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
        // Create a simple rectangle for the sprite
        var position = transform.LocalPosition;
        var size = sprite.Size;
        
        // Generate vertices for a rectangle
        var vertices = new float[]
        {
            // Bottom-left triangle
            position.X - size.X/2, position.Y - size.Y/2,  // Bottom-left
            position.X + size.X/2, position.Y - size.Y/2,  // Bottom-right
            position.X + size.X/2, position.Y + size.Y/2,  // Top-right
            
            // Top-right triangle  
            position.X - size.X/2, position.Y - size.Y/2,  // Bottom-left
            position.X + size.X/2, position.Y + size.Y/2,  // Top-right
            position.X - size.X/2, position.Y + size.Y/2,  // Top-left
        };

        // Set color and render
        _engine.Renderer.SetColor(sprite.Color);
        _engine.Renderer.UpdateVertices(vertices);
        _engine.Renderer.Draw();
    }
}