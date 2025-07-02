using Rac.ECS.Components;

namespace Rac.ECS.Core;

/// <summary>
/// Interface for building advanced queries without initially specifying a primary component type.
/// 
/// This interface enables the syntax world.Query().With&lt;ComponentType&gt;() where the first
/// With&lt;T&gt;() call establishes the primary component type for the query.
/// </summary>
/// <remarks>
/// Educational Design Pattern: Progressive Interface Narrowing
/// 
/// This interface demonstrates how APIs can start broadly and narrow down to specific
/// types through method calls. The Query() method returns this interface, and the first
/// With&lt;T&gt;() call converts it to a typed IQueryBuilder&lt;T&gt;.
/// 
/// This pattern provides flexibility while maintaining type safety:
/// - Start: world.Query() (untyped)
/// - Progress: world.Query().With&lt;Position&gt;() (typed to IQueryBuilder&lt;Position&gt;)
/// - Continue: .With&lt;Velocity&gt;().Without&lt;Player&gt;() (fluent chaining)
/// </remarks>
public interface IQueryRoot
{
    /// <summary>
    /// Establishes the primary component type and begins building a typed query.
    /// This method converts the untyped query root into a typed query builder.
    /// </summary>
    /// <typeparam name="TPrimary">The primary component type to query for</typeparam>
    /// <returns>A typed QueryBuilder for fluent query construction</returns>
    /// <remarks>
    /// This method serves as the entry point for typed query building. Once called,
    /// the returned IQueryBuilder&lt;TPrimary&gt; provides strongly-typed query operations.
    /// </remarks>
    IQueryBuilder<TPrimary> With<TPrimary>()
        where TPrimary : IComponent;
}