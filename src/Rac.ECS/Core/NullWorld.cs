using Rac.ECS.Components;

namespace Rac.ECS.Core;

/// <summary>
/// Null Object Pattern implementation of IWorld for testing and headless scenarios.
/// 
/// This implementation provides safe no-op behavior for all World operations,
/// following the Null Object Pattern to eliminate the need for null checks
/// and provide predictable behavior in testing scenarios.
/// </summary>
/// <remarks>
/// Educational Design Pattern: The Null Object Pattern
/// 
/// Instead of returning null and requiring null checks everywhere, the Null Object
/// pattern provides an object that implements the same interface but with safe,
/// do-nothing behavior. This is particularly useful in:
/// 
/// - **Unit Testing**: Test systems without setting up full World state
/// - **Headless Servers**: Run game logic without entity management
/// - **Safe Defaults**: Prevent null reference exceptions in edge cases
/// 
/// Academic Reference: "Design Patterns: Elements of Reusable Object-Oriented Software"
/// by Gamma, Helm, Johnson, and Vlissides (Gang of Four), 1994
/// </remarks>
public sealed class NullWorld : IWorld
{
    // ═══════════════════════════════════════════════════════════════════════════
    // INVALID ENTITY CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════
    
    private static readonly Entity InvalidEntity = new Entity(0, false);

    // ═══════════════════════════════════════════════════════════════════════════
    // ENTITY MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an invalid entity for safe no-op behavior.
    /// Returns a dead entity that won't be processed by systems.
    /// </summary>
    /// <returns>An invalid Entity that systems should ignore.</returns>
    public Entity CreateEntity()
    {
        return InvalidEntity;
    }

    /// <summary>
    /// Creates an invalid named entity for safe no-op behavior.
    /// Returns a dead entity that won't be processed by systems.
    /// </summary>
    /// <param name="name">The name for the entity (ignored in null implementation)</param>
    /// <returns>An invalid Entity that systems should ignore.</returns>
    public Entity CreateEntity(string name)
    {
        return InvalidEntity;
    }

    /// <summary>
    /// No-op entity destruction. Entity is ignored.
    /// </summary>
    /// <param name="entity">The entity to destroy (ignored).</param>
    public void DestroyEntity(Entity entity)
    {
        // No-op: Entity destruction is ignored
    }

    /// <summary>
    /// No-op batch entity destruction. Entities are ignored.
    /// Educational note: Even null implementations should maintain consistent interface.
    /// </summary>
    /// <param name="entities">The entities to destroy (ignored).</param>
    public void DestroyEntities(IEnumerable<Entity> entities)
    {
        // No-op: Batch entity destruction is ignored
    }

