using Rac.ECS.Core;
using Rac.ECS.Systems;
using PupperQuest.Components;

namespace PupperQuest.Systems;

/// <summary>
/// Manages game state including win/lose conditions and level progression.
/// Handles player interactions with items, enemies, and level exits.
/// </summary>
/// <remarks>
/// Educational Note: Game state management is crucial for providing clear objectives
/// and feedback to players. This system demonstrates how ECS can handle complex
/// game logic through simple component queries and state changes.
/// </remarks>
public class GameStateSystem : ISystem
{
    private IWorld _world = null!;
    public bool IsGameWon { get; private set; }
    public bool IsGameLost { get; private set; }
    public bool ShouldAdvanceLevel { get; private set; }

    public void Initialize(IWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    public void Update(float deltaTime)
    {
        CheckPlayerCollisions();
        CheckWinLoseConditions();
    }

    public void Shutdown(IWorld world)
    {
        // No cleanup needed
    }

    private void CheckPlayerCollisions()
    {
        // Find player
        Entity? playerEntity = null;
        GridPositionComponent playerPos = default;
        PuppyComponent puppy = default;

        foreach (var (entity, puppyComp, gridPos) in _world.Query<PuppyComponent, GridPositionComponent>())
        {
            playerEntity = entity;
            playerPos = gridPos;
            puppy = puppyComp;
            break;
        }

        if (playerEntity == null) return;

        // Check collisions with enemies
        var enemiesToRemove = new List<Entity>();
        foreach (var (enemy, enemyComponent, enemyPos) in _world.Query<EnemyComponent, GridPositionComponent>())
        {
            if (enemyPos.X == playerPos.X && enemyPos.Y == playerPos.Y)
            {
                // Player collided with enemy - take damage
                var newHealth = Math.Max(0, puppy.Health - enemyComponent.AttackDamage);
                var newPuppy = puppy with { Health = newHealth };
                _world.SetComponent(playerEntity.Value, newPuppy);

                enemiesToRemove.Add(enemy);
                Console.WriteLine($"🐕 Ouch! Enemy hit you for {enemyComponent.AttackDamage} damage. Health: {newHealth}");
            }
        }

        // Remove defeated enemies
        foreach (var enemy in enemiesToRemove)
        {
            _world.DestroyEntity(enemy);
        }

        // Check collisions with items
        var itemsToRemove = new List<Entity>();
        foreach (var (item, itemComponent, itemPos) in _world.Query<ItemComponent, GridPositionComponent>())
        {
            if (itemPos.X == playerPos.X && itemPos.Y == playerPos.Y)
            {
                // Player collected item
                ApplyItemEffect(playerEntity.Value, itemComponent, puppy);
                itemsToRemove.Add(item);
            }
        }

        // Remove collected items
        foreach (var item in itemsToRemove)
        {
            _world.DestroyEntity(item);
        }

        // Check collision with exit
        foreach (var (entity, tile, tilePos) in _world.Query<TileComponent, GridPositionComponent>())
        {
            if (tile.Type == TileType.Exit && tilePos.X == playerPos.X && tilePos.Y == playerPos.Y)
            {
                ShouldAdvanceLevel = true;
                Console.WriteLine("🚪 Found the exit! Loading next level...");
            }
        }
    }

    private void ApplyItemEffect(Entity playerEntity, ItemComponent item, PuppyComponent puppy)
    {
        var newPuppy = item.Type switch
        {
            ItemType.Treat => puppy with { Health = Math.Min(100, puppy.Health + item.Value) },
            ItemType.Water => puppy with { Energy = Math.Min(100, puppy.Energy + item.Value) },
            ItemType.Bone => puppy with { SmellRadius = Math.Min(10, puppy.SmellRadius + 1) },
            ItemType.Key => puppy, // Keys could unlock doors in future
            ItemType.Toy => puppy with { Energy = Math.Min(100, puppy.Energy + item.Value) },
            _ => puppy
        };

        _world.SetComponent(playerEntity, newPuppy);

        var message = item.Type switch
        {
            ItemType.Treat => $"🦴 Yum! Health restored to {newPuppy.Health}",
            ItemType.Water => $"💧 Refreshing! Energy restored to {newPuppy.Energy}",
            ItemType.Bone => $"🦴 Special bone! Smell range increased to {newPuppy.SmellRadius}",
            ItemType.Key => "🔑 Found a key! (Future feature)",
            ItemType.Toy => $"🧸 Fun toy! Energy boosted to {newPuppy.Energy}",
            _ => "❓ Found something!"
        };

        Console.WriteLine(message);
    }

    private void CheckWinLoseConditions()
    {
        // Check if player is dead
        foreach (var (_, puppy, _) in _world.Query<PuppyComponent, GridPositionComponent>())
        {
            if (puppy.Health <= 0)
            {
                IsGameLost = true;
                Console.WriteLine("💀 Game Over! The puppy has been defeated.");
                return;
            }
        }

        // Win condition: reach level 5 (for demo purposes)
        // In a full game, this could be more complex
        // For now, just advancing levels is the main goal
    }

    public void ResetLevelProgression()
    {
        ShouldAdvanceLevel = false;
    }

    public void ResetGameState()
    {
        IsGameWon = false;
        IsGameLost = false;
        ShouldAdvanceLevel = false;
    }
}