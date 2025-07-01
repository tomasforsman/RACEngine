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
}