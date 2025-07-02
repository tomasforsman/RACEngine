# Rac.ECS Project Documentation

## Project Overview

The `Rac.ECS` project implements a complete Entity-Component-System architecture that serves as the foundational data management and game object system for the Rac game engine. This implementation prioritizes data-oriented design, composition over inheritance, and high-performance component queries to support modern game development workflows.

### Key Design Principles

- **Data-Oriented Design**: Components store only data, systems contain only logic
- **Composition Over Inheritance**: Game objects are assembled from components rather than inherited from base classes
- **Performance-First**: Optimized component storage and query systems for frame-rate critical operations
- **Immutable Components**: Components are readonly record structs for thread safety and predictable behavior
- **Sparse Data Storage**: Only entities with specific components consume memory for those components

## Architecture Overview

The ECS implementation follows a three-pillar architecture where entities serve as lightweight identifiers, components represent pure data containers, and systems provide all behavioral logic. The `World` class acts as the central database, managing component storage and providing efficient query mechanisms for systems to operate on relevant entity sets.

### Core Architectural Decisions

- **Dictionary-based Component Storage**: Two-tier dictionary system (Type → EntityId → Component) enables O(1) component access
- **Query-driven System Design**: Systems discover relevant entities through type-safe component queries
- **Hierarchical Transform Support**: Built-in support for parent-child relationships with automatic world transform computation
- **Extension Method Pattern**: Core entity operations exposed through extension methods to maintain clean separation of concerns

## Namespace Organization

### Rac.ECS.Core

Contains the fundamental building blocks of the ECS architecture and primary interaction points for game code.

**Entity**: Lightweight identifier struct that represents game objects. Contains only an ID and alive status, following ECS principles where entities are pure identifiers without behavior or data.

**World**: Central component database and query engine. Manages all component storage, entity creation, and provides both traditional and advanced query interfaces that systems use to discover relevant entities. Implements optimized multi-component queries with performance considerations for varying component pool sizes. Supports up to 5-component queries and includes advanced filtering capabilities through the progressive type specification API.

**EntityFluentExtensions**: Modern fluent interface API for entity component assignment. Implements Martin Fowler's "Fluent Interface" pattern using C# extension methods, providing chainable component assignment operations. This is the **recommended approach** for entity creation, offering superior readability, IDE discoverability, and reduced error potential compared to traditional verbose component assignment patterns.

**EntityHierarchyExtensions**: Convenience layer that provides intuitive hierarchy management operations. Encapsulates complex parent-child relationship logic while maintaining the underlying ECS data model. Enables natural game object manipulation patterns while preserving ECS architectural integrity.

**IQueryRoot**: Interface for building advanced queries without initially specifying a primary component type. Enables the progressive type specification syntax `world.Query().With<ComponentType>()` where the first `With<T>()` call establishes the primary component type for the query. Demonstrates progressive interface narrowing design patterns.

**IQueryBuilder&lt;T&gt;**: Interface for building advanced queries with inclusion and exclusion filters. Provides fluent query syntax for complex entity filtering with method chaining capabilities. Follows Martin Fowler's "Fluent Interface" pattern for improved readability and maintainability.

**QueryRoot**: Concrete implementation of IQueryRoot that serves as the entry point for progressive type specification queries. Converts untyped query requests into typed QueryBuilder instances when the first component type is specified.

**QueryBuilder&lt;T&gt;**: High-performance implementation of IQueryBuilder that provides advanced filtering capabilities with inclusion (`With<T>()`) and exclusion (`Without<T>()`) criteria. Implements sophisticated optimization strategies including smallest pool iteration and early exit logic.

### Rac.ECS.Components

Houses all component definitions and the base component interface. Components in this namespace represent the data vocabulary of the game engine.

**IComponent**: Marker interface that defines the contract for all component types. Establishes the pure data principle where components contain no methods or behavior.

**TransformComponent**: Stores local transformation data (position, rotation, scale) relative to an entity's parent in the hierarchy. Provides the foundation for all spatial relationships in the game world.

**WorldTransformComponent**: Contains computed world-space transformation data. Generated automatically by the TransformSystem and represents the final transformation used by rendering, physics, and other world-space systems.

