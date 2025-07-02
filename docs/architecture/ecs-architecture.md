---
title: "ECS Architecture"
description: "Detailed Entity-Component-System design documentation and implementation patterns"
version: "1.0.0"
last_updated: "2025-06-26"
author: "RACEngine Team"
tags: ["ecs", "architecture", "data-oriented-design"]
---

# ECS Architecture

## Overview

RACEngine implements a modern Entity-Component-System (ECS) architecture that prioritizes data-oriented design, performance, and educational clarity. This document details the ECS implementation, design decisions, and usage patterns.

## Prerequisites

- Understanding of data-oriented programming concepts
- Familiarity with C# record types and structs
- Basic knowledge of game engine architecture
- [System Overview](system-overview.md) for context

## ECS Fundamentals

### What is ECS?

Entity-Component-System is an architectural pattern that fundamentally differs from traditional object-oriented approaches by separating data (Components) from behavior (Systems) and using Entities as lightweight identifiers to link them together.

**Conceptual Differences:**

**Traditional OOP Approach:**
- Objects contain both data and behavior
- Inheritance creates rigid hierarchies
- Difficult to compose functionality
- Performance issues due to cache misses

**ECS Approach:**
- Pure data separation in components
- Behavior isolated in systems
- Flexible composition through component combinations
- Cache-friendly data layout for performance

*Implementation Details: See `src/Rac.ECS/Core/Entity.cs` and related files*

## Core Architecture

### Entity Structure

Entities in RACEngine serve as unique identifiers that link components together:

**Design Characteristics:**
- **Immutable Value Types**: Entities never change once created for thread safety
- **Minimal Memory Footprint**: Contains only essential identification data
- **Type Safety**: Strong typing prevents invalid entity operations
- **Performance Optimized**: Value type semantics avoid heap allocations

**Structural Role:**
- Acts as a key to look up associated components
- Provides lifecycle management (alive/dead state)
- Enables efficient batch operations across entity collections

*Implementation: `src/Rac.ECS/Core/Entity.cs`*

### Component Architecture

Components represent pure data containers that define what an entity is:

**Design Principles:**
- **Data-Only Structures**: No behavior, only state information
- **Immutable Records**: Prevent accidental state mutations
- **Single Responsibility**: Each component represents one aspect of an entity
- **Compositional**: Multiple components combine to create complex entities

**Common Component Categories:**
- **Transform Components**: Position, rotation, scale information
- **Physics Components**: Velocity, acceleration, collision data
- **Rendering Components**: Sprite, color, visibility information
- **Game Logic Components**: Health, inventory, AI state

*Implementation: `src/Rac.ECS/Components/` directory*

### System Architecture

Systems contain all game logic and operate on component data with full lifecycle management:

**System Lifecycle Management:**
- **Initialization Phase**: `Initialize(IWorld)` called once when system is registered
- **Update Phase**: `Update(float)` called every frame with delta time for frame-rate independence
- **Shutdown Phase**: `Shutdown(IWorld)` called once when system is removed or scheduler shuts down

**System Dependency Management:**
- **Declarative Dependencies**: Systems can declare dependencies using `[RunAfter(typeof(OtherSystem))]` attributes
- **Automatic Ordering**: SystemScheduler uses topological sorting to resolve execution order
- **Circular Dependency Detection**: Invalid dependency chains are detected and reported clearly
- **Backward Compatibility**: Systems implementing only `Update()` method continue to work unchanged

**System Processing Patterns:**
- **Update Loop Integration**: Systems execute within the main game loop in dependency order
- **Component Querying**: Systems filter entities by required components
- **Batch Processing**: Operations applied efficiently across entity collections
- **Data Transformation**: Pure functions that transform component data
- **State Isolation**: Systems remain stateless between update cycles

**Common System Categories:**
- **Input Systems**: Process user input and update input components
- **Logic Systems**: Implement game rules, AI behavior, and state management
- **Physics Systems**: Handle movement, collision detection, and response
- **Rendering Systems**: Process visual components and send data to GPU
- **Audio Systems**: Manage sound effects and music based on entity states

