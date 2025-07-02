// ═══════════════════════════════════════════════════════════════════════════════
// ADVANCED QUERY SAMPLE - EDUCATIONAL ECS QUERY SYSTEM DEMONSTRATION
// ═══════════════════════════════════════════════════════════════════════════════
//
// This sample demonstrates the new advanced query system features in RACEngine ECS,
// showcasing flexible filtering capabilities and extended query functionality.
//
// EDUCATIONAL OBJECTIVES:
//
// 1. ADVANCED QUERY PATTERNS:
//    - Progressive Type Specification: world.Query().With<T>()
//    - Inclusion/Exclusion Filters: .With<T>().Without<U>()
//    - Extended Component Queries: 4+ component combinations
//    - Multi-Component Helpers: TryGetComponents<T1,T2>()
//
// 2. ENTITY FILTERING STRATEGIES:
//    - Complex entity categorization using component combinations
//    - Performance-optimized query execution
//    - Real-world game entity management scenarios
//
// 3. FLUENT INTERFACE DESIGN:
//    - Method chaining for readable query construction
//    - Builder pattern for complex query building
//    - Type safety through progressive interface narrowing
//
// SAMPLE SCENARIO:
// A simple RPG-style game world with different entity types:
// - Players: Have Position, Health, Experience components
// - Enemies: Have Position, Health, AI components  
// - NPCs: Have Position, Health, Dialogue components
// - Dead entities: Have DeadComponent
// - Armed entities: Have WeaponComponent
//
// QUERY DEMONSTRATIONS:
// 1. Find all living players: world.Query().With<Player>().Without<Dead>()
// 2. Find armed enemies: world.Query().With<Enemy>().With<Weapon>().Without<Dead>()
// 3. Find all NPCs that can be interacted with
// 4. Complex 5-component queries for advanced entity processing
//
// ═══════════════════════════════════════════════════════════════════════════════

using Rac.ECS.Components;
using Rac.ECS.Core;

namespace AdvancedQuerySample;

// ═══════════════════════════════════════════════════════════════════════════════
// COMPONENT DEFINITIONS
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Marks an entity as a player character.
/// Educational Note: Marker components (no data) are useful for categorization.
/// </summary>
public readonly record struct PlayerComponent() : IComponent;

/// <summary>
/// Marks an entity as an enemy character.
/// </summary>
public readonly record struct EnemyComponent() : IComponent;

/// <summary>
/// Marks an entity as a non-player character.
/// </summary>
public readonly record struct NPCComponent() : IComponent;

/// <summary>
/// Marks an entity as dead (should be excluded from most queries).
/// </summary>
public readonly record struct DeadComponent() : IComponent;

/// <summary>
/// Represents the position of an entity in 2D space.
/// </summary>
public readonly record struct PositionComponent(float X, float Y) : IComponent;

/// <summary>
/// Represents the health of an entity.
/// </summary>
public readonly record struct HealthComponent(int Current, int Maximum) : IComponent;

/// <summary>
/// Represents a weapon carried by an entity.
/// </summary>
public readonly record struct WeaponComponent(string Name, int Damage) : IComponent;

/// <summary>
/// Represents player experience and level.
/// </summary>
public readonly record struct ExperienceComponent(int Level, int XP) : IComponent;

/// <summary>
/// Marks an entity as having AI behavior.
/// </summary>
public readonly record struct AIComponent() : IComponent;

/// <summary>
/// Represents dialogue capabilities for NPCs.
/// </summary>
public readonly record struct DialogueComponent(string[] Lines) : IComponent;

class Program
{
    static void Main()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              ADVANCED QUERY SYSTEM DEMONSTRATION                            ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════════════════
        // WORLD SETUP
        // ═══════════════════════════════════════════════════════════════════════════

        var world = new World();
        SetupGameWorld(world);

        // ═══════════════════════════════════════════════════════════════════════════
        // DEMONSTRATION 1: PROGRESSIVE TYPE SPECIFICATION
        // ═══════════════════════════════════════════════════════════════════════════

        Console.WriteLine("🔍 DEMONSTRATION 1: Progressive Type Specification");
        Console.WriteLine("   Query syntax: world.Query().With<Component>()");
        Console.WriteLine();

        var livingPlayers = world.Query()
            .With<PlayerComponent>()
            .Without<DeadComponent>()
            .Execute();

        Console.WriteLine("   Living Players:");
        foreach (var (entity, _) in livingPlayers)
        {
            if (world.TryGetComponents<PositionComponent, HealthComponent>(entity, out var pos, out var health))
            {
                Console.WriteLine($"   • Player {entity.Id}: Position({pos.X:F1}, {pos.Y:F1}), Health({health.Current}/{health.Maximum})");
            }
        }
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════════════════
        // DEMONSTRATION 2: COMPLEX FILTERING
        // ═══════════════════════════════════════════════════════════════════════════

