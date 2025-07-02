# Advanced Query Sample

This sample demonstrates the new advanced query system features in RACEngine ECS, showcasing flexible filtering capabilities and extended query functionality.

## Features Demonstrated

### 1. Progressive Type Specification
```csharp
// Start untyped, establish primary component with first With<T>()
var livingPlayers = world.Query()
    .With<PlayerComponent>()
    .Without<DeadComponent>()
    .Execute();
```

### 2. Complex Filtering
```csharp
// Multiple inclusion and exclusion filters
var armedEnemies = world.Query()
    .With<EnemyComponent>()
    .With<WeaponComponent>()
    .Without<DeadComponent>()
    .Execute();
```

### 3. Extended Query Support (4+ Components)
```csharp
// Support for 5-component queries and beyond
var complexEntities = world.Query<PositionComponent, HealthComponent, WeaponComponent, ExperienceComponent, PlayerComponent>();
```

### 4. Multi-Component Helpers
```csharp
// Efficient batch component retrieval
if (world.TryGetComponents<Position, Velocity>(entity, out var pos, out var vel))
{
    // Process entity with both components
}
```

### 5. Performance Optimization
- Maintains O(n) complexity using smallest pool iteration
- Early exit optimizations for filter evaluation
- Lazy evaluation with yield return for memory efficiency
- Supports large datasets (1000+ entities) with sub-millisecond query times

## Sample Scenario

The sample creates a simple RPG-style game world with different entity types:
- **Players**: Have Position, Health, Experience components
- **Enemies**: Have Position, Health, AI components  
- **NPCs**: Have Position, Health, Dialogue components
- **Dead entities**: Have DeadComponent (excluded from most queries)
- **Armed entities**: Have WeaponComponent

## Running the Sample

```bash
cd samples/AdvancedQuerySample
dotnet run
```

## Educational Value

This sample demonstrates:
- **Entity Composition**: How different component combinations create entity types
- **Query Optimization**: Performance characteristics with large datasets
- **Fluent Interface Design**: Method chaining for readable query construction
- **Type Safety**: Progressive interface narrowing for compile-time safety
- **Real-world Patterns**: Common game development query scenarios

## Key API Patterns

| Pattern | Description | Example |
|---------|-------------|---------|
| Progressive Typing | `Query().With<T>()` | `world.Query().With<Player>().Without<Dead>()` |
| Direct Typing | `QueryBuilder<T>()` | `world.QueryBuilder<Enemy>().With<Weapon>()` |
| Extended Queries | `Query<T1,T2,T3,T4,T5>()` | `world.Query<Pos,Vel,Health,Weapon,AI>()` |
| Helper Methods | `TryGetComponents<T1,T2>()` | `world.TryGetComponents<Pos,Vel>(entity, out p, out v)` |
| Component Checking | `HasComponent<T>()` | `world.HasComponent<Player>(entity)` |

This sample serves as both a demonstration and a reference for implementing advanced entity queries in your own RACEngine projects.