**System Communication Patterns:**
- **Component-Mediated**: Systems communicate through shared component data
- **Event Systems**: Decoupled communication using event publishing/subscription  
- **Service Dependencies**: Systems can depend on external services for complex operations
- **World Queries**: Systems discover relevant entities through component queries

**System Dependency Examples:**
```csharp
// Simple dependency chain: Input → Movement → Rendering
[RunAfter(typeof(InputSystem))]
public class MovementSystem : ISystem
{
    public void Initialize(IWorld world) 
    { 
        // Set up physics configuration or caches
    }
    
    public void Update(float delta) 
    { 
        // Process movement based on input
    }
    
    public void Shutdown(IWorld world) 
    { 
        // Clean up resources
    }
}

[RunAfter(typeof(MovementSystem))]
public class RenderingSystem : ISystem
{
    public void Update(float delta) 
    { 
        // Render entities after movement is complete
    }
}

// Multiple dependencies
[RunAfter(typeof(InputSystem))]
[RunAfter(typeof(PhysicsSystem))]
public class CombatSystem : ISystem
{
    public void Update(float delta) 
    { 
        // Combat logic runs after input and physics
    }
}
```

*Implementation: `src/Rac.ECS/Systems/` and scheduler implementations*

### World Management Architecture

The World serves as the central data coordinator and storage manager:

**World Responsibilities:**
- **Entity Lifecycle**: Creation, destruction, and state management
- **Component Storage**: Efficient storage and retrieval of component data
- **Query Processing**: Filtering entities by component composition
- **System Coordination**: Providing data access for system operations

**Key Design Features:**
- **Type-Safe Operations**: Generic methods ensure component type safety
- **Efficient Queries**: Fast lookups using optimized data structures
- **Memory Management**: Handles component allocation and deallocation
- **Thread Safety**: Designed for concurrent access where appropriate

*Implementation: `src/Rac.ECS/Core/World.cs`*

### IWorld Interface and Service Integration

RACEngine provides interface-based access to World functionality following engine service patterns:

**IWorld Interface Benefits:**
- **Testability**: Mock implementations enable comprehensive unit testing
- **Dependency Injection**: Consistent service registration patterns throughout engine
- **Architecture Consistency**: Matches other engine services (IAudioService, IRenderer)
- **Headless Operation**: NullWorld implementation for server/testing scenarios

**Service Patterns:**
- **Interface Abstraction**: `IWorld` defines contracts for all world operations
- **Concrete Implementation**: `World` class provides full ECS functionality
- **Null Object Pattern**: `NullWorld` provides safe no-op behavior for testing

**Integration Points:**
- **Engine Facade**: Exposes `IWorld` interface instead of concrete class
- **System Dependencies**: Systems accept `IWorld` for improved testability
- **Extension Methods**: Entity operations work with both interface and concrete types