**ParentHierarchyComponent**: Manages parent-child relationships between entities. Stores parent references and child collections to enable scene graph traversal and hierarchical transform inheritance.

### Rac.ECS.Systems

Contains the behavioral logic layer of the ECS architecture and system management infrastructure.

**ISystem**: Defines the contract for all system implementations. Establishes the complete system lifecycle with Initialize(IWorld), Update(float), and Shutdown(IWorld) methods. All three methods are required implementations to ensure proper resource management and deterministic system behavior throughout the application lifecycle.

**RunAfterAttribute**: Declarative dependency management attribute that allows systems to specify execution order dependencies. Supports multiple dependencies per system and includes validation to prevent circular references. Uses standard .NET attribute inheritance patterns for modular system design.

**SystemScheduler**: Manages system registration, execution order, lifecycle, and batch updates. Provides automatic dependency resolution using topological sorting algorithms to ensure systems execute in correct order. Includes circular dependency detection and supports both manual and automatic execution ordering. Handles system initialization during registration and shutdown during removal.

**SystemDependencyResolver**: Internal utility class implementing Kahn's topological sorting algorithm for dependency resolution. Provides O(V + E) performance for dependency graphs and includes comprehensive circular dependency detection with clear error reporting.

**TransformSystem**: Specialized system that computes world transforms from local transforms and hierarchy relationships. Implements depth-first traversal algorithms to efficiently propagate transformations through the scene graph. Demonstrates advanced system implementation patterns with Initialize() setup and complex update logic.

### Rac.ECS.Entities

Reserved namespace for future entity-related functionality. Currently contains placeholder interfaces for potential entity behavior extensions.

## Core Concepts and Workflows

### Component Lifecycle

Components follow an immutable data model where modifications create new component instances rather than mutating existing ones. This approach ensures predictable behavior, enables efficient caching strategies, and supports potential multithreading scenarios. Components are added to entities through the World's SetComponent method and queried through type-safe query operations.

### Hierarchy Management

The hierarchy system operates on two-way relationships where parent entities maintain child collections and child entities store parent references. This design enables efficient upward and downward traversal while supporting complex scene graph operations. Transform inheritance flows from parent to child through matrix composition, following standard graphics pipeline conventions.

### System Execution Model

Systems operate on entity sets discovered through component queries, processing relevant entities each frame. The SystemScheduler manages the complete system lifecycle with automatic dependency resolution and proper execution ordering.

**System Lifecycle Management:**
Systems follow a three-phase lifecycle managed by the SystemScheduler:

1. **Initialize Phase**: Called once when system is added to scheduler. Systems receive IWorld parameter for setup operations like creating configuration entities, caching queries, or registering event handlers.

2. **Update Phase**: Called every frame with delta time for frame-rate independent logic. Systems query entities, process component data, and update world state based on game logic.

3. **Shutdown Phase**: Called once when system is removed or scheduler is cleared. Systems receive IWorld parameter for cleanup operations like disposing resources, removing configuration entities, or unregistering event handlers.

**Dependency Resolution:**
The SystemScheduler uses declarative dependency management through RunAfter attributes:

```csharp
// Dependency chain: Input → Movement → Physics → Rendering
public class InputSystem : ISystem { /* ... */ }

[RunAfter(typeof(InputSystem))]
public class MovementSystem : ISystem { /* ... */ }

[RunAfter(typeof(MovementSystem))]  
public class PhysicsSystem : ISystem { /* ... */ }

[RunAfter(typeof(PhysicsSystem))]
public class RenderingSystem : ISystem { /* ... */ }
```

The scheduler automatically resolves execution order using topological sorting, ensuring systems run in dependency order regardless of registration order. Circular dependencies are detected and reported with clear error messages.

**Required System Implementation:**
All systems must implement the complete ISystem interface including Initialize(), Update(), and Shutdown() methods. The SystemScheduler requires an IWorld instance during construction to ensure proper system lifecycle management and resource cleanup.

### Query Performance Characteristics

Multi-component queries optimize performance by iterating the smallest component pool first, reducing unnecessary entity checks. The query system supports single, dual, triple, quadruple, and quintuple component combinations with automatic performance optimization. Query results are lazy-evaluated and can be safely enumerated multiple times within a frame.

