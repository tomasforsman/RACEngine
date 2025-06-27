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

Systems contain all game logic and operate on component data:

**System Processing Patterns:**
- **Update Loop Integration**: Systems execute within the main game loop
- **Component Querying**: Systems filter entities by required components
- **Batch Processing**: Operations applied efficiently across entity collections
- **Data Transformation**: Pure functions that transform component data

*Implementation: `src/Rac.ECS/Systems/` directory*

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
- **Update Loop Integration**: Systems execute in specific phases of the game loop
- **Dependency Ordering**: Systems execute in logical dependency order
- **Batch Processing**: Systems process all relevant entities in single passes
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

*Implementation: `src/Rac.ECS/Systems/` and scheduler implementations*
        _systems.Add(new CollisionSystem());
        
        // 4. Rendering systems (visual output)
        _systems.Add(new SpriteRenderSystem());
        _systems.Add(new ParticleRenderSystem());
    }
    
    /// <summary>
    /// Updates all systems in dependency order
    /// </summary>
    public void Update(World world, float deltaTime)
    {
        foreach (var system in _systems)
        {
            system.Update(world, deltaTime);
        }
    }
}
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

// Usage example
var movableNonPlayerEntities = world.QueryBuilder<VelocityComponent>()
    .With<PositionComponent>()
    .Without<PlayerComponent>()
    .Execute(world);
```

## See Also

- [System Overview](system-overview.md) - High-level architecture context
- [Performance Considerations](performance-considerations.md) - Optimization strategies
- [Design Patterns](design-patterns.md) - ECS and related patterns
- [Rac.ECS Project Documentation](../projects/Rac.ECS.md) - Implementation details

## Changelog

- 2025-06-26: Comprehensive ECS architecture documentation created with examples and best practices