*Implementation: `src/Rac.ECS/Core/IWorld.cs`, `src/Rac.ECS/Core/NullWorld.cs`*
    /// Destroys an entity and removes all its components
    /// </summary>
    /// <param name="entity">Entity to destroy</param>
    public void DestroyEntity(Entity entity);
    
    /// <summary>
    /// Sets a component on an entity, replacing any existing component of the same type
    /// </summary>
    /// <typeparam name="T">Component type implementing IComponent</typeparam>
    /// <param name="entity">Target entity</param>
    /// <param name="component">Component data to set</param>
    public void SetComponent<T>(Entity entity, T component) where T : struct, IComponent;
    
    /// <summary>
    /// Gets a component from an entity
    /// </summary>
    /// <typeparam name="T">Component type to retrieve</typeparam>
    /// <param name="entity">Source entity</param>
    /// <returns>Component if present, null otherwise</returns>
    public T? GetComponent<T>(Entity entity) where T : struct, IComponent;
    
    /// <summary>
    /// Removes a component from an entity
    /// </summary>
    /// <typeparam name="T">Component type to remove</typeparam>
    /// <param name="entity">Target entity</param>
    public void RemoveComponent<T>(Entity entity) where T : struct, IComponent;
    
    /// <summary>
    /// Query entities with specific component combinations
    /// </summary>
    /// <typeparam name="T1">First required component type</typeparam>
    /// <returns>Enumerable of entities with components</returns>
    public IEnumerable<(Entity, T1)> Query<T1>() where T1 : struct, IComponent;
    
    /// <summary>
    /// Query entities with two required components
    /// </summary>
    public IEnumerable<(Entity, T1, T2)> Query<T1, T2>() 
        where T1 : struct, IComponent 
        where T2 : struct, IComponent;
}
```

## Implementation Strategy

### Data Storage Approach

RACEngine employs optimized data structures for component management:

**Storage Design Principles:**
- **Sparse Array Architecture**: Provides O(1) component access while minimizing memory waste
- **Type-Specific Storage**: Each component type has dedicated storage for cache efficiency
- **Dynamic Capacity**: Storage grows as needed to accommodate new entities
- **Memory Locality**: Components of the same type stored contiguously for better performance

**Trade-offs and Rationale:**
- Memory usage vs. access speed: Sparse arrays chosen for optimal access patterns
- Flexibility vs. performance: Generic storage allows for any component type
- Growth strategy: Balanced approach between memory waste and reallocation frequency

*Implementation: Component storage classes in `src/Rac.ECS/Core/`*

### Query Processing Strategy

Efficient entity filtering and component retrieval:

**Query Architecture:**
- **Component Filtering**: Entities matched based on required component combinations
- **Lazy Evaluation**: Queries use iterator patterns for memory efficiency
- **Type Safety**: Generic constraints ensure compile-time component validation
- **Performance Optimization**: Bit masks and efficient iteration for fast queries

**Query Pattern Benefits:**
- **Composability**: Multiple component requirements can be combined
- **Safety**: Type system prevents invalid component access
- **Performance**: Optimized iteration minimizes allocation overhead
- **Flexibility**: Supports single and multi-component queries

*Implementation: Query methods in `src/Rac.ECS/Core/World.cs`*

## Design Patterns and Best Practices

### Entity Composition Patterns

RACEngine promotes composition over inheritance through flexible component combinations:

**Composition Strategy:**
- **Modular Design**: Entities built by combining independent components
- **Flexible Combinations**: Any component can be added to any entity
- **Single Responsibility**: Each component represents one specific aspect
- **Runtime Modification**: Components can be added/removed during gameplay

**Common Entity Archetypes:**
- **Player Characters**: Input, movement, health, rendering components
- **Projectiles**: Position, velocity, collision, lifetime components  
- **Static Objects**: Position, rendering, collision components
- **UI Elements**: Position, rendering, input handling components

**Benefits of Composition:**
- **Reusability**: Components shared across different entity types
- **Maintainability**: Changes to one component don't affect others
- **Testability**: Individual components can be tested in isolation
- **Flexibility**: Easy to create new entity types by mixing components

*Examples: Entity creation patterns in game systems and factories*

### System Architecture Patterns

Effective system design follows established patterns for maintainability and performance:

**System Execution Patterns:**
- **Lifecycle Management**: Systems follow Initialize → Update → Shutdown lifecycle
- **Dependency Ordering**: SystemScheduler automatically resolves execution order
- **Batch Processing**: Systems process all relevant entities in single passes
- **State Isolation**: Systems remain stateless between update cycles

**System Scheduler Architecture:**
The SystemScheduler manages the complete system lifecycle with automatic dependency resolution:

```csharp
// Example of dependency-driven system registration
var world = new World();
var scheduler = new SystemScheduler(world);

// Systems can be registered in any order - scheduler resolves dependencies
scheduler.Add(new RenderingSystem());     // Depends on TransformSystem
scheduler.Add(new MovementSystem());      // Depends on InputSystem  
scheduler.Add(new InputSystem());         // No dependencies
scheduler.Add(new TransformSystem());     // Depends on MovementSystem

// Scheduler automatically orders: Input → Movement → Transform → Rendering
scheduler.Update(deltaTime);
```

**Dependency Declaration Patterns:**
```csharp
// Simple linear dependency
[RunAfter(typeof(InputSystem))]
public class MovementSystem : ISystem
{
    public void Initialize(IWorld world) 
    {
        // Setup physics configuration
        world.SetComponent(world.CreateEntity(), new GravityComponent(-9.81f));
    }
    
    public void Update(float delta) 
    {
        // Process movement after input is handled
    }
    
    public void Shutdown(IWorld world) 
    {
        // Clean up physics resources
    }
}

