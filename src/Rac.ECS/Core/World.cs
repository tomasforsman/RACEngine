// File: src/Rac.ECS.Core/World.cs

using Rac.ECS.Components;

namespace Rac.ECS.Core;

/// <summary>
/// The central container for all entities and their components in the ECS.
/// 
/// This class implements the Entity-Component-System (ECS) pattern, which is a popular
/// architectural pattern in game development that favors composition over inheritance.
/// The World acts as the database for all game objects (entities) and their data (components).
/// </summary>
public sealed class World : IWorld
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CORE STORAGE AND STATE
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // The World maintains component storage using a dictionary-of-dictionaries approach:
    // - Outer dictionary: Type -> Component pool for that type
    // - Inner dictionary: EntityId -> Component instance
    // This allows for efficient component queries and storage.
    
    private readonly Dictionary<Type, Dictionary<int, IComponent>> _components = new();
    private readonly HashSet<int> _livingEntities = new(); // Track living entities for counting and destruction
    private int _nextEntityId = 1;

    // ═══════════════════════════════════════════════════════════════════════════
    // ENTITY MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new entity with a unique identifier.
    /// Entities are lightweight containers that exist to group components together.
    /// </summary>
    /// <returns>A new Entity with a unique ID.</returns>
    public Entity CreateEntity()
    {
        var entity = new Entity(_nextEntityId++);
        _livingEntities.Add(entity.Id);
        return entity;
    }

    /// <summary>
    /// Creates a new entity with a name component assigned.
    /// Convenience method equivalent to CreateEntity().WithName(this, name).
    /// </summary>
    /// <param name="name">Human-readable name for the entity</param>
    /// <returns>A new Entity with a NameComponent already assigned</returns>
    public Entity CreateEntity(string name)
    {
        var entity = CreateEntity();
        SetComponent(entity, new NameComponent(name ?? string.Empty));
        return entity;
    }

    /// <summary>
    /// Destroys an entity and removes all its components from the world.
    /// This efficiently removes the entity from all component pools and tracking structures.
    /// </summary>
    /// <param name="entity">The entity to destroy.</param>
    public void DestroyEntity(Entity entity)
    {
        if (!_livingEntities.Remove(entity.Id))
        {
            // Entity was already destroyed or never existed
            return;
        }

        // Remove entity from all component pools
        foreach (var pool in _components.Values)
        {
            pool.Remove(entity.Id);
        }
    }

    /// <summary>
    /// Destroys multiple entities in a single batch operation for improved performance.
    /// Educational note: Batch operations reduce overhead when destroying many entities at once.
    /// </summary>
    /// <param name="entities">Collection of entities to destroy</param>
    /// <remarks>
    /// Batch operations are an important optimization in game engines where hundreds or thousands
    /// of entities might need to be destroyed simultaneously (e.g., clearing a level, despawning
    /// a large group of enemies). This approach is more efficient than calling DestroyEntity
    /// repeatedly due to reduced method call overhead and potential memory allocation optimizations.
    /// </remarks>
    public void DestroyEntities(IEnumerable<Entity> entities)
    {
        if (entities == null) return;

        // Convert to list to avoid multiple enumeration and enable efficient iteration
        var entityList = entities.ToList();
        if (entityList.Count == 0) return;

        // Remove from living entities tracking in batch
        foreach (var entity in entityList)
        {
            _livingEntities.Remove(entity.Id);
        }

        // Remove components from all pools efficiently
        foreach (var pool in _components.Values)
        {
            foreach (var entity in entityList)
            {
                pool.Remove(entity.Id);
            }
        }
    }

    /// <summary>
    /// Gets all entities currently in the world.
    /// Returns all living entities that have been created but not destroyed.
    /// </summary>
    /// <returns>Collection of all living entities.</returns>
    public IEnumerable<Entity> GetAllEntities()
    {
        return _livingEntities.Select(id => new Entity(id));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPONENT MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Adds or replaces a component of type <typeparamref name="T" /> on the given <paramref name="entity" />.
    /// Components represent the data associated with an entity (position, health, etc.).
    /// </summary>
    /// <typeparam name="T">The type of component to add.</typeparam>
    /// <param name="entity">The entity to add the component to.</param>
    /// <param name="component">The component instance to add.</param>
    /// <exception cref="ArgumentException">Thrown when the entity does not exist (development mode validation)</exception>
    public void SetComponent<T>(Entity entity, T component)
        where T : IComponent
    {
        // Development-mode validation: Check if entity exists
        ValidateEntityExists(entity, nameof(entity));

        var componentType = typeof(T);
        if (!_components.TryGetValue(componentType, out var pool))
        {
            pool = new Dictionary<int, IComponent>();
            _components[componentType] = pool;
        }

        pool[entity.Id] = component;
    }

    /// <summary>
    /// Retrieves the first registered component of type <typeparamref name="T" />,
    /// or throws if none has been set. Useful for global/singleton components.
    /// </summary>
    /// <typeparam name="T">The type of component to retrieve.</typeparam>
    /// <returns>The singleton component instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no component of the specified type is registered.</exception>
    public T GetSingleton<T>()
        where T : IComponent
    {
        var componentType = typeof(T);
        if (_components.TryGetValue(componentType, out var pool) && pool.Count > 0)
            return (T)pool.Values.First();
        throw new InvalidOperationException(
            $"No singleton component of type {componentType.Name} registered."
        );
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPONENT QUERIES - SINGLE TYPE
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // Query methods allow systems to efficiently find entities with specific components.
    // This is the core mechanism for system-component interactions in ECS.

    /// <summary>
    ///   Queries all entities that have component <typeparamref name="T1" />.
    /// </summary>
    public IEnumerable<(Entity Entity, T1 Component1)> Query<T1>()
        where T1 : IComponent
    {
        var pool1 = GetPool<T1>();

        foreach ((int entityId, var component) in pool1)
            yield return (new Entity(entityId), (T1)component);
    }

    // ───────────────────────────────────────────────────────────────────────────
    // MULTI-COMPONENT QUERIES
    // ───────────────────────────────────────────────────────────────────────────
    //
    // Multi-component queries use intersection logic to find entities that have
    // all specified component types. Performance optimization: iterate the smallest
    // component pool first to reduce unnecessary checks.

    /// <summary>
    /// Queries all entities that have both components T1 and T2.
    /// Uses performance optimization by iterating the smaller component pool first.
    /// </summary>
    /// <typeparam name="T1">First component type to query for.</typeparam>
    /// <typeparam name="T2">Second component type to query for.</typeparam>
    /// <returns>Entities and their component instances that match the query.</returns>
    public IEnumerable<(Entity Entity, T1 Component1, T2 Component2)> Query<T1, T2>()
        where T1 : IComponent
        where T2 : IComponent
    {
        var pool1 = GetPool<T1>();
        var pool2 = GetPool<T2>();

        // Iterate the smaller pool for better performance
        var (smallerPool, largerPool) =
            pool1.Count <= pool2.Count ? (pool1, pool2) : (pool2, pool1);

        foreach (var entry in smallerPool)
        {
            int entityId = entry.Key;
            if (pool1.ContainsKey(entityId) && pool2.ContainsKey(entityId))
                yield return (new Entity(entityId), (T1)pool1[entityId], (T2)pool2[entityId]);
        }
    }

    /// <summary>
    ///   Queries all entities that have components T1, T2, and T3.
    /// </summary>
    public IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3)> Query<
        T1,
        T2,
        T3
    >()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    {
        var pool1 = GetPool<T1>();
        var pool2 = GetPool<T2>();
        var pool3 = GetPool<T3>();

        // Pick the smallest pool to drive iteration
        var smallestPool = new[] { pool1, pool2, pool3 }.OrderBy(p => p.Count).First();

        foreach (var entry in smallestPool)
        {
            int entityId = entry.Key;
            if (
                pool1.ContainsKey(entityId)
                && pool2.ContainsKey(entityId)
                && pool3.ContainsKey(entityId)
            )
                yield return (
                    new Entity(entityId),
                    (T1)pool1[entityId],
                    (T2)pool2[entityId],
                    (T3)pool3[entityId]
                );
        }
    }

    /// <summary>
    /// Queries all entities that have components T1, T2, T3, and T4.
    /// Uses performance optimization by iterating the smallest component pool first.
    /// </summary>
    /// <typeparam name="T1">First component type to query for.</typeparam>
    /// <typeparam name="T2">Second component type to query for.</typeparam>
    /// <typeparam name="T3">Third component type to query for.</typeparam>
    /// <typeparam name="T4">Fourth component type to query for.</typeparam>
    /// <returns>Entities and their component instances that match the query.</returns>
    public IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3, T4 Component4)> Query<T1, T2, T3, T4>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
    {
        var pool1 = GetPool<T1>();
        var pool2 = GetPool<T2>();
        var pool3 = GetPool<T3>();
        var pool4 = GetPool<T4>();

        // Pick the smallest pool to drive iteration for optimal performance
        var smallestPool = new[] { pool1, pool2, pool3, pool4 }.OrderBy(p => p.Count).First();

        foreach (var entry in smallestPool)
        {
            int entityId = entry.Key;
            if (
                pool1.ContainsKey(entityId)
                && pool2.ContainsKey(entityId)
                && pool3.ContainsKey(entityId)
                && pool4.ContainsKey(entityId)
            )
                yield return (
                    new Entity(entityId),
                    (T1)pool1[entityId],
                    (T2)pool2[entityId],
                    (T3)pool3[entityId],
                    (T4)pool4[entityId]
                );
        }
    }

    /// <summary>
    /// Queries all entities that have components T1, T2, T3, T4, and T5.
    /// Uses performance optimization by iterating the smallest component pool first.
    /// </summary>
    /// <typeparam name="T1">First component type to query for.</typeparam>
    /// <typeparam name="T2">Second component type to query for.</typeparam>
    /// <typeparam name="T3">Third component type to query for.</typeparam>
    /// <typeparam name="T4">Fourth component type to query for.</typeparam>
    /// <typeparam name="T5">Fifth component type to query for.</typeparam>
    /// <returns>Entities and their component instances that match the query.</returns>
    public IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3, T4 Component4, T5 Component5)> Query<T1, T2, T3, T4, T5>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
    {
        var pool1 = GetPool<T1>();
        var pool2 = GetPool<T2>();
        var pool3 = GetPool<T3>();
        var pool4 = GetPool<T4>();
        var pool5 = GetPool<T5>();

        // Pick the smallest pool to drive iteration for optimal performance
        var smallestPool = new[] { pool1, pool2, pool3, pool4, pool5 }.OrderBy(p => p.Count).First();

        foreach (var entry in smallestPool)
        {
            int entityId = entry.Key;
            if (
                pool1.ContainsKey(entityId)
                && pool2.ContainsKey(entityId)
                && pool3.ContainsKey(entityId)
                && pool4.ContainsKey(entityId)
                && pool5.ContainsKey(entityId)
            )
                yield return (
                    new Entity(entityId),
                    (T1)pool1[entityId],
                    (T2)pool2[entityId],
                    (T3)pool3[entityId],
                    (T4)pool4[entityId],
                    (T5)pool5[entityId]
                );
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ADVANCED QUERY SYSTEM
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new query root for building queries with progressive type specification.
    /// Supports the fluent syntax world.Query().With&lt;ComponentType&gt;().
    /// </summary>
    /// <returns>A new QueryRoot instance for building complex queries</returns>
    /// <remarks>
    /// Educational Note: Progressive Type Specification
    /// 
    /// This method enables the exact syntax specified in the requirements:
    /// world.Query().With&lt;Velocity&gt;().Without&lt;Player&gt;()
    /// 
    /// The implementation uses a two-phase approach:
    /// 1. Query() returns an untyped IQueryRoot
    /// 2. With&lt;T&gt;() converts it to a typed IQueryBuilder&lt;T&gt;
    /// 
    /// This pattern provides maximum flexibility while maintaining type safety.
    /// </remarks>
    public IQueryRoot Query()
    {
        return new QueryRoot(this);
    }

    /// <summary>
    /// Creates a new query builder for advanced filtering operations.
    /// Supports fluent syntax for inclusion and exclusion filters.
    /// </summary>
    /// <typeparam name="T">The primary component type to query for.</typeparam>
    /// <returns>A new QueryBuilder instance for building complex queries.</returns>
    /// <remarks>
    /// Educational Note: Builder Pattern with Fluent Interface
    /// 
    /// This method demonstrates the Builder pattern combined with a fluent interface
    /// to create complex queries in a readable and maintainable way. The pattern
    /// separates query construction from execution, allowing for optimization and
    /// better code organization.
    /// 
    /// Example Usage:
    /// var enemies = world.QueryBuilder&lt;PositionComponent&gt;()
    ///     .With&lt;HealthComponent&gt;()
    ///     .With&lt;AIComponent&gt;()
    ///     .Without&lt;PlayerComponent&gt;()
    ///     .Without&lt;DeadComponent&gt;()
    ///     .Execute();
    /// </remarks>
    public IQueryBuilder<T> QueryBuilder<T>()
        where T : IComponent
    {
        return new QueryBuilder<T>(this);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MULTI-COMPONENT HELPER METHODS  
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to retrieve two components from an entity in a single operation.
    /// This is more efficient than calling TryGetComponent twice.
    /// </summary>
    /// <typeparam name="T1">First component type to retrieve.</typeparam>
    /// <typeparam name="T2">Second component type to retrieve.</typeparam>
    /// <param name="entity">The entity to get components from.</param>
    /// <param name="component1">The first component if found; default value otherwise.</param>
    /// <param name="component2">The second component if found; default value otherwise.</param>
    /// <returns>True if both components were found; false otherwise.</returns>
    public bool TryGetComponents<T1, T2>(Entity entity, out T1 component1, out T2 component2)
        where T1 : IComponent
        where T2 : IComponent
    {
        bool found1 = TryGetComponent(entity, out component1);
        bool found2 = TryGetComponent(entity, out component2);
        
        return found1 && found2;
    }

    /// <summary>
    /// Attempts to retrieve three components from an entity in a single operation.
    /// This is more efficient than calling TryGetComponent three times.
    /// </summary>
    /// <typeparam name="T1">First component type to retrieve.</typeparam>
    /// <typeparam name="T2">Second component type to retrieve.</typeparam>
    /// <typeparam name="T3">Third component type to retrieve.</typeparam>
    /// <param name="entity">The entity to get components from.</param>
    /// <param name="component1">The first component if found; default value otherwise.</param>
    /// <param name="component2">The second component if found; default value otherwise.</param>
    /// <param name="component3">The third component if found; default value otherwise.</param>
    /// <returns>True if all three components were found; false otherwise.</returns>
    public bool TryGetComponents<T1, T2, T3>(Entity entity, out T1 component1, out T2 component2, out T3 component3)
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    {
        bool found1 = TryGetComponent(entity, out component1);
        bool found2 = TryGetComponent(entity, out component2);
        bool found3 = TryGetComponent(entity, out component3);
        
        return found1 && found2 && found3;
    }

    /// <summary>
    /// Attempts to retrieve four components from an entity in a single operation.
    /// This is more efficient than calling TryGetComponent four times.
    /// </summary>
    /// <typeparam name="T1">First component type to retrieve.</typeparam>
    /// <typeparam name="T2">Second component type to retrieve.</typeparam>
    /// <typeparam name="T3">Third component type to retrieve.</typeparam>
    /// <typeparam name="T4">Fourth component type to retrieve.</typeparam>
    /// <param name="entity">The entity to get components from.</param>
    /// <param name="component1">The first component if found; default value otherwise.</param>
    /// <param name="component2">The second component if found; default value otherwise.</param>
    /// <param name="component3">The third component if found; default value otherwise.</param>
    /// <param name="component4">The fourth component if found; default value otherwise.</param>
    /// <returns>True if all four components were found; false otherwise.</returns>
    public bool TryGetComponents<T1, T2, T3, T4>(Entity entity, out T1 component1, out T2 component2, out T3 component3, out T4 component4)
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
    {
        bool found1 = TryGetComponent(entity, out component1);
        bool found2 = TryGetComponent(entity, out component2);
        bool found3 = TryGetComponent(entity, out component3);
        bool found4 = TryGetComponent(entity, out component4);
        
        return found1 && found2 && found3 && found4;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INTERNAL UTILITIES AND MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Retrieves (or creates) the dictionary pool for components of type T.
    /// This is an internal utility method that ensures component pools exist before access.
    /// </summary>
    /// <typeparam name="T">The component type to get a pool for.</typeparam>
    /// <returns>The dictionary pool for the specified component type.</returns>
    private Dictionary<int, IComponent> GetPool<T>()
        where T : IComponent
    {
        var componentType = typeof(T);
        if (!_components.TryGetValue(componentType, out var pool))
        {
            pool = new Dictionary<int, IComponent>();
            _components[componentType] = pool;
        }

        return pool;
    }

    /// <summary>
    /// Removes a component of type <typeparamref name="T" /> from the given <paramref name="entity" />.
    /// This is useful for entities that no longer need certain behaviors or data.
    /// </summary>
    /// <typeparam name="T">The type of component to remove.</typeparam>
    /// <param name="entity">The entity to remove the component from.</param>
    /// <returns>True if a component was removed; false if the entity didn't have that component.</returns>
    public bool RemoveComponent<T>(Entity entity)
        where T : IComponent
    {
        var componentType = typeof(T);
        if (_components.TryGetValue(componentType, out var pool))
            return pool.Remove(entity.Id);
        return false;
    }

    /// <summary>
    /// Checks if an entity has a specific component type.
    /// This is useful for conditional logic and filtering operations.
    /// </summary>
    /// <typeparam name="T">The type of component to check for.</typeparam>
    /// <param name="entity">The entity to check.</param>
    /// <returns>True if the entity has the component; false otherwise.</returns>
    /// <remarks>
    /// This method returns false for non-existent entities instead of throwing,
    /// making it safe to use in conditional logic without explicit entity existence checks.
    /// </remarks>
    public bool HasComponent<T>(Entity entity)
        where T : IComponent
    {
        // Gracefully handle non-existent entities by returning false
        if (!_livingEntities.Contains(entity.Id))
        {
            return false;
        }

        var componentType = typeof(T);
        return _components.TryGetValue(componentType, out var pool) && pool.ContainsKey(entity.Id);
    }

    /// <summary>
    /// Attempts to retrieve a specific component from an entity.
    /// This method provides a safe way to access components without throwing exceptions.
    /// </summary>
    /// <typeparam name="T">The type of component to retrieve.</typeparam>
    /// <param name="entity">The entity to get the component from.</param>
    /// <param name="component">The component instance if found; default value otherwise.</param>
    /// <returns>True if the component was found; false otherwise.</returns>
    public bool TryGetComponent<T>(Entity entity, out T component)
        where T : IComponent
    {
        var componentType = typeof(T);
        if (_components.TryGetValue(componentType, out var pool) && 
            pool.TryGetValue(entity.Id, out var componentObj))
        {
            component = (T)componentObj;
            return true;
        }
        
        component = default!;
        return false;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING AND DEVELOPMENT TOOLS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Retrieves detailed information about an entity and all its components for debugging purposes.
    /// This method provides comprehensive entity inspection for development-time debugging.
    /// </summary>
    /// <param name="entity">The entity to inspect</param>
    /// <returns>A dictionary containing all components attached to the entity, keyed by component type name</returns>
    /// <exception cref="ArgumentException">Thrown when the entity does not exist</exception>
    /// <remarks>
    /// Educational Note: This debugging method enables developers to inspect entity composition
    /// at runtime, which is crucial for understanding ECS behavior and troubleshooting issues.
    /// The method uses reflection to provide comprehensive information about all attached components.
    /// </remarks>
    public Dictionary<string, object> InspectEntity(Entity entity)
    {
        if (!_livingEntities.Contains(entity.Id))
        {
            throw new ArgumentException($"Entity with ID {entity.Id} does not exist or has been destroyed", nameof(entity));
        }

        var inspection = new Dictionary<string, object>();
        
        // Iterate through all component pools to find components for this entity
        foreach (var (componentType, pool) in _components)
        {
            if (pool.TryGetValue(entity.Id, out var component))
            {
                // Use friendly type name for debugging
                var typeName = componentType.Name;
                if (typeName.EndsWith("Component"))
                {
                    typeName = typeName[..^"Component".Length];
                }
                
                inspection[typeName] = component;
            }
        }

        return inspection;
    }

    /// <summary>
    /// Gets the name of an entity, or its ID if no NameComponent is present.
    /// Provides a human-readable identifier for entities in debugging scenarios.
    /// </summary>
    /// <param name="entity">The entity to get the name for</param>
    /// <returns>Entity name if NameComponent exists, otherwise "Entity #{ID}"</returns>
    /// <remarks>
    /// This method follows the pattern of providing fallback identification when explicit names
    /// are not available, ensuring every entity can be meaningfully referenced in debug output.
    /// </remarks>
    public string GetEntityName(Entity entity)
    {
        if (TryGetComponent<NameComponent>(entity, out var nameComponent))
        {
            return nameComponent.Name;
        }
        
        return $"Entity #{entity.Id}";
    }

    /// <summary>
    /// Finds all entities that have a component with the specified tag.
    /// Enables tag-based entity queries for debugging and gameplay systems.
    /// </summary>
    /// <param name="tag">The tag to search for</param>
    /// <returns>Collection of entities that have the specified tag</returns>
    /// <exception cref="ArgumentNullException">Thrown when tag is null</exception>
    /// <remarks>
    /// Educational Note: Tag-based queries are a common pattern in ECS systems for
    /// categorizing and filtering entities. This method provides direct access to
    /// tag queries at the World level, complementing the existing engine facade methods.
    /// </remarks>
    public IEnumerable<Entity> GetEntitiesWithTag(string tag)
    {
        if (tag == null)
        {
            throw new ArgumentNullException(nameof(tag), "Tag cannot be null. Use string.Empty for empty tags.");
        }

        return Query<TagComponent>()
            .Where(result => result.Component1.HasTag(tag))
            .Select(result => result.Entity);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEVELOPMENT MODE VALIDATION HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates that an entity exists in the world.
    /// Provides helpful error messages for common development mistakes.
    /// </summary>
    /// <param name="entity">The entity to validate</param>
    /// <param name="paramName">The parameter name for exception reporting</param>
    /// <exception cref="ArgumentException">Thrown when entity does not exist with helpful diagnostic information</exception>
    private void ValidateEntityExists(Entity entity, string paramName)
    {
        if (!_livingEntities.Contains(entity.Id))
        {
            // Provide helpful diagnostic information
            var errorMessage = entity.Id <= 0
                ? $"Entity with ID {entity.Id} appears to be invalid. Ensure you're using entities created by World.CreateEntity()."
                : $"Entity with ID {entity.Id} does not exist or has been destroyed. " +
                  $"Common causes: entity was destroyed, entity is from a different world, or entity was never created.";

            throw new ArgumentException(errorMessage, paramName);
        }
    }
}