### Advanced Query System

The ECS includes a comprehensive advanced query system that supports flexible filtering through inclusion and exclusion criteria. This system enables complex entity discovery patterns while maintaining optimal performance characteristics.

#### Progressive Type Specification API

The advanced query system supports progressive type specification, allowing developers to build queries dynamically without initially specifying all component types:

```csharp
// Progressive type specification - starts untyped, becomes typed through With<T>()
var enemies = world.Query()
    .With<VelocityComponent>()
    .Without<PlayerComponent>()
    .Execute();

// Complex filtering with multiple inclusion and exclusion criteria
var armedLivingEnemies = world.Query()
    .With<HealthComponent>()
    .With<WeaponComponent>()
    .With<AIComponent>()
    .Without<PlayerComponent>()
    .Without<DeadComponent>()
    .Execute();
```

#### Extended Multi-Component Query Support

The query system supports complex entity combinations beyond the traditional 3-component limit:

```csharp
// 4-component queries for complex entity types
foreach (var (entity, pos, vel, health, weapon) in 
    world.Query<PositionComponent, VelocityComponent, HealthComponent, WeaponComponent>())
{
    // Process entities with all four components
}

// 5-component queries for highly specialized entities
foreach (var (entity, transform, sprite, ai, stats, inventory) in 
    world.Query<TransformComponent, SpriteComponent, AIComponent, StatsComponent, InventoryComponent>())
{
    // Process complex game entities with five components
}
```

#### Multi-Component Helper Methods

Efficient batch component retrieval methods reduce the overhead of multiple individual component access operations:

```csharp
// Retrieve two components in a single operation
if (world.TryGetComponents<PositionComponent, VelocityComponent>(entity, out var pos, out var vel))
{
    // Process entity with both components - more efficient than two separate calls
}

// Support for 3 and 4 component batch retrieval
world.TryGetComponents<T1, T2, T3>(entity, out var c1, out var c2, out var c3);
world.TryGetComponents<T1, T2, T3, T4>(entity, out var c1, out var c2, out var c3, out var c4);
```

#### Advanced Query Architecture

The advanced query system follows a builder pattern architecture with clear separation of concerns:

- **IQueryRoot**: Untyped query starting point enabling progressive type specification
- **IQueryBuilder&lt;T&gt;**: Typed query builder providing fluent filtering interface
- **QueryRoot**: Concrete implementation of the untyped query entry point
- **QueryBuilder&lt;T&gt;**: High-performance query builder with optimization strategies
- **Null Object Pattern**: Complete null object implementations for testing scenarios

#### Performance Optimization Strategies

The advanced query system implements several performance optimization techniques:

- **Smallest Pool Iteration**: Queries iterate the component pool with the fewest entities to minimize unnecessary checks
- **Early Exit Logic**: Filtering stops as soon as any exclusion condition is met
- **Lazy Evaluation**: Query results use `yield return` for memory-efficient iteration
- **Batch Component Access**: Multi-component helpers reduce World access overhead

## Integration Points

### Engine Dependencies

- **Silk.NET.Maths**: Provides vector and matrix mathematics for spatial calculations
- **Rac.Core**: Foundation types and utilities (referenced but not detailed in provided files)

### Rendering System Integration

The ECS provides WorldTransformComponent data that rendering systems consume for vertex transformation and culling operations. The transform hierarchy ensures that rendering reflects the logical scene structure without requiring additional spatial calculations in rendering code.

### Physics System Integration

Physics systems operate on world transform data for collision detection and response calculations. The ECS component model allows physics properties to be attached to specific entities without affecting the transform system or other game logic.

### Input System Integration

Input systems can query for entities with input-related components and update their state based on user interaction. The hierarchy system supports interaction models where input on parent entities can affect child entities or vice versa.

## Usage Patterns

### Modern Entity Creation (Recommended)

The fluent API is the recommended approach for entity creation, providing clean, readable, and chainable component assignment:

