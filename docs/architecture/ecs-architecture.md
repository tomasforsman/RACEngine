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

Entity-Component-System is an architectural pattern that separates data (Components) from behavior (Systems) and uses Entities as lightweight identifiers to link them together.

**Traditional OOP vs ECS:**
```csharp
// Traditional OOP approach
public class GameObject
{
    public Vector2 Position { get; set; }
    public float Health { get; set; }
    public void Update() { /* mixed data and behavior */ }
    public void Render() { /* inheritance issues */ }
}

// ECS approach
public readonly record struct Entity(int Id, bool IsAlive = true);
public readonly record struct PositionComponent(Vector2 Position) : IComponent;
public readonly record struct HealthComponent(float Health) : IComponent;
public class MovementSystem : ISystem { /* pure behavior */ }
public class RenderSystem : ISystem { /* focused responsibility */ }
```

## Core Components

### Entity

Entities are lightweight identifiers that represent game objects:

```csharp
/// <summary>
/// Represents a unique entity in the game world.
/// Entities are immutable identifiers that link components together.
/// </summary>
/// <param name="Id">Unique identifier for this entity</param>
/// <param name="IsAlive">Whether this entity is active in the world</param>
public readonly record struct Entity(int Id, bool IsAlive = true)
{
    /// <summary>
    /// Invalid entity constant for null checks and initialization
    /// </summary>
    public static readonly Entity Invalid = new(-1, false);
    
    /// <summary>
    /// Checks if this entity is valid and alive
    /// </summary>
    public bool IsValid => Id >= 0 && IsAlive;
}
```

**Entity Design Principles:**
- **Immutable**: Entities never change once created
- **Lightweight**: Only an ID and alive flag
- **Type-safe**: Strong typing prevents invalid operations
- **Efficient**: Value type semantics for performance

### Components

Components are pure data containers that implement the `IComponent` marker interface:

```csharp
/// <summary>
/// Marker interface for all ECS components.
/// Components must be readonly record structs for immutability and performance.
/// </summary>
public interface IComponent { }

/// <summary>
/// Position component storing 2D coordinates
/// </summary>
/// <param name="Position">World position coordinates</param>
public readonly record struct PositionComponent(Vector2 Position) : IComponent;

/// <summary>
/// Velocity component for movement calculations
/// </summary>
/// <param name="Velocity">Velocity vector in units per second</param>
public readonly record struct VelocityComponent(Vector2 Velocity) : IComponent;

/// <summary>
/// Health component for destructible entities
/// </summary>
/// <param name="Current">Current health points</param>
/// <param name="Maximum">Maximum health points</param>
public readonly record struct HealthComponent(float Current, float Maximum) : IComponent;
```

**Component Design Rules:**
- **readonly record struct**: Immutable, efficient value types
- **Pure data**: No behavior, only data storage
- **Single responsibility**: Each component has one clear purpose
- **Composable**: Components can be combined freely

### Systems

Systems contain pure logic and operate on World data:

```csharp
/// <summary>
/// Base interface for all ECS systems
/// </summary>
public interface ISystem
{
    /// <summary>
    /// Update system logic for current frame
    /// </summary>
    /// <param name="world">World containing entity and component data</param>
    /// <param name="deltaTime">Time elapsed since last frame in seconds</param>
    void Update(World world, float deltaTime);
}

/// <summary>
/// Movement system that applies velocity to position
/// Educational example of physics integration in game engines
/// </summary>
public class MovementSystem : ISystem
{
    public void Update(World world, float deltaTime)
    {
        // Query entities with both Position and Velocity components
        foreach (var (entity, position, velocity) in world.Query<PositionComponent, VelocityComponent>())
        {
            // Calculate new position using basic physics: position = position + velocity * time
            var newPosition = position.Position + velocity.Velocity * deltaTime;
            
            // Update component with new position
            world.SetComponent(entity, new PositionComponent(newPosition));
        }
    }
}
```

**System Design Principles:**
- **Stateless**: Systems don't store data between frames
- **Pure functions**: Same input always produces same output
- **Single purpose**: Each system handles one specific concern
- **Testable**: Easy to unit test with mock World data

### World

The World manages entity creation, component storage, and querying:

```csharp
/// <summary>
/// Central repository for entities and components in the ECS architecture.
/// Provides efficient storage and querying capabilities.
/// </summary>
public class World
{
    /// <summary>
    /// Creates a new entity with unique ID
    /// </summary>
    /// <returns>New entity ready for component assignment</returns>
    public Entity CreateEntity();
    
    /// <summary>
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

## Implementation Details

### Component Storage

RACEngine uses sparse arrays for efficient component storage:

```csharp
/// <summary>
/// Sparse array storage for components of type T
/// Provides O(1) access while minimizing memory usage
/// </summary>
internal class ComponentArray<T> where T : struct, IComponent
{
    private T[] _components;
    private bool[] _hasComponent;
    private int _capacity;
    
    /// <summary>
    /// Sets component for entity, growing array if necessary
    /// Educational note: Sparse arrays trade memory for access speed
    /// </summary>
    public void SetComponent(int entityId, T component)
    {
        EnsureCapacity(entityId + 1);
        _components[entityId] = component;
        _hasComponent[entityId] = true;
    }
    
