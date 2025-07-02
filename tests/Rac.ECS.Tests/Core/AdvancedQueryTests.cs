using Rac.ECS.Components;
using Rac.ECS.Core;
using Xunit;

namespace Rac.ECS.Tests.Core;

/// <summary>
/// Tests for the advanced query system with flexible filtering capabilities.
/// Tests both the progressive typing approach (Query().With()) and direct typed approach (QueryBuilder()).
/// </summary>
public class AdvancedQueryTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TEST COMPONENT DEFINITIONS
    // ═══════════════════════════════════════════════════════════════════════════

    private record struct PlayerComponent() : IComponent;
    private record struct EnemyComponent() : IComponent;
    private record struct VelocityComponent(float X, float Y) : IComponent;
    private record struct PositionComponent(float X, float Y) : IComponent;
    private record struct HealthComponent(int Value) : IComponent;
    private record struct WeaponComponent(string Type) : IComponent;
    private record struct DeadComponent() : IComponent;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROGRESSIVE TYPING QUERY TESTS (Query().With() syntax)
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Query_WithSingleComponent_ReturnsCorrectEntities()
    {
        // Arrange
        var world = new World();
        var entity1 = world.CreateEntity();
        var entity2 = world.CreateEntity();
        var entity3 = world.CreateEntity();

        world.SetComponent(entity1, new PlayerComponent());
        world.SetComponent(entity2, new EnemyComponent());
        world.SetComponent(entity3, new PlayerComponent());

        // Act
        var results = world.Query().With<PlayerComponent>().Execute().ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.Entity.Id == entity1.Id);
        Assert.Contains(results, r => r.Entity.Id == entity3.Id);
    }

    [Fact]
    public void Query_WithInclusionFilter_ReturnsEntitiesWithBothComponents()
    {
        // Arrange
        var world = new World();
        var entity1 = world.CreateEntity();
        var entity2 = world.CreateEntity();
        var entity3 = world.CreateEntity();

        // Entity1: Player + Velocity (should match)
        world.SetComponent(entity1, new PlayerComponent());
        world.SetComponent(entity1, new VelocityComponent(1.0f, 2.0f));

        // Entity2: Player only (should not match)
        world.SetComponent(entity2, new PlayerComponent());

        // Entity3: Velocity only (should not match)
        world.SetComponent(entity3, new VelocityComponent(3.0f, 4.0f));

        // Act
        var results = world.Query()
            .With<PlayerComponent>()
            .With<VelocityComponent>()
            .Execute().ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(entity1.Id, results[0].Entity.Id);
    }

    [Fact]
    public void Query_WithExclusionFilter_ExcludesEntitiesWithComponent()
    {
        // Arrange
        var world = new World();
        var entity1 = world.CreateEntity();
        var entity2 = world.CreateEntity();
        var entity3 = world.CreateEntity();

        // Entity1: Player (alive) - should match
        world.SetComponent(entity1, new PlayerComponent());

        // Entity2: Player + Dead - should not match
        world.SetComponent(entity2, new PlayerComponent());
        world.SetComponent(entity2, new DeadComponent());

        // Entity3: Enemy - should not match (doesn't have primary component)
        world.SetComponent(entity3, new EnemyComponent());

        // Act
        var results = world.Query()
            .With<PlayerComponent>()
            .Without<DeadComponent>()
            .Execute().ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(entity1.Id, results[0].Entity.Id);
    }

    [Fact]
    public void Query_ComplexFilter_ReturnsCorrectEntities()
    {
        // Arrange
        var world = new World();
        var player1 = world.CreateEntity(); // Living armed player
        var player2 = world.CreateEntity(); // Living unarmed player
        var player3 = world.CreateEntity(); // Dead armed player
        var enemy1 = world.CreateEntity();  // Living armed enemy

        // Living armed player (should match)
        world.SetComponent(player1, new PlayerComponent());
        world.SetComponent(player1, new HealthComponent(100));
        world.SetComponent(player1, new WeaponComponent("sword"));

        // Living unarmed player (should not match - no weapon)
        world.SetComponent(player2, new PlayerComponent());
        world.SetComponent(player2, new HealthComponent(100));

        // Dead armed player (should not match - has dead component)
        world.SetComponent(player3, new PlayerComponent());
        world.SetComponent(player3, new WeaponComponent("bow"));
        world.SetComponent(player3, new DeadComponent());

        // Living armed enemy (should not match - not a player)
        world.SetComponent(enemy1, new EnemyComponent());
        world.SetComponent(enemy1, new HealthComponent(50));
        world.SetComponent(enemy1, new WeaponComponent("axe"));

        // Act: Find living players with weapons
        var results = world.Query()
            .With<PlayerComponent>()
            .With<HealthComponent>()
            .With<WeaponComponent>()
            .Without<DeadComponent>()
            .Execute().ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(player1.Id, results[0].Entity.Id);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TYPED QUERY BUILDER TESTS (QueryBuilder<T>() syntax)
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void QueryBuilder_WithFilters_ReturnsCorrectEntities()
    {
        // Arrange
        var world = new World();
        var entity1 = world.CreateEntity();
        var entity2 = world.CreateEntity();

        world.SetComponent(entity1, new PlayerComponent());
        world.SetComponent(entity1, new VelocityComponent(1.0f, 2.0f));

        world.SetComponent(entity2, new PlayerComponent());
        world.SetComponent(entity2, new DeadComponent());

        // Act
        var results = world.QueryBuilder<PlayerComponent>()
            .With<VelocityComponent>()
            .Without<DeadComponent>()
            .Execute().ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(entity1.Id, results[0].Entity.Id);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void HasComponent_WithExistingComponent_ReturnsTrue()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        world.SetComponent(entity, new PlayerComponent());

        // Act & Assert
        Assert.True(world.HasComponent<PlayerComponent>(entity));
        Assert.False(world.HasComponent<EnemyComponent>(entity));
    }

    [Fact]
    public void TryGetComponent_WithExistingComponent_ReturnsTrue()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        var originalComponent = new VelocityComponent(5.0f, 10.0f);
        world.SetComponent(entity, originalComponent);

        // Act
        bool found = world.TryGetComponent<VelocityComponent>(entity, out var component);

        // Assert
        Assert.True(found);
        Assert.Equal(originalComponent, component);
    }

    [Fact]
    public void TryGetComponent_WithNonExistingComponent_ReturnsFalse()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act
        bool found = world.TryGetComponent<VelocityComponent>(entity, out var component);

        // Assert
        Assert.False(found);
        Assert.Equal(default(VelocityComponent), component);
    }

    [Fact]
    public void TryGetComponents_TwoComponents_WithExistingComponents_ReturnsTrue()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        var player = new PlayerComponent();
        var velocity = new VelocityComponent(1.0f, 2.0f);

        world.SetComponent(entity, player);
        world.SetComponent(entity, velocity);

        // Act
        bool found = world.TryGetComponents<PlayerComponent, VelocityComponent>(
            entity, out var retrievedPlayer, out var retrievedVelocity);

        // Assert
        Assert.True(found);
        Assert.Equal(player, retrievedPlayer);
        Assert.Equal(velocity, retrievedVelocity);
    }

    [Fact]
    public void TryGetComponents_TwoComponents_WithMissingComponent_ReturnsFalse()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        world.SetComponent(entity, new PlayerComponent());
        // Missing VelocityComponent

        // Act
        bool found = world.TryGetComponents<PlayerComponent, VelocityComponent>(
            entity, out var player, out var velocity);

        // Assert
        Assert.False(found);
    }

    [Fact]
    public void TryGetComponents_ThreeComponents_WithAllComponents_ReturnsTrue()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        var player = new PlayerComponent();
        var position = new PositionComponent(10.0f, 20.0f);
        var velocity = new VelocityComponent(1.0f, 2.0f);

        world.SetComponent(entity, player);
        world.SetComponent(entity, position);
        world.SetComponent(entity, velocity);

        // Act
        bool found = world.TryGetComponents<PlayerComponent, PositionComponent, VelocityComponent>(
            entity, out var retrievedPlayer, out var retrievedPosition, out var retrievedVelocity);

        // Assert
        Assert.True(found);
        Assert.Equal(player, retrievedPlayer);
        Assert.Equal(position, retrievedPosition);
        Assert.Equal(velocity, retrievedVelocity);
    }

    [Fact]
    public void TryGetComponents_FourComponents_WithAllComponents_ReturnsTrue()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();
        var player = new PlayerComponent();
        var position = new PositionComponent(10.0f, 20.0f);
        var velocity = new VelocityComponent(1.0f, 2.0f);
        var health = new HealthComponent(100);

        world.SetComponent(entity, player);
        world.SetComponent(entity, position);
        world.SetComponent(entity, velocity);
        world.SetComponent(entity, health);

        // Act
        bool found = world.TryGetComponents<PlayerComponent, PositionComponent, VelocityComponent, HealthComponent>(
            entity, out var retrievedPlayer, out var retrievedPosition, out var retrievedVelocity, out var retrievedHealth);

        // Assert
        Assert.True(found);
        Assert.Equal(player, retrievedPlayer);
        Assert.Equal(position, retrievedPosition);
        Assert.Equal(velocity, retrievedVelocity);
        Assert.Equal(health, retrievedHealth);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EXTENDED QUERY TESTS (4+ components)
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Query_FourComponents_ReturnsCorrectEntities()
    {
        // Arrange
        var world = new World();
        var entity1 = world.CreateEntity();
        var entity2 = world.CreateEntity();

        // Entity1 has all four components
        world.SetComponent(entity1, new PlayerComponent());
        world.SetComponent(entity1, new PositionComponent(1.0f, 2.0f));
        world.SetComponent(entity1, new VelocityComponent(3.0f, 4.0f));
        world.SetComponent(entity1, new HealthComponent(100));

        // Entity2 has only three components
        world.SetComponent(entity2, new PlayerComponent());
        world.SetComponent(entity2, new PositionComponent(5.0f, 6.0f));
        world.SetComponent(entity2, new VelocityComponent(7.0f, 8.0f));

        // Act
        var results = world.Query<PlayerComponent, PositionComponent, VelocityComponent, HealthComponent>().ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(entity1.Id, results[0].Entity.Id);
        Assert.Equal(100, results[0].Component4.Value);
    }

    [Fact]
    public void Query_FiveComponents_ReturnsCorrectEntities()
    {
        // Arrange
        var world = new World();
        var entity1 = world.CreateEntity();
        var entity2 = world.CreateEntity();

        // Entity1 has all five components
        world.SetComponent(entity1, new PlayerComponent());
        world.SetComponent(entity1, new PositionComponent(1.0f, 2.0f));
        world.SetComponent(entity1, new VelocityComponent(3.0f, 4.0f));
        world.SetComponent(entity1, new HealthComponent(100));
        world.SetComponent(entity1, new WeaponComponent("sword"));

        // Entity2 has only four components
        world.SetComponent(entity2, new PlayerComponent());
        world.SetComponent(entity2, new PositionComponent(5.0f, 6.0f));
        world.SetComponent(entity2, new VelocityComponent(7.0f, 8.0f));
        world.SetComponent(entity2, new HealthComponent(50));

        // Act
        var results = world.Query<PlayerComponent, PositionComponent, VelocityComponent, HealthComponent, WeaponComponent>().ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(entity1.Id, results[0].Entity.Id);
        Assert.Equal("sword", results[0].Component5.Type);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NULL WORLD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void NullWorld_Query_ReturnsEmptyResults()
    {
        // Arrange
        var nullWorld = new NullWorld();

        // Act
        var results = nullWorld.Query().With<PlayerComponent>().Execute().ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void NullWorld_QueryBuilder_ReturnsEmptyResults()
    {
        // Arrange
        var nullWorld = new NullWorld();

        // Act
        var results = nullWorld.QueryBuilder<PlayerComponent>()
            .With<VelocityComponent>()
            .Without<DeadComponent>()
            .Execute().ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void NullWorld_HasComponent_ReturnsFalse()
    {
        // Arrange
        var nullWorld = new NullWorld();
        var entity = new Entity(1);

        // Act & Assert
        Assert.False(nullWorld.HasComponent<PlayerComponent>(entity));
    }

    [Fact]
    public void NullWorld_TryGetComponents_ReturnsFalse()
    {
        // Arrange
        var nullWorld = new NullWorld();
        var entity = new Entity(1);

        // Act
        bool found = nullWorld.TryGetComponents<PlayerComponent, VelocityComponent>(
            entity, out var player, out var velocity);

        // Assert
        Assert.False(found);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PERFORMANCE CHARACTERISTICS TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Query_WithLargeDataset_MaintainsPerformance()
    {
        // Arrange
        var world = new World();
        const int entityCount = 1000;
        var players = new List<Entity>();

        // Create 1000 entities, half are players
        for (int i = 0; i < entityCount; i++)
        {
            var entity = world.CreateEntity();
            if (i % 2 == 0)
            {
                world.SetComponent(entity, new PlayerComponent());
                players.Add(entity);
                
                // Only some players have weapons (every 4th entity overall)
                if (i % 4 == 0)
                {
                    world.SetComponent(entity, new WeaponComponent("weapon"));
                }
            }
            else
            {
                world.SetComponent(entity, new EnemyComponent());
            }
        }

        // Act
        var start = DateTime.UtcNow;
        var results = world.Query()
            .With<PlayerComponent>()
            .With<WeaponComponent>()
            .Execute().ToList();
        var elapsed = DateTime.UtcNow - start;

        // Assert - Every 4th entity has weapons, but only half are players, so (entityCount / 4) = 250
        // Wait, that's not right. Let me recalculate:
        // i=0: Player + Weapon
        // i=2: Player (no weapon)
        // i=4: Player + Weapon  
        // i=6: Player (no weapon)
        // So pattern is: 0,4,8,12... have PlayerComponent + WeaponComponent = entityCount/4 = 250
        Assert.Equal(250, results.Count); // Every 4th entity starting from 0
        Assert.True(elapsed.TotalMilliseconds < 100, $"Query took {elapsed.TotalMilliseconds}ms, expected < 100ms");
    }
}