// Multiple dependencies
[RunAfter(typeof(MovementSystem))]
[RunAfter(typeof(PhysicsSystem))]
public class CollisionSystem : ISystem
{
    public void Update(float delta) 
    {
        // Handle collisions after movement and physics
    }
}
```

**System Communication Patterns:**
- **Component-Mediated**: Systems communicate through shared component data
- **Event Systems**: Decoupled communication using event publishing/subscription  
- **Service Dependencies**: Systems can depend on external services for complex operations
- **World Queries**: Systems discover relevant entities through component queries

*Implementation: `src/Rac.ECS/Systems/` with SystemScheduler for lifecycle and dependency management*
```

### Component Lifecycle

Managing component creation, updates, and destruction:

```csharp
/// <summary>
/// Health system demonstrating component lifecycle management
/// Educational example of entity destruction and cleanup
/// </summary>
public class HealthSystem : ISystem
{
    public void Update(World world, float deltaTime)
    {
        var entitiesToDestroy = new List<Entity>();
        
        // Process all entities with health
        foreach (var (entity, health) in world.Query<HealthComponent>())
        {
            // Check for death condition
            if (health.Current <= 0)
            {
                // Mark for destruction (don't modify collection during iteration)
                entitiesToDestroy.Add(entity);
                
                // Trigger death effects before destruction
                TriggerDeathEffects(world, entity);
            }
        }
        
        // Batch destroy all dead entities
        foreach (var entity in entitiesToDestroy)
        {
            world.DestroyEntity(entity);
        }
    }
}
```

*Implementation Example: `src/Rac.ECS/Systems/` directory*

## Fluent Interface Architecture

### Fluent Entity API Design

RACEngine implements a comprehensive fluent interface for entity creation and component assignment, following Martin Fowler's "Fluent Interface" pattern design principles. This architectural decision significantly improves developer experience and code maintainability.

