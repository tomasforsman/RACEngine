using Rac.ECS.Core;
using Rac.ECS.Systems;
using PupperQuest.Components;
using Silk.NET.Maths;

namespace PupperQuest.Systems;

/// <summary>
/// Handles grid-based movement with smooth animation interpolation.
/// Processes movement commands and updates both grid positions and visual positions.
/// </summary>
/// <remarks>
/// Educational Note: Grid-based movement combines discrete logical positions with 
/// smooth visual transitions. This provides the strategic benefits of grid-based 
/// gameplay while maintaining visual polish.
/// 
/// The system separates game logic (grid positions) from presentation (visual positions)
/// following the separation of concerns principle common in game architecture.
/// </remarks>
public class GridMovementSystem : ISystem
{
    private const float TileSize = 1.0f;
    private IWorld _world = null!;
    private readonly Dictionary<int, Vector2D<float>> _visualPositions = new();

    public void Initialize(IWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        
        // Initialize visual positions for all entities with grid positions
        foreach (var (entity, gridPos) in _world.Query<GridPositionComponent>())
        {
            _visualPositions[entity.Id] = gridPos.ToWorldPosition(TileSize);
        }
    }

    public void Update(float deltaTime)
    {
        // Process movement for entities with movement components
        foreach (var (entity, gridPos, movement) in _world.Query<GridPositionComponent, MovementComponent>())
        {
            if (movement.Direction == Vector2D<int>.Zero)
                continue;

            // Calculate target position
            var targetGridPos = new GridPositionComponent(
                gridPos.X + movement.Direction.X,
                gridPos.Y + movement.Direction.Y);

            // Check for collision before moving
            if (IsValidMove(targetGridPos))
            {
                // Update grid position (single step)
                _world.SetComponent(entity, targetGridPos);
                
                // Clear movement immediately after single step
                var clearedMovement = movement with { Direction = Vector2D<int>.Zero, MoveTimer = 0 };
                _world.SetComponent(entity, clearedMovement);
            }
            else
            {
                // Collision detected, stop movement
                var stoppedMovement = movement with { Direction = Vector2D<int>.Zero, MoveTimer = 0 };
                _world.SetComponent(entity, stoppedMovement);
            }
        }

        // Update visual positions for smooth animation
        UpdateVisualPositions(deltaTime);
    }

    public void Shutdown(IWorld world)
    {
        _visualPositions.Clear();
    }

    private bool IsValidMove(GridPositionComponent targetPos)
    {
        // Check for wall tiles at target position
        foreach (var (_, tile, tileGridPos) in _world.Query<TileComponent, GridPositionComponent>())
        {
            if (tileGridPos.X == targetPos.X && tileGridPos.Y == targetPos.Y)
            {
                return tile.IsPassable;
            }
        }

        // Check for other entities at target position
        foreach (var (_, otherGridPos) in _world.Query<GridPositionComponent>())
        {
            if (otherGridPos.X == targetPos.X && otherGridPos.Y == targetPos.Y)
            {
                return false; // Position occupied
            }
        }

        return true; // Valid move
    }

    private void UpdateVisualPositions(float deltaTime)
    {
        foreach (var (entity, gridPos) in _world.Query<GridPositionComponent>())
        {
            var targetWorldPos = gridPos.ToWorldPosition(TileSize);
            
            if (!_visualPositions.ContainsKey(entity.Id))
            {
                _visualPositions[entity.Id] = targetWorldPos;
                continue;
            }

            var currentVisualPos = _visualPositions[entity.Id];
            
            // Smooth interpolation to target position
            const float lerpSpeed = 8.0f;
            var newVisualPos = Vector2D.Lerp(currentVisualPos, targetWorldPos, deltaTime * lerpSpeed);
            
            _visualPositions[entity.Id] = newVisualPos;

            // Update transform component for rendering
            if (_world.HasComponent<Rac.ECS.Components.TransformComponent>(entity))
            {
                if (_world.TryGetComponent<Rac.ECS.Components.TransformComponent>(entity, out var transform))
                {
                    var newTransform = transform with { LocalPosition = newVisualPos };
                    _world.SetComponent(entity, newTransform);
                }
            }
        }
    }
}