```csharp
// ✅ RECOMMENDED: Fluent API approach
var player = world.CreateEntity()
    .WithName(world, "Player")
    .WithPosition(world, new Vector2D<float>(100, 200))
    .WithTags(world, "Player", "Controllable")
    .WithComponent(world, new HealthComponent(100))
    .WithComponent(world, new VelocityComponent(Vector2D<float>.Zero));

// Complex entities with transforms
var boss = world.CreateEntity()
    .WithName(world, "BossEnemy")
    .WithTransform(world, x: 400, y: 300, rotation: 0f, scaleX: 2f, scaleY: 2f)
    .WithTags(world, "Enemy", "Boss", "Elite")
    .WithComponent(world, new HealthComponent(500))
    .WithComponent(world, new BossAIComponent());

// Batch entity creation for procedural content
var bullets = Enumerable.Range(0, 10)
    .Select(i => world.CreateEntity()
        .WithName(world, $"Bullet_{i}")
        .WithPosition(world, GetBulletSpawnPosition(i))
        .WithTags(world, "Projectile", "PlayerBullet")
        .WithComponent(world, new VelocityComponent(GetBulletVelocity(i))))
    .ToList();
```

### Traditional Entity Creation (Alternative)

The traditional approach remains supported for scenarios requiring explicit control:

```csharp
// ⚠️ VERBOSE: Traditional approach - more error-prone
var entity = world.CreateEntity();
world.SetComponent(entity, new NameComponent("Player"));
world.SetComponent(entity, new TransformComponent(new Vector2D<float>(100, 200)));
world.SetComponent(entity, new TagComponent(new[] { "Player", "Controllable" }));
world.SetComponent(entity, new HealthComponent(100));
```

### Available Fluent Methods

**Core Identity Methods:**
- `WithName(world, string)` - Assigns NameComponent for entity identification
- `WithTag(world, string)` - Assigns single tag via TagComponent
- `WithTags(world, params string[])` - Assigns multiple tags via TagComponent

**Spatial Methods:**
- `WithPosition(world, Vector2D<float>)` - Sets entity position via TransformComponent
- `WithPosition(world, float x, float y)` - Convenience overload for position
- `WithTransform(world, Vector2D<float>, float, Vector2D<float>)` - Full transform setup
- `WithTransform(world, float x, float y, float rotation, float scaleX, float scaleY)` - Convenience overload

**Generic Method:**
- `WithComponent<T>(world, T component)` - Assigns any component type implementing IComponent

### Batch Operations and Performance

The ECS API provides efficient batch operations for managing multiple entities:

```csharp
// Batch entity destruction for better performance
var expiredEntities = world.Query<LifetimeComponent>()
    .Where(q => q.Component1.TimeRemaining <= 0)
    .Select(q => q.Entity);

world.DestroyEntities(expiredEntities); // More efficient than individual DestroyEntity calls

// Named entity creation with batch setup
var enemies = new[] { "Grunt", "Elite", "Boss" }
    .Select(name => world.CreateEntity()
        .WithName(world, name)
        .WithTags(world, "Enemy", "AI")
        .WithPosition(world, GetSpawnPosition(name))
        .WithComponent(world, GetEnemyStats(name)))
    .ToList();
```

**Performance Benefits:**
- **Batch Destruction**: `DestroyEntities(IEnumerable<Entity>)` reduces component storage overhead
- **Fluent API**: Zero performance overhead compared to traditional component assignment
- **Method Chaining**: Enables efficient entity setup without temporary variables

### Advanced Query Usage Patterns

The advanced query system provides two complementary approaches for complex entity discovery with flexible filtering capabilities.

#### Progressive Type Specification (Recommended for Dynamic Queries)

```csharp
// ✅ RECOMMENDED: When building queries dynamically or when primary type is unknown
var livingEnemies = world.Query()
    .With<HealthComponent>()
    .With<AIComponent>()
    .Without<PlayerComponent>()
    .Without<DeadComponent>()
    .Execute();

// Complex filtering for specialized gameplay systems
var renderableMovingEntities = world.Query()
    .With<TransformComponent>()
    .With<SpriteComponent>()
    .With<VelocityComponent>()
    .Without<HiddenComponent>()
    .Without<CulledComponent>()
    .Execute();

// Conditional query building based on game state
var query = world.Query().With<PositionComponent>();
if (includeMoving) query = query.With<VelocityComponent>();
if (excludePlayer) query = query.Without<PlayerComponent>();
var entities = query.Execute();
```