        Console.WriteLine("🔍 DEMONSTRATION 2: Complex Filtering");
        Console.WriteLine("   Find armed enemies that are still alive");
        Console.WriteLine();

        var armedEnemies = world.Query()
            .With<EnemyComponent>()
            .With<WeaponComponent>()
            .Without<DeadComponent>()
            .Execute();

        Console.WriteLine("   Armed Living Enemies:");
        foreach (var (entity, _) in armedEnemies)
        {
            if (world.TryGetComponents<PositionComponent, HealthComponent, WeaponComponent>(entity, out var pos, out var health, out var weapon))
            {
                Console.WriteLine($"   • Enemy {entity.Id}: {weapon.Name} (Damage: {weapon.Damage}), Health({health.Current}/{health.Maximum})");
            }
        }
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════════════════
        // DEMONSTRATION 3: EXTENDED QUERY SUPPORT (4+ COMPONENTS)
        // ═══════════════════════════════════════════════════════════════════════════

        Console.WriteLine("🔍 DEMONSTRATION 3: Extended Query Support (4+ Components)");
        Console.WriteLine("   Using Query<T1, T2, T3, T4, T5>() for complex entity processing");
        Console.WriteLine();

        var complexEntities = world.Query<PositionComponent, HealthComponent, WeaponComponent, ExperienceComponent, PlayerComponent>();

        Console.WriteLine("   Armed Player Characters (5-Component Query):");
        foreach (var (entity, pos, health, weapon, xp, _) in complexEntities)
        {
            Console.WriteLine($"   • Player {entity.Id}: Level {xp.Level} with {weapon.Name}");
            Console.WriteLine($"     Position: ({pos.X:F1}, {pos.Y:F1}), Health: {health.Current}/{health.Maximum}, XP: {xp.XP}");
        }
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════════════════
        // DEMONSTRATION 4: MULTI-COMPONENT HELPERS
        // ═══════════════════════════════════════════════════════════════════════════

        Console.WriteLine("🔍 DEMONSTRATION 4: Multi-Component Helpers");
        Console.WriteLine("   Using TryGetComponents<T1, T2, T3>() for efficient batch retrieval");
        Console.WriteLine();

        var allEntities = world.GetAllEntities();
        Console.WriteLine("   Entity Component Analysis:");
        
        foreach (var entity in allEntities)
        {
            var components = new List<string>();
            
            // Check for various component combinations
            if (world.HasComponent<PlayerComponent>(entity)) components.Add("Player");
            if (world.HasComponent<EnemyComponent>(entity)) components.Add("Enemy");
            if (world.HasComponent<NPCComponent>(entity)) components.Add("NPC");
            if (world.HasComponent<WeaponComponent>(entity)) components.Add("Armed");
            if (world.HasComponent<DeadComponent>(entity)) components.Add("Dead");
            if (world.HasComponent<AIComponent>(entity)) components.Add("AI");
            if (world.HasComponent<DialogueComponent>(entity)) components.Add("Dialogue");

            Console.WriteLine($"   • Entity {entity.Id}: [{string.Join(", ", components)}]");
        }
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════════════════
        // DEMONSTRATION 5: PERFORMANCE COMPARISON
        // ═══════════════════════════════════════════════════════════════════════════

        Console.WriteLine("🔍 DEMONSTRATION 5: Performance Characteristics");
        Console.WriteLine("   Comparing query performance with larger datasets");
        Console.WriteLine();

        PerformanceDemo(world);