    /// <summary>
    /// Gets component for entity with O(1) access time
    /// </summary>
    public T? GetComponent(int entityId)
    {
        if (entityId >= _capacity || !_hasComponent[entityId])
            return null;
        return _components[entityId];
    }
}
```

### Query Implementation

Efficient querying using component masks and iteration:

```csharp
/// <summary>
/// Query implementation for entities with specific component combinations
/// Uses bit masks for efficient filtering
/// </summary>
public IEnumerable<(Entity, T1, T2)> Query<T1, T2>() 
    where T1 : struct, IComponent 
    where T2 : struct, IComponent
{
    var componentArray1 = GetComponentArray<T1>();
    var componentArray2 = GetComponentArray<T2>();
    
    // Iterate through all entities
    for (int i = 0; i < _entityCount; i++)
    {
        if (!_entities[i].IsAlive)
            continue;
            
        var component1 = componentArray1.GetComponent(i);
        var component2 = componentArray2.GetComponent(i);
        
        // Yield entity only if it has both required components
        if (component1.HasValue && component2.HasValue)
        {
            yield return (_entities[i], component1.Value, component2.Value);
        }
    }
}
```

## Common Patterns

### Component Composition

Building complex entities through component combination:

```csharp
/// <summary>
/// Creates a player entity with all necessary components
/// Demonstrates composition over inheritance principle
/// </summary>
public Entity CreatePlayer(World world, Vector2 position)
{
    var player = world.CreateEntity();
    
    // Core components
    world.SetComponent(player, new PositionComponent(position));
    world.SetComponent(player, new VelocityComponent(Vector2.Zero));
    
    // Gameplay components
    world.SetComponent(player, new HealthComponent(100f, 100f));
    world.SetComponent(player, new PlayerInputComponent());
    
    // Visual components
    world.SetComponent(player, new SpriteComponent("player.png"));
    world.SetComponent(player, new AnimationComponent("idle"));
    
    // Audio components
    world.SetComponent(player, new AudioSourceComponent());
    
    return player;
}
```

### System Dependencies

Managing system execution order and dependencies:

```csharp
/// <summary>
/// System scheduler that manages execution order
/// Educational example of dependency management in game engines
/// </summary>
public class SystemScheduler
{
    private readonly List<ISystem> _systems = new();
    
    /// <summary>
    /// Registers systems in dependency order
    /// Order matters: Input → Logic → Physics → Rendering
    /// </summary>
    public void RegisterSystems()
    {
        // 1. Input systems (process user input)
        _systems.Add(new InputSystem());
        
        // 2. Logic systems (game rules and AI)
        _systems.Add(new PlayerMovementSystem());
        _systems.Add(new AIBehaviorSystem());
        
        // 3. Physics systems (collision and movement)
        _systems.Add(new PhysicsSystem());
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
        
        // Destroy dead entities
        foreach (var entity in entitiesToDestroy)
        {
            world.DestroyEntity(entity);
        }
    }
    
    private void TriggerDeathEffects(World world, Entity entity)
    {
        // Add particle effects
        if (world.GetComponent<PositionComponent>(entity) is var pos)
        {
            world.SetComponent(entity, new ParticleEffectComponent("death_explosion"));
        }
        
        // Play death sound
        if (world.GetComponent<AudioSourceComponent>(entity) is var audio)
        {
            world.SetComponent(entity, new PlaySoundComponent("death_sound"));
        }
    }
}
```

## Performance Considerations

### Memory Layout

Components are stored contiguously for cache efficiency:

```csharp
/// <summary>
/// Memory layout optimization for component iteration
/// Educational note: Data-oriented design principles in practice
/// </summary>
public void OptimizedSystemUpdate(World world, float deltaTime)
{
    // Bad: Object-oriented approach with poor cache locality
    // foreach (var gameObject in gameObjects)
    //     gameObject.UpdatePhysics(deltaTime);
    
    // Good: Data-oriented approach with excellent cache locality
    foreach (var (entity, position, velocity) in world.Query<PositionComponent, VelocityComponent>())
    {
        // All position components are stored contiguously in memory
        // All velocity components are stored contiguously in memory
        // CPU cache can efficiently prefetch data for next iterations
        var newPosition = position.Position + velocity.Velocity * deltaTime;
        world.SetComponent(entity, new PositionComponent(newPosition));
    }
}
```

### Component Pooling

Reusing component arrays to reduce garbage collection:

```csharp
/// <summary>
/// Component array pooling to reduce allocations
/// Educational example of memory management in high-performance systems
/// </summary>
public class ComponentPool<T> where T : struct, IComponent
{
    private readonly Stack<T[]> _arrayPool = new();
    private readonly int _arraySize;
    
    public T[] RentArray()
    {
        if (_arrayPool.Count > 0)
            return _arrayPool.Pop();
        
        return new T[_arraySize];
    }
    
    public void ReturnArray(T[] array)
    {
        // Clear array before returning to pool
        Array.Clear(array, 0, array.Length);
        _arrayPool.Push(array);
    }
}
```

## Testing Strategies

### Unit Testing Systems

Testing systems in isolation:

```csharp
[Test]
public void MovementSystem_AppliesVelocityToPosition()
{
    // Arrange
    var world = new World();
    var system = new MovementSystem();
    var entity = world.CreateEntity();
    
    var initialPosition = new Vector2(0, 0);
    var velocity = new Vector2(10, 5);
    var deltaTime = 0.1f;
    
    world.SetComponent(entity, new PositionComponent(initialPosition));
    world.SetComponent(entity, new VelocityComponent(velocity));
    
    // Act
    system.Update(world, deltaTime);
    
    // Assert
    var finalPosition = world.GetComponent<PositionComponent>(entity);
    var expectedPosition = initialPosition + velocity * deltaTime;
    
    Assert.That(finalPosition.Value.Position, Is.EqualTo(expectedPosition).Within(0.001f));
}
```

### Integration Testing

Testing system interactions:

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