**Educational Reference**: [Fluent Interface by Martin Fowler](https://martinfowler.com/bliki/FluentInterface.html)

### Design Principles and Benefits

**Core Design Principles:**
- **Method Chaining**: Each fluent method returns the entity for continued chaining
- **Extension Methods**: Fluent API implemented through C# extension methods to avoid polluting core Entity struct
- **Type Safety**: All fluent operations maintain compile-time type safety
- **IDE Discoverability**: IntelliSense shows available With* methods after entity creation

**Architectural Benefits:**
- **Readability**: Clear, English-like syntax for entity composition
- **Maintainability**: Reduced boilerplate code decreases maintenance burden
- **Error Reduction**: Method chaining reduces chance of forgetting essential components
- **Extensibility**: New component types can add their own fluent methods without modifying core API

### Implementation Architecture

**Extension Method Pattern:**
```csharp
/// <summary>
/// Extension methods that provide a fluent API for entity component assignment.
/// Educational note: Demonstrates clean separation of concerns through extension methods
/// </summary>
public static class EntityFluentExtensions
{
    public static Entity WithName(this Entity entity, IWorld world, string name)
    {
        world.SetComponent(entity, new NameComponent(name));
        return entity; // Enable method chaining
    }
    
    public static Entity WithPosition(this Entity entity, IWorld world, Vector2D<float> position)
    {
        world.SetComponent(entity, new TransformComponent(position));
        return entity;
    }
    
    // Additional fluent methods...
}
```

**Usage Patterns:**
```csharp
// ✅ RECOMMENDED: Modern fluent approach
var player = world.CreateEntity()
    .WithName(world, "Player")
    .WithPosition(world, 100, 200)
    .WithTags(world, "Player", "Controllable")
    .WithComponent(world, new HealthComponent(100));

// ⚠️ TRADITIONAL: Verbose alternative approach
var entity = world.CreateEntity();
world.SetComponent(entity, new NameComponent("Player"));
world.SetComponent(entity, new TransformComponent(new Vector2D<float>(100, 200)));
world.SetComponent(entity, new TagComponent(new[] { "Player", "Controllable" }));
world.SetComponent(entity, new HealthComponent(100));
```

### Architectural Integration

**Component Type Coverage:**
- **Core Components**: WithName, WithTag, WithTags for entity identification
- **Spatial Components**: WithPosition, WithTransform for world positioning
- **Generic Components**: WithComponent<T> for any component type
- **Composite Operations**: WithTransform overloads for complex spatial setup

**Performance Considerations:**
- **Zero Overhead**: Fluent API compiles to identical IL as direct component assignment
- **Memory Efficiency**: No wrapper objects or additional allocations
- **Cache Friendly**: Same memory access patterns as traditional approach

**Extension Point Architecture:**
```csharp
// Example: Game-specific fluent extensions
public static class GameEntityExtensions
{
    public static Entity WithWeapon(this Entity entity, IWorld world, WeaponType weaponType)
    {
        return entity.WithComponent(world, new WeaponComponent(weaponType));
    }
    
    public static Entity WithAI(this Entity entity, IWorld world, AIBehavior behavior)
    {
        return entity.WithComponent(world, new AIComponent(behavior));
    }
}
```

*Implementation: `src/Rac.ECS/Core/EntityFluentExtensions.cs`*

## Performance Architecture

### Memory Layout Optimization

RACEngine's ECS design prioritizes cache-friendly memory patterns:

**Data-Oriented Design Principles:**
- **Component Locality**: Components of the same type stored contiguously in memory
- **Cache Efficiency**: Systems iterate through tightly packed component arrays
- **Reduced Pointer Chasing**: Direct array access rather than object graph traversal
- **Vectorization Friendly**: Memory layout supports SIMD operations where applicable

**Performance Benefits:**
- **CPU Cache Utilization**: Better cache hit rates during component iteration
- **Memory Bandwidth**: More efficient use of memory bus bandwidth
- **Branch Prediction**: Predictable iteration patterns improve CPU performance
- **Thermal Efficiency**: Reduced memory stalls lead to lower power consumption

**Design Trade-offs:**
- **Memory Overhead**: Sparse arrays may waste memory for sparse entity IDs
- **Insertion Cost**: Adding components may require array resizing
- **Complexity**: More complex than simple object-oriented approaches
- **Debugging**: Component data distributed across multiple arrays

*Implementation: Memory layout optimizations in component storage classes*

### Resource Management Strategy

Efficient handling of component lifecycle and memory allocation:

**Allocation Patterns:**
- **Component Pooling**: Reuse component storage to reduce garbage collection pressure
- **Array Reuse**: Pool component arrays for temporary operations
- **Lazy Allocation**: Allocate component storage only when first component is added
- **Capacity Growth**: Exponential growth strategies balance memory and performance

**Garbage Collection Optimization:**
- **Value Type Components**: Components are structs to avoid heap allocations
- **Immutable Data**: Reduces object mutation and GC tracking overhead
- **Array Pooling**: Reuse temporary arrays to minimize allocation pressure
- **Batch Operations**: Group operations to reduce allocation frequency

*Implementation: Resource management in `src/Rac.ECS/Core/` storage classes*

## Testing Architecture

ECS design enables comprehensive testing strategies:

**System Testing Approach:**
- **Isolated Testing**: Systems can be tested independently with mock world data
- **Behavioral Verification**: Test system behavior through component state changes
- **Integration Testing**: Multiple systems tested together for interaction validation
- **Performance Testing**: Benchmark system performance with large entity collections

**Testing Benefits:**
- **Deterministic Behavior**: Pure functions produce predictable, testable outcomes
- **Dependency Isolation**: Systems depend only on component data, not external state
- **Scenario Creation**: Easy to create specific test scenarios by configuring components
- **State Verification**: Component states provide clear verification points

**Test Strategy Categories:**
- **Unit Tests**: Individual system logic with controlled component data
- **Integration Tests**: Multiple systems working together in realistic scenarios
- **Performance Tests**: System behavior under various entity counts and configurations
- **Stress Tests**: Edge cases like massive entity counts or complex component combinations

*Testing Implementation: Test projects demonstrate testing patterns for ECS systems*

```csharp
[Test]
public void PhysicsAndMovement_IntegrateCorrectly()
{
    // Arrange
    var world = new World();
    var movementSystem = new MovementSystem();
    var physicsSystem = new PhysicsSystem();
    
    var entity = CreatePhysicsEntity(world);
    
    // Act - simulate multiple frames
    for (int frame = 0; frame < 10; frame++)
    {
        physicsSystem.Update(world, 0.016f);  // Apply forces
        movementSystem.Update(world, 0.016f); // Apply movement
    }
    
    // Assert - verify realistic physics behavior
    var finalPosition = world.GetComponent<PositionComponent>(entity);
    Assert.That(finalPosition.Value.Position.Y, Is.LessThan(0)); // Gravity effect
}
```

## Advanced Topics

### Component Events

Reactive programming with component changes:

```csharp
/// <summary>
/// Event system for component lifecycle notifications
/// Educational example of observer pattern in ECS architecture
/// </summary>
public class ComponentEventSystem
{
    public event Action<Entity, IComponent> ComponentAdded;
    public event Action<Entity, IComponent> ComponentRemoved;
    public event Action<Entity, IComponent, IComponent> ComponentChanged;
    
    public void NotifyComponentAdded<T>(Entity entity, T component) where T : struct, IComponent
    {
        ComponentAdded?.Invoke(entity, component);
    }
}
```

### Component Queries with Filters

Advanced querying with include/exclude filters:

```csharp
/// <summary>
/// Advanced query system with inclusion and exclusion filters
/// </summary>
public class QueryBuilder<T1> where T1 : struct, IComponent
{
    private readonly List<Type> _includeTypes = new();
    private readonly List<Type> _excludeTypes = new();
    
    public QueryBuilder<T1> With<T2>() where T2 : struct, IComponent
    {
        _includeTypes.Add(typeof(T2));
        return this;
    }
    
    public QueryBuilder<T1> Without<T2>() where T2 : struct, IComponent
    {
        _excludeTypes.Add(typeof(T2));
        return this;
    }
    
    public IEnumerable<(Entity, T1)> Execute(World world)
    {
        foreach (var (entity, component) in world.Query<T1>())
        {
            bool hasAllRequired = _includeTypes.All(type => world.HasComponent(entity, type));
            bool hasAnyExcluded = _excludeTypes.Any(type => world.HasComponent(entity, type));
            
            if (hasAllRequired && !hasAnyExcluded)
                yield return (entity, component);
        }
    }
}

// Usage example - Progressive Type Specification
var movableNonPlayerEntities = world.Query()
    .With<VelocityComponent>()
    .With<PositionComponent>()
    .Without<PlayerComponent>()
    .Execute();

// Alternative - Direct QueryBuilder approach
var armedEnemies = world.QueryBuilder<EnemyComponent>()
    .With<WeaponComponent>()
    .Without<DeadComponent>()
    .Execute();

// Extended Query Support (4+ components)
foreach (var (entity, pos, vel, health, weapon, ai) in world.Query<PositionComponent, VelocityComponent, HealthComponent, WeaponComponent, AIComponent>())
{
    // Process complex entity combinations
}

// Multi-Component Helpers
if (world.TryGetComponents<Position, Velocity>(entity, out var pos, out var vel))
{
    // Efficient batch component retrieval
}
```

### Advanced Query Features

The advanced query system in RACEngine provides sophisticated entity filtering capabilities:

**Progressive Type Specification:**
```csharp
// Start untyped, establish primary component with first With<T>()
var results = world.Query()
    .With<PrimaryComponent>()  // Establishes primary type
    .With<RequiredComponent>() // Additional requirements
    .Without<ExcludedComponent>() // Exclusion filters
    .Execute();
```

**Extended Query Support:**
```csharp
// Support for 4+ component combinations
var complexEntities = world.Query<T1, T2, T3, T4, T5>();

// Helper methods for batch component access
bool success = world.TryGetComponents<T1, T2, T3, T4>(entity, out var c1, out var c2, out var c3, out var c4);
```

**Performance Characteristics:**
- Maintains O(n) complexity using smallest pool iteration
- Early exit optimizations for filter evaluation
- Lazy evaluation with yield return for memory efficiency
- Supports large datasets (1000+ entities) with sub-millisecond query times
```

## See Also

- [System Overview](system-overview.md) - High-level architecture context

- [Rac.ECS Project Documentation](../projects/Rac.ECS.md) - Implementation details

## Changelog

- 2025-06-26: Comprehensive ECS architecture documentation created with examples and best practices