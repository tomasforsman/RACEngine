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
    /// Returns FluentEntity which provides fluent component addition API and can be used as Entity.
    /// </summary>
    /// <returns>A new FluentEntity with a unique ID that supports fluent component addition.</returns>
    public FluentEntity CreateEntity()
    {
        var entity = new Entity(_nextEntityId++);
        _livingEntities.Add(entity.Id);
        return new FluentEntity(this, entity);
    }

    /// <summary>
    /// Creates a new named entity with fluent component addition API.
    /// Convenience method that creates an entity and assigns a NameComponent.
    /// </summary>
    /// <param name="name">Human-readable name for the entity</param>
    /// <returns>FluentEntity with name already assigned</returns>
    public FluentEntity CreateEntity(string name)
    {
        return CreateEntity().WithName(name);
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
    /// Creates a new entity with fluent builder API for adding components.
    /// Educational note: Enables readable entity composition with method chaining.
    /// </summary>
    /// <returns>EntityBuilder for fluent component addition</returns>
    public EntityBuilder CreateEntityBuilder()
    {
        var fluentEntity = CreateEntity();
        return new EntityBuilder(this, fluentEntity);
    }

    /// <summary>
    /// Creates a new named entity with fluent builder API.
    /// Convenience method that creates an entity and assigns a NameComponent.
    /// </summary>
    /// <param name="name">Human-readable name for the entity</param>
    /// <returns>EntityBuilder with name already assigned</returns>
    public EntityBuilder CreateEntityBuilder(string name)
    {
        var fluentEntity = CreateEntity();
        var builder = new EntityBuilder(this, fluentEntity);
        return builder.WithName(name);
    }

    /// <summary>
    /// Destroys multiple entities in a single batch operation.
    /// Educational note: Batch operations improve performance for bulk operations.
    /// </summary>
    /// <param name="entities">Collection of entities to destroy</param>
    public void DestroyEntities(IEnumerable<Entity> entities)
    {
        if (entities == null) return;

        // Convert to list to avoid multiple enumeration
        var entityList = entities.ToList();
        
        // Remove all entities from living set
        foreach (var entity in entityList)
        {
            _livingEntities.Remove(entity.Id);
        }

        // Remove from all component pools
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
    public void SetComponent<T>(Entity entity, T component)
        where T : IComponent
    {
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
}