    /// <summary>
    /// Returns empty enumerable as no entities exist.
    /// </summary>
    /// <returns>Empty enumerable.</returns>
    public IEnumerable<Entity> GetAllEntities()
    {
        return Enumerable.Empty<Entity>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPONENT MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// No-op component setting. Component is ignored.
    /// </summary>
    /// <typeparam name="T">The type of component to add.</typeparam>
    /// <param name="entity">The entity to add the component to (ignored).</param>
    /// <param name="component">The component instance to add (ignored).</param>
    public void SetComponent<T>(Entity entity, T component)
        where T : IComponent
    {
        // No-op: Component is not stored
    }

    /// <summary>
    /// Throws InvalidOperationException as no singleton components are available.
    /// This maintains the contract that GetSingleton should throw when no component exists.
    /// </summary>
    /// <typeparam name="T">The type of component to retrieve.</typeparam>
    /// <returns>Never returns - always throws.</returns>
    /// <exception cref="InvalidOperationException">Always thrown as no components are stored.</exception>
    public T GetSingleton<T>()
        where T : IComponent
    {
        throw new InvalidOperationException(
            $"No singleton component of type {typeof(T).Name} registered in NullWorld."
        );
    }

    /// <summary>
    /// Always returns false as no components are stored.
    /// </summary>
    /// <typeparam name="T">The type of component to remove.</typeparam>
    /// <param name="entity">The entity to remove the component from (ignored).</param>
    /// <returns>Always false as no components exist to remove.</returns>
    public bool RemoveComponent<T>(Entity entity)
        where T : IComponent
    {
        return false;
    }

    /// <summary>
    /// Always returns false as no components are stored.
    /// </summary>
    /// <typeparam name="T">The type of component to check for.</typeparam>
    /// <param name="entity">The entity to check (ignored).</param>
    /// <returns>Always false as no components exist.</returns>
    public bool HasComponent<T>(Entity entity)
        where T : IComponent
    {
        return false;
    }

    /// <summary>
    /// Always returns false as no components are stored.
    /// </summary>
    /// <typeparam name="T">The type of component to retrieve.</typeparam>
    /// <param name="entity">The entity to get the component from (ignored).</param>
    /// <param name="component">Always set to default value.</param>
    /// <returns>Always false as no components exist.</returns>
    public bool TryGetComponent<T>(Entity entity, out T component)
        where T : IComponent
    {
        component = default!;
        return false;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPONENT QUERIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns empty enumerable as no entities have components.
    /// </summary>
    /// <typeparam name="T1">The component type to query for.</typeparam>
    /// <returns>Empty enumerable.</returns>
    public IEnumerable<(Entity Entity, T1 Component1)> Query<T1>()
        where T1 : IComponent
    {
        return Enumerable.Empty<(Entity, T1)>();
    }

    /// <summary>
    /// Returns empty enumerable as no entities have components.
    /// </summary>
    /// <typeparam name="T1">First component type to query for.</typeparam>
    /// <typeparam name="T2">Second component type to query for.</typeparam>
    /// <returns>Empty enumerable.</returns>
    public IEnumerable<(Entity Entity, T1 Component1, T2 Component2)> Query<T1, T2>()
        where T1 : IComponent
        where T2 : IComponent
    {
        return Enumerable.Empty<(Entity, T1, T2)>();
    }

    /// <summary>
    /// Returns empty enumerable as no entities have components.
    /// </summary>
    /// <typeparam name="T1">First component type to query for.</typeparam>
    /// <typeparam name="T2">Second component type to query for.</typeparam>
    /// <typeparam name="T3">Third component type to query for.</typeparam>
    /// <returns>Empty enumerable.</returns>
    public IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3)> Query<T1, T2, T3>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    {
        return Enumerable.Empty<(Entity, T1, T2, T3)>();
    }

    /// <summary>
    /// Returns empty enumerable as no entities have components.
    /// </summary>
    /// <typeparam name="T1">First component type to query for.</typeparam>
    /// <typeparam name="T2">Second component type to query for.</typeparam>
    /// <typeparam name="T3">Third component type to query for.</typeparam>
    /// <typeparam name="T4">Fourth component type to query for.</typeparam>
    /// <returns>Empty enumerable.</returns>
    public IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3, T4 Component4)> Query<T1, T2, T3, T4>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
    {
        return Enumerable.Empty<(Entity, T1, T2, T3, T4)>();
    }