        Console.WriteLine("═══════════════════════════════════════════════════════════════════════════════");
        Console.WriteLine("Advanced Query System Demonstration Complete!");
        Console.WriteLine("Key Features Demonstrated:");
        Console.WriteLine("✓ Progressive type specification (Query().With<T>())");
        Console.WriteLine("✓ Complex inclusion/exclusion filtering");
        Console.WriteLine("✓ Extended 4+ component queries");
        Console.WriteLine("✓ Multi-component helper methods");
        Console.WriteLine("✓ Performance optimization with large datasets");
        Console.WriteLine("═══════════════════════════════════════════════════════════════════════════════");
    }

    private static void SetupGameWorld(World world)
    {
        // Create Players
        var player1 = world.CreateEntity("Hero");
        world.SetComponent(player1, new PlayerComponent());
        world.SetComponent(player1, new PositionComponent(10.5f, 20.3f));
        world.SetComponent(player1, new HealthComponent(100, 100));
        world.SetComponent(player1, new WeaponComponent("Excalibur", 50));
        world.SetComponent(player1, new ExperienceComponent(5, 1250));

        var player2 = world.CreateEntity("Mage");
        world.SetComponent(player2, new PlayerComponent());
        world.SetComponent(player2, new PositionComponent(8.2f, 15.7f));
        world.SetComponent(player2, new HealthComponent(75, 80));
        world.SetComponent(player2, new WeaponComponent("Magic Staff", 35));
        world.SetComponent(player2, new ExperienceComponent(3, 750));

        // Create Enemies
        var enemy1 = world.CreateEntity("Orc");
        world.SetComponent(enemy1, new EnemyComponent());
        world.SetComponent(enemy1, new PositionComponent(25.0f, 30.0f));
        world.SetComponent(enemy1, new HealthComponent(60, 60));
        world.SetComponent(enemy1, new WeaponComponent("Battle Axe", 40));
        world.SetComponent(enemy1, new AIComponent());

        var enemy2 = world.CreateEntity("Goblin");
        world.SetComponent(enemy2, new EnemyComponent());
        world.SetComponent(enemy2, new PositionComponent(18.5f, 22.1f));
        world.SetComponent(enemy2, new HealthComponent(30, 30));
        world.SetComponent(enemy2, new AIComponent());

        // Create Dead Enemy
        var deadEnemy = world.CreateEntity("Dead Troll");
        world.SetComponent(deadEnemy, new EnemyComponent());
        world.SetComponent(deadEnemy, new PositionComponent(5.0f, 5.0f));
        world.SetComponent(deadEnemy, new HealthComponent(0, 120));
        world.SetComponent(deadEnemy, new WeaponComponent("Troll Club", 60));
        world.SetComponent(deadEnemy, new DeadComponent());

        // Create NPCs
        var merchant = world.CreateEntity("Merchant");
        world.SetComponent(merchant, new NPCComponent());
        world.SetComponent(merchant, new PositionComponent(15.0f, 10.0f));
        world.SetComponent(merchant, new HealthComponent(50, 50));
        world.SetComponent(merchant, new DialogueComponent(new[] { "Welcome to my shop!", "What can I get you today?" }));

        var guard = world.CreateEntity("City Guard");
        world.SetComponent(guard, new NPCComponent());
        world.SetComponent(guard, new PositionComponent(12.0f, 8.0f));
        world.SetComponent(guard, new HealthComponent(80, 80));
        world.SetComponent(guard, new WeaponComponent("Guard Sword", 30));
        world.SetComponent(guard, new DialogueComponent(new[] { "Halt! State your business.", "The city is safe under our watch." }));
    }

    private static void PerformanceDemo(World world)
    {
        // Create a larger dataset for performance testing
        var random = new Random();
        var createdEntities = new List<Entity>();

        Console.WriteLine("   Creating 1000 additional entities for performance testing...");
        
        var start = DateTime.UtcNow;
        
        for (int i = 0; i < 1000; i++)
        {
            var entity = world.CreateEntity($"TestEntity{i}");
            createdEntities.Add(entity);
            
            // Randomly assign components
            if (random.Next(100) < 30) // 30% are players
            {
                world.SetComponent(entity, new PlayerComponent());
                world.SetComponent(entity, new ExperienceComponent(random.Next(1, 10), random.Next(100, 2000)));
            }
            else if (random.Next(100) < 50) // 50% are enemies
            {
                world.SetComponent(entity, new EnemyComponent());
                world.SetComponent(entity, new AIComponent());
            }
            else // 20% are NPCs
            {
                world.SetComponent(entity, new NPCComponent());
            }

            // All entities have position and health
            world.SetComponent(entity, new PositionComponent(random.Next(-100, 100), random.Next(-100, 100)));
            world.SetComponent(entity, new HealthComponent(random.Next(20, 100), 100));

            // Some have weapons
            if (random.Next(100) < 40)
            {
                world.SetComponent(entity, new WeaponComponent($"Weapon{i}", random.Next(10, 50)));
            }

            // Some are dead
            if (random.Next(100) < 10)
            {
                world.SetComponent(entity, new DeadComponent());
            }
        }

        var setupTime = DateTime.UtcNow - start;

        // Test complex query performance
        start = DateTime.UtcNow;
        var livingArmedPlayers = world.Query()
            .With<PlayerComponent>()
            .With<WeaponComponent>()
            .Without<DeadComponent>()
            .Execute()
            .ToList();
        var queryTime = DateTime.UtcNow - start;

        Console.WriteLine($"   Setup time: {setupTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"   Complex query time: {queryTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"   Results found: {livingArmedPlayers.Count} living armed players");
        Console.WriteLine($"   Total entities in world: {world.GetAllEntities().Count()}");

        // Clean up test entities to avoid affecting other demonstrations
        foreach (var entity in createdEntities)
        {
            world.DestroyEntity(entity);
        }
    }
}