using Rac.ECS.Components;

namespace Rac.ECS.Core;

/// <summary>
/// Interface for the ECS World, providing entity and component management operations.
/// 
/// This interface follows the service interface pattern used throughout RACEngine
/// (like IAudioService, IRenderer) to enable dependency injection, testing, and
/// architectural consistency.
/// </summary>
/// <remarks>
/// The IWorld interface enables:
/// - **Testability**: Mock implementations for unit testing
/// - **Headless Operation**: NullWorld for server/testing scenarios
/// - **Dependency Injection**: Consistent service registration patterns
/// - **Architecture Consistency**: Matches other engine service interfaces
/// 
/// Educational Note: Interface-based design is a fundamental principle in 
/// enterprise software architecture, enabling loose coupling and testability.
/// </remarks>
public interface IWorld
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ENTITY MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new entity with a unique identifier.
    /// Entities are lightweight containers that exist to group components together.
    /// </summary>
    /// <returns>A new Entity with a unique ID.</returns>
    Entity CreateEntity();

    /// <summary>
    /// Creates a new entity with a name component assigned.
    /// Convenience method equivalent to CreateEntity().WithName(this, name).
    /// </summary>
    /// <param name="name">Human-readable name for the entity</param>
    /// <returns>A new Entity with a NameComponent already assigned</returns>
    Entity CreateEntity(string name);

    /// <summary>
    /// Destroys an entity and removes all its components from the world.
    /// This is a convenience method that efficiently removes an entity from all component pools.
    /// </summary>
    /// <param name="entity">The entity to destroy.</param>
    void DestroyEntity(Entity entity);

    /// <summary>
    /// Destroys multiple entities in a single batch operation for improved performance.
    /// Educational note: Batch operations reduce overhead when destroying many entities at once.
    /// </summary>
    /// <param name="entities">Collection of entities to destroy</param>
    void DestroyEntities(IEnumerable<Entity> entities);

    /// <summary>
    /// Gets all entities currently in the world.
    /// Useful for counting entities and performing broad queries.
    /// </summary>
    /// <returns>Collection of all living entities.</returns>
    IEnumerable<Entity> GetAllEntities();

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
    void SetComponent<T>(Entity entity, T component)
        where T : IComponent;

    /// <summary>
    /// Retrieves the first registered component of type <typeparamref name="T" />,
    /// or throws if none has been set. Useful for global/singleton components.
    /// </summary>
    /// <typeparam name="T">The type of component to retrieve.</typeparam>
    /// <returns>The singleton component instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no component of the specified type is registered.</exception>
    T GetSingleton<T>()
        where T : IComponent;

    /// <summary>
    /// Removes a component of type <typeparamref name="T" /> from the given <paramref name="entity" />.
    /// This is useful for entities that no longer need certain behaviors or data.
    /// </summary>
    /// <typeparam name="T">The type of component to remove.</typeparam>
    /// <param name="entity">The entity to remove the component from.</param>
    /// <returns>True if a component was removed; false if the entity didn't have that component.</returns>
    bool RemoveComponent<T>(Entity entity)
        where T : IComponent;

    /// <summary>
    /// Checks if an entity has a specific component type.
    /// This is useful for conditional logic and filtering operations.
    /// </summary>
    /// <typeparam name="T">The type of component to check for.</typeparam>
    /// <param name="entity">The entity to check.</param>
    /// <returns>True if the entity has the component; false otherwise.</returns>
    bool HasComponent<T>(Entity entity)
        where T : IComponent;

    /// <summary>
    /// Attempts to retrieve a specific component from an entity.
    /// </summary>
    /// <typeparam name="T">The type of component to retrieve.</typeparam>
    /// <param name="entity">The entity to get the component from.</param>
    /// <param name="component">The component instance if found; default value otherwise.</param>
    /// <returns>True if the component was found; false otherwise.</returns>
    bool TryGetComponent<T>(Entity entity, out T component)
        where T : IComponent;

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPONENT QUERIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Queries all entities that have component <typeparamref name="T1" />.
    /// This is the core mechanism for system-component interactions in ECS.
    /// </summary>
    /// <typeparam name="T1">The component type to query for.</typeparam>
    /// <returns>Entities and their component instances that match the query.</returns>
    IEnumerable<(Entity Entity, T1 Component1)> Query<T1>()
        where T1 : IComponent;

    /// <summary>
    /// Queries all entities that have both components T1 and T2.
    /// Uses performance optimization by iterating the smaller component pool first.
    /// </summary>
    /// <typeparam name="T1">First component type to query for.</typeparam>
    /// <typeparam name="T2">Second component type to query for.</typeparam>
    /// <returns>Entities and their component instances that match the query.</returns>
    IEnumerable<(Entity Entity, T1 Component1, T2 Component2)> Query<T1, T2>()
        where T1 : IComponent
        where T2 : IComponent;

    /// <summary>
    /// Queries all entities that have components T1, T2, and T3.
    /// Uses performance optimization by iterating the smallest component pool first.
    /// </summary>
    /// <typeparam name="T1">First component type to query for.</typeparam>
    /// <typeparam name="T2">Second component type to query for.</typeparam>
    /// <typeparam name="T3">Third component type to query for.</typeparam>
    /// <returns>Entities and their component instances that match the query.</returns>
    IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3)> Query<T1, T2, T3>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent;

    /// <summary>
    /// Queries all entities that have components T1, T2, T3, and T4.
    /// Uses performance optimization by iterating the smallest component pool first.
    /// </summary>
    /// <typeparam name="T1">First component type to query for.</typeparam>
    /// <typeparam name="T2">Second component type to query for.</typeparam>
    /// <typeparam name="T3">Third component type to query for.</typeparam>
    /// <typeparam name="T4">Fourth component type to query for.</typeparam>
    /// <returns>Entities and their component instances that match the query.</returns>
    IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3, T4 Component4)> Query<T1, T2, T3, T4>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent;

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
    IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3, T4 Component4, T5 Component5)> Query<T1, T2, T3, T4, T5>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent;

    // ═══════════════════════════════════════════════════════════════════════════
    // ADVANCED QUERY SYSTEM
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new query root for building queries with progressive type specification.
    /// Supports the fluent syntax world.Query().With&lt;ComponentType&gt;().
    /// </summary>
    /// <returns>A new QueryRoot instance for building complex queries</returns>
    /// <remarks>
    /// This method enables the syntax specified in the requirements:
    /// world.Query().With&lt;Velocity&gt;().Without&lt;Player&gt;()
    /// 
    /// The Query() method returns an IQueryRoot that becomes typed when the first
    /// With&lt;T&gt;() method is called, establishing the primary component type.
    /// </remarks>
    IQueryRoot Query();

    /// <summary>
    /// Creates a new query builder for advanced filtering operations.
    /// Supports fluent syntax for inclusion and exclusion filters.
    /// </summary>
    /// <typeparam name="T">The primary component type to query for.</typeparam>
    /// <returns>A new QueryBuilder instance for building complex queries.</returns>
    IQueryBuilder<T> QueryBuilder<T>()
        where T : IComponent;

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
    bool TryGetComponents<T1, T2>(Entity entity, out T1 component1, out T2 component2)
        where T1 : IComponent
        where T2 : IComponent;

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
    bool TryGetComponents<T1, T2, T3>(Entity entity, out T1 component1, out T2 component2, out T3 component3)
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent;

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
    bool TryGetComponents<T1, T2, T3, T4>(Entity entity, out T1 component1, out T2 component2, out T3 component3, out T4 component4)
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent;

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING AND DEVELOPMENT TOOLS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Retrieves detailed information about an entity and all its components for debugging purposes.
    /// This method provides comprehensive entity inspection for development-time debugging.
    /// </summary>
    /// <param name="entity">The entity to inspect</param>
    /// <returns>A dictionary containing all components attached to the entity, keyed by component type name</returns>
    /// <remarks>
    /// Educational Note: This debugging method enables developers to inspect entity composition
    /// at runtime, which is crucial for understanding ECS behavior and troubleshooting issues.
    /// The method uses reflection to provide comprehensive information about all attached components.
    /// </remarks>
    Dictionary<string, object> InspectEntity(Entity entity);

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
    string GetEntityName(Entity entity);

    /// <summary>
    /// Finds all entities that have a component with the specified tag.
    /// Enables tag-based entity queries for debugging and gameplay systems.
    /// </summary>
    /// <param name="tag">The tag to search for</param>
    /// <returns>Collection of entities that have the specified tag</returns>
    /// <remarks>
    /// Educational Note: Tag-based queries are a common pattern in ECS systems for
    /// categorizing and filtering entities. This method provides direct access to
    /// tag queries at the World level, complementing the existing engine facade methods.
    /// </remarks>
    IEnumerable<Entity> GetEntitiesWithTag(string tag);
}