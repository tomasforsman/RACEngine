using Rac.ECS.Components;

namespace Rac.ECS.Core;

/// <summary>
/// Interface for building advanced queries with inclusion and exclusion filters.
/// 
/// This interface enables fluent query syntax for complex entity filtering:
/// - Inclusion filters: .With&lt;ComponentType&gt;() to require additional components
/// - Exclusion filters: .Without&lt;ComponentType&gt;() to exclude entities with specific components
/// - Method chaining: Multiple filters can be combined fluently
/// - Deferred execution: Query is built first, executed on demand
/// </summary>
/// <typeparam name="TPrimary">The primary component type that drives the query</typeparam>
/// <remarks>
/// Educational Design Pattern: Builder Pattern with Fluent Interface
/// 
/// The QueryBuilder follows Martin Fowler's "Fluent Interface" pattern design principles,
/// providing an API that reads like natural language and guides developers toward correct usage.
/// 
/// Benefits of this approach:
/// - **Readability**: Query intent is clear from the method chain
/// - **Type Safety**: Generic constraints prevent invalid component access
/// - **Composability**: Multiple filters can be combined in any order
/// - **Performance**: Deferred execution allows for query optimization
/// - **Maintainability**: Complex queries are easier to understand and modify
/// 
/// Academic Reference: "Fluent Interface" by Martin Fowler
/// https://martinfowler.com/bliki/FluentInterface.html
/// </remarks>
/// <example>
/// <code>
/// // Example usage with fluent syntax
/// var playerEnemies = world.Query&lt;PositionComponent&gt;()
///     .With&lt;HealthComponent&gt;()
///     .With&lt;AIComponent&gt;()
///     .Without&lt;PlayerComponent&gt;()
///     .Without&lt;DeadComponent&gt;()
///     .Execute();
/// 
/// foreach (var (entity, position) in playerEnemies)
/// {
///     // Process living AI entities that are not players
/// }
/// </code>
/// </example>
public interface IQueryBuilder<TPrimary>
    where TPrimary : IComponent
{
    /// <summary>
    /// Adds an inclusion filter requiring entities to have the specified component type.
    /// Entities without this component will be excluded from results.
    /// </summary>
    /// <typeparam name="TWith">The component type that entities must have</typeparam>
    /// <returns>The same QueryBuilder instance for method chaining</returns>
    /// <remarks>
    /// Multiple With() calls create an AND relationship - entities must have ALL specified components.
    /// This method supports fluent chaining to build complex queries.
    /// </remarks>
    IQueryBuilder<TPrimary> With<TWith>()
        where TWith : IComponent;

    /// <summary>
    /// Adds an exclusion filter rejecting entities that have the specified component type.
    /// Entities with this component will be excluded from results.
    /// </summary>
    /// <typeparam name="TWithout">The component type that entities must NOT have</typeparam>
    /// <returns>The same QueryBuilder instance for method chaining</returns>
    /// <remarks>
    /// Multiple Without() calls create an OR relationship for exclusions - entities with ANY excluded component are filtered out.
    /// This method supports fluent chaining to build complex queries.
    /// </remarks>
    IQueryBuilder<TPrimary> Without<TWithout>()
        where TWithout : IComponent;

    /// <summary>
    /// Executes the built query and returns matching entities with their primary components.
    /// This is where the actual entity filtering and component retrieval occurs.
    /// </summary>
    /// <returns>Enumerable of entities and their primary components that match all filters</returns>
    /// <remarks>
    /// Performance Notes:
    /// - Uses lazy evaluation (yield return) for memory efficiency
    /// - Iterates through the smallest component pool first
    /// - O(n) complexity where n is the size of the smallest relevant component pool
    /// - Deferred execution allows for optimal query planning
    /// </remarks>
    IEnumerable<(Entity Entity, TPrimary Component)> Execute();
}