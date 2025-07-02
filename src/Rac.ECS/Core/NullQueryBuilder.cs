using Rac.ECS.Components;

namespace Rac.ECS.Core;

/// <summary>
/// Null Object Pattern implementation of IQueryBuilder for testing and headless scenarios.
/// 
/// This implementation provides safe no-op behavior for all QueryBuilder operations,
/// following the Null Object Pattern to eliminate the need for null checks
/// and provide predictable behavior in testing scenarios.
/// </summary>
/// <typeparam name="TPrimary">The primary component type that would be queried for</typeparam>
/// <remarks>
/// Educational Design Pattern: The Null Object Pattern for Complex Interfaces
/// 
/// The NullQueryBuilder demonstrates how the Null Object Pattern scales to more
/// complex interfaces beyond simple method calls. Even sophisticated fluent
/// interfaces can have null implementations that maintain the same API surface
/// while providing safe, do-nothing behavior.
/// 
/// Benefits in this context:
/// - **Testing**: Query-dependent code can run without setting up World state
/// - **Consistency**: Same fluent interface works in null scenarios
/// - **Safety**: No null reference exceptions from QueryBuilder operations
/// - **Predictability**: Always returns empty results, never throws
/// </remarks>
internal sealed class NullQueryBuilder<TPrimary> : IQueryBuilder<TPrimary>
    where TPrimary : IComponent
{
    /// <summary>
    /// No-op inclusion filter. Filter is ignored in null implementation.
    /// </summary>
    /// <typeparam name="TWith">The component type that entities must have (ignored)</typeparam>
    /// <returns>The same NullQueryBuilder instance for method chaining</returns>
    public IQueryBuilder<TPrimary> With<TWith>()
        where TWith : IComponent
    {
        return this; // Method chaining continues to work
    }

    /// <summary>
    /// No-op exclusion filter. Filter is ignored in null implementation.
    /// </summary>
    /// <typeparam name="TWithout">The component type that entities must NOT have (ignored)</typeparam>
    /// <returns>The same NullQueryBuilder instance for method chaining</returns>
    public IQueryBuilder<TPrimary> Without<TWithout>()
        where TWithout : IComponent
    {
        return this; // Method chaining continues to work
    }

    /// <summary>
    /// Returns empty enumerable as no entities exist in null world.
    /// </summary>
    /// <returns>Empty enumerable of entities and components</returns>
    public IEnumerable<(Entity Entity, TPrimary Component)> Execute()
    {
        return Enumerable.Empty<(Entity, TPrimary)>();
    }
}