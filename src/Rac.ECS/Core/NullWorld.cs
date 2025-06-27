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
}