    /// <summary>
    /// Returns empty enumerable as no entities have components.
    /// </summary>
    /// <typeparam name="T1">First component type to query for.</typeparam>
    /// <typeparam name="T2">Second component type to query for.</typeparam>
    /// <typeparam name="T3">Third component type to query for.</typeparam>
    /// <typeparam name="T4">Fourth component type to query for.</typeparam>
    /// <typeparam name="T5">Fifth component type to query for.</typeparam>
    /// <returns>Empty enumerable.</returns>
    public IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3, T4 Component4, T5 Component5)> Query<T1, T2, T3, T4, T5>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
    {
        return Enumerable.Empty<(Entity, T1, T2, T3, T4, T5)>();
    }

    /// <summary>
    /// Creates a null query root that produces empty results.
    /// </summary>
    /// <returns>A NullQueryRoot that always returns empty results</returns>
    public IQueryRoot Query()
    {
        return new NullQueryRoot();
    }

    /// <summary>
    /// Returns a null query builder that produces empty results.
    /// </summary>
    /// <typeparam name="T">The primary component type to query for.</typeparam>
    /// <returns>A NullQueryBuilder that always returns empty results.</returns>
    public IQueryBuilder<T> QueryBuilder<T>()
        where T : IComponent
    {
        return new NullQueryBuilder<T>();
    }

    /// <summary>
    /// Always returns false as no components are stored.
    /// </summary>
    /// <typeparam name="T1">First component type to retrieve.</typeparam>
    /// <typeparam name="T2">Second component type to retrieve.</typeparam>
    /// <param name="entity">The entity to get components from (ignored).</param>
    /// <param name="component1">Always set to default value.</param>
    /// <param name="component2">Always set to default value.</param>
    /// <returns>Always false as no components exist.</returns>
    public bool TryGetComponents<T1, T2>(Entity entity, out T1 component1, out T2 component2)
        where T1 : IComponent
        where T2 : IComponent
    {
        component1 = default!;
        component2 = default!;
        return false;
    }

    /// <summary>
    /// Always returns false as no components are stored.
    /// </summary>
    /// <typeparam name="T1">First component type to retrieve.</typeparam>
    /// <typeparam name="T2">Second component type to retrieve.</typeparam>
    /// <typeparam name="T3">Third component type to retrieve.</typeparam>
    /// <param name="entity">The entity to get components from (ignored).</param>
    /// <param name="component1">Always set to default value.</param>
    /// <param name="component2">Always set to default value.</param>
    /// <param name="component3">Always set to default value.</param>
    /// <returns>Always false as no components exist.</returns>
    public bool TryGetComponents<T1, T2, T3>(Entity entity, out T1 component1, out T2 component2, out T3 component3)
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    {
        component1 = default!;
        component2 = default!;
        component3 = default!;
        return false;
    }

    /// <summary>
    /// Always returns false as no components are stored.
    /// </summary>
    /// <typeparam name="T1">First component type to retrieve.</typeparam>
    /// <typeparam name="T2">Second component type to retrieve.</typeparam>
    /// <typeparam name="T3">Third component type to retrieve.</typeparam>
    /// <typeparam name="T4">Fourth component type to retrieve.</typeparam>
    /// <param name="entity">The entity to get components from (ignored).</param>
    /// <param name="component1">Always set to default value.</param>
    /// <param name="component2">Always set to default value.</param>
    /// <param name="component3">Always set to default value.</param>
    /// <param name="component4">Always set to default value.</param>
    /// <returns>Always false as no components exist.</returns>
    public bool TryGetComponents<T1, T2, T3, T4>(Entity entity, out T1 component1, out T2 component2, out T3 component3, out T4 component4)
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
    {
        component1 = default!;
        component2 = default!;
        component3 = default!;
        component4 = default!;
        return false;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING AND DEVELOPMENT TOOLS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns empty dictionary as no components are stored in NullWorld.
    /// Provides safe no-op behavior for entity inspection.
    /// </summary>
    /// <param name="entity">The entity to inspect (ignored)</param>
    /// <returns>Empty dictionary representing no components</returns>
    public Dictionary<string, object> InspectEntity(Entity entity)
    {
        return new Dictionary<string, object>();
    }

    /// <summary>
    /// Returns generic entity identifier as no names are stored in NullWorld.
    /// Provides consistent behavior for entity naming.
    /// </summary>
    /// <param name="entity">The entity to get the name for</param>
    /// <returns>Generic entity identifier string</returns>
    public string GetEntityName(Entity entity)
    {
        return $"NullEntity #{entity.Id}";
    }

    /// <summary>
    /// Returns empty enumerable as no tags are stored in NullWorld.
    /// Provides safe no-op behavior for tag-based queries.
    /// </summary>
    /// <param name="tag">The tag to search for (ignored)</param>
    /// <returns>Empty enumerable representing no tagged entities</returns>
    public IEnumerable<Entity> GetEntitiesWithTag(string tag)
    {
        return Enumerable.Empty<Entity>();
    }
}