#### Direct Type Specification (Optimized for Known Primary Types)

```csharp
// ✅ OPTIMIZED: When primary component type is known at design time
var playerEntities = world.QueryBuilder<PlayerComponent>()
    .With<HealthComponent>()
    .With<InventoryComponent>()
    .Without<DeadComponent>()
    .Execute();

// Performance-critical systems with known entity archetypes
var physicsEntities = world.QueryBuilder<PhysicsBodyComponent>()
    .With<TransformComponent>()
    .With<VelocityComponent>()
    .Without<StaticComponent>()
    .Execute();
```

#### Extended Multi-Component Queries

```csharp
// Traditional approach for simple multi-component queries
foreach (var (entity, pos, vel, health) in world.Query<PositionComponent, VelocityComponent, HealthComponent>())
{
    // Direct access to all three components
}

// Extended support for complex entity archetypes (4+ components)
foreach (var (entity, transform, sprite, physics, ai, inventory) in 
    world.Query<TransformComponent, SpriteComponent, PhysicsComponent, AIComponent, InventoryComponent>())
{
    // Process highly specialized entities with five components
}
```

#### Multi-Component Helper Methods

```csharp
// Efficient batch component retrieval for individual entities
if (world.TryGetComponents<PositionComponent, VelocityComponent>(targetEntity, out var pos, out var vel))
{
    // Process specific entity with guaranteed component access
    var newPosition = pos.Value + vel.Value * deltaTime;
    world.SetComponent(targetEntity, new PositionComponent(newPosition));
}

// Complex component combinations for specialized operations
if (world.TryGetComponents<HealthComponent, ArmorComponent, ShieldComponent>(combatant, 
    out var health, out var armor, out var shield))
{
    var totalDefense = armor.Value + shield.Value;
    var damageReduction = CalculateDamageReduction(totalDefense);
    // Apply damage calculations with all defensive components
}
```

#### Performance-Optimized Query Patterns

```csharp
// Cache expensive queries when entity sets remain stable
private IEnumerable<(Entity, PositionComponent, VelocityComponent)>? _cachedMovingEntities;

public void UpdateMovementSystem(World world, float deltaTime)
{
    // Recalculate query only when needed (e.g., after entity creation/destruction)
    _cachedMovingEntities ??= world.Query<PositionComponent, VelocityComponent>().ToList();
    
    foreach (var (entity, pos, vel) in _cachedMovingEntities)
    {
        // Process cached query results for better performance
    }
}

// Use smallest component pool advantage for rare component combinations
var rareEntities = world.Query()
    .With<RareComponent>()      // Start with rarest component
    .With<CommonComponent>()    // Add more common components
    .Execute();
```

### System Implementation

Custom systems implement `ISystem` and use World queries to discover relevant entities. Systems typically iterate query results, read component data, perform calculations, and update components with new states. Frame-rate independence is achieved by scaling operations with the provided delta time.

### Hierarchy Operations

Parent-child relationships are established through `EntityHierarchyExtensions.SetParent()` or similar methods. Local transforms are set relative to parent coordinates, and world transforms are computed automatically by the TransformSystem. Hierarchy queries enable discovery of entity relationships for gameplay logic.

### Performance Optimization

Performance-critical code should minimize component queries per frame and cache query results when appropriate. Systems should process entities in batches when possible to improve cache locality. Component modifications should be batched when feasible to reduce World manipulation overhead.

## Extension Points

The ECS architecture supports extension through additional component types, custom systems, and enhanced query capabilities. New component types integrate seamlessly with existing query infrastructure, including both traditional multi-component queries and the advanced filtering system. Custom systems can implement domain-specific logic while leveraging the established component, hierarchy, and advanced query systems.

The current advanced query system provides a foundation for future enhancements such as:
- **Query Caching**: Automatic caching of expensive query results for stable entity sets
- **Parallel Query Execution**: Multi-threaded query processing for large entity collections  
- **Query Optimization**: Advanced query planning and execution optimization
- **Custom Filter Predicates**: User-defined filtering logic beyond component presence/absence

These enhancements can be implemented without fundamental architectural changes, building upon the existing builder pattern infrastructure.