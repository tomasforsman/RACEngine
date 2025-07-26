using Rac.ECS.Core;
using Rac.ECS.Systems;
using PupperQuest.Components;
using Silk.NET.Maths;

namespace PupperQuest.Systems;

/// <summary>
/// Implements basic AI behaviors for enemy entities.
/// Demonstrates simple state-based AI suitable for turn-based roguelike gameplay.
/// </summary>
/// <remarks>
/// Educational Note: AI in roguelike games focuses on tactical challenge rather than realism.
/// Simple behaviors like chase, flee, and patrol create engaging gameplay patterns.
/// 
/// Academic Reference: "AI for Game Developers" (Bourg & Seemann, 2004)
/// State-based AI provides predictable yet challenging enemy behaviors.
/// </remarks>
public class SimpleAISystem : ISystem
{
    private IWorld _world = null!;
    private readonly Random _random = new();

    public void Initialize(IWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    public void Update(float deltaTime)
    {
        // Find the player position for AI targeting
        Entity? playerEntity = null;
        GridPositionComponent playerPos = default;
        
        foreach (var (entity, puppy, gridPos) in _world.Query<PuppyComponent, GridPositionComponent>())
        {
            playerEntity = entity;
            playerPos = gridPos;
            break;
        }

        if (playerEntity == null) return;

        // Process AI for each enemy
        foreach (var (entity, enemy, ai, gridPos, movement) in _world.Query<EnemyComponent, AIComponent, GridPositionComponent, MovementComponent>())
        {
            // Skip if already moving
            if (movement.MoveTimer > 0) continue;

            var direction = CalculateAIDirection(ai, gridPos, playerPos, enemy);
            
            if (direction != Vector2D<int>.Zero)
            {
                var newMovement = movement with { Direction = direction, MoveTimer = 0.5f };
                _world.SetComponent(entity, newMovement);
            }
        }
    }

    public void Shutdown(IWorld world)
    {
        // No cleanup needed
    }

    private Vector2D<int> CalculateAIDirection(AIComponent ai, GridPositionComponent currentPos, 
        GridPositionComponent playerPos, EnemyComponent enemy)
    {
        return ai.Behavior switch
        {
            AIBehavior.Hostile => CalculateChaseDirection(currentPos, playerPos, enemy.DetectionRange),
            AIBehavior.Flee => CalculateFleeDirection(currentPos, playerPos, enemy.DetectionRange),
            AIBehavior.Patrol => CalculatePatrolDirection(ai, currentPos),
            AIBehavior.Guard => CalculateGuardDirection(currentPos, playerPos, enemy.DetectionRange),
            AIBehavior.Wander => CalculateWanderDirection(),
            _ => Vector2D<int>.Zero
        };
    }

    private Vector2D<int> CalculateChaseDirection(GridPositionComponent currentPos, 
        GridPositionComponent playerPos, int detectionRange)
    {
        var distance = CalculateDistance(currentPos, playerPos);
        
        if (distance > detectionRange) return Vector2D<int>.Zero;

        // Simple pursuit - move one step toward player
        var deltaX = Math.Sign(playerPos.X - currentPos.X);
        var deltaY = Math.Sign(playerPos.Y - currentPos.Y);

        // Prefer horizontal or vertical movement (no diagonal)
        if (Math.Abs(playerPos.X - currentPos.X) > Math.Abs(playerPos.Y - currentPos.Y))
        {
            return new Vector2D<int>(deltaX, 0);
        }
        else
        {
            return new Vector2D<int>(0, deltaY);
        }
    }

    private Vector2D<int> CalculateFleeDirection(GridPositionComponent currentPos, 
        GridPositionComponent playerPos, int detectionRange)
    {
        var distance = CalculateDistance(currentPos, playerPos);
        
        if (distance > detectionRange) return CalculateWanderDirection();

        // Flee - move away from player
        var deltaX = Math.Sign(currentPos.X - playerPos.X);
        var deltaY = Math.Sign(currentPos.Y - playerPos.Y);

        // Prefer horizontal or vertical movement
        if (Math.Abs(playerPos.X - currentPos.X) > Math.Abs(playerPos.Y - currentPos.Y))
        {
            return new Vector2D<int>(deltaX, 0);
        }
        else
        {
            return new Vector2D<int>(0, deltaY);
        }
    }

    private Vector2D<int> CalculatePatrolDirection(AIComponent ai, GridPositionComponent currentPos)
    {
        if (ai.PatrolRoute.Length == 0) return CalculateWanderDirection();

        // Find closest patrol point and move toward it
        var closestPoint = ai.PatrolRoute
            .OrderBy(point => CalculateDistance(currentPos, new GridPositionComponent(point.X, point.Y)))
            .First();

        var deltaX = Math.Sign(closestPoint.X - currentPos.X);
        var deltaY = Math.Sign(closestPoint.Y - currentPos.Y);

        if (Math.Abs(closestPoint.X - currentPos.X) > Math.Abs(closestPoint.Y - currentPos.Y))
        {
            return new Vector2D<int>(deltaX, 0);
        }
        else
        {
            return new Vector2D<int>(0, deltaY);
        }
    }

    private Vector2D<int> CalculateGuardDirection(GridPositionComponent currentPos, 
        GridPositionComponent playerPos, int detectionRange)
    {
        var distance = CalculateDistance(currentPos, playerPos);
        
        // Only move if player is very close
        if (distance <= 2)
        {
            return CalculateChaseDirection(currentPos, playerPos, detectionRange);
        }
        
        return Vector2D<int>.Zero; // Stay in place
    }

    private Vector2D<int> CalculateWanderDirection()
    {
        // Random movement
        var directions = new[]
        {
            new Vector2D<int>(0, 0),   // Stay still (most common)
            new Vector2D<int>(1, 0),   // East
            new Vector2D<int>(-1, 0),  // West  
            new Vector2D<int>(0, 1),   // South
            new Vector2D<int>(0, -1),  // North
        };

        return directions[_random.Next(directions.Length)];
    }

    private static int CalculateDistance(GridPositionComponent pos1, GridPositionComponent pos2)
    {
        return Math.Abs(pos1.X - pos2.X) + Math.Abs(pos1.Y - pos2.Y); // Manhattan distance
    }
}