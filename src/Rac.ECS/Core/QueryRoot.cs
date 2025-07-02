using Rac.ECS.Components;

namespace Rac.ECS.Core;

/// <summary>
/// Concrete implementation of IQueryRoot for building queries that start without a primary component type.
/// 
/// This class enables the fluent syntax world.Query().With&lt;ComponentType&gt;() where the primary
/// component type is established by the first With&lt;T&gt;() call.
/// </summary>
/// <remarks>
/// Educational Pattern: Type Progression in Fluent Interfaces
/// 
/// This implementation demonstrates how fluent interfaces can evolve from untyped
/// to typed contexts. The QueryRoot starts as an untyped builder and transforms
/// into a typed QueryBuilder&lt;T&gt; when the first component type is specified.
/// 
/// Benefits of this approach:
/// - **Natural Syntax**: Matches the expected world.Query().With&lt;T&gt;() pattern
/// - **Type Safety**: Converts to strongly-typed builder after first component
/// - **Flexibility**: Allows queries to start without predetermined primary types
/// - **Consistency**: Maintains the same fluent interface throughout
/// </remarks>
internal sealed class QueryRoot : IQueryRoot
{
    // ═══════════════════════════════════════════════════════════════════════════
    // INTERNAL STATE AND DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════
    
    private readonly IWorld _world;

    /// <summary>
    /// Initializes a new QueryRoot instance for the specified world.
    /// </summary>
    /// <param name="world">The ECS world instance to query against</param>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    internal QueryRoot(IWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TYPE PROGRESSION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Establishes the primary component type and creates a typed query builder.
    /// This method converts the untyped query root into a typed query builder.
    /// </summary>
    /// <typeparam name="TPrimary">The primary component type to query for</typeparam>
    /// <returns>A typed QueryBuilder for fluent query construction</returns>
    /// <remarks>
    /// Educational Note: Interface Transformation Pattern
    /// 
    /// This method demonstrates how an interface can transform from one type to another
    /// while maintaining fluent chaining. The QueryRoot implements IQueryRoot, but the
    /// With&lt;T&gt;() method returns IQueryBuilder&lt;T&gt;, effectively changing the interface
    /// type mid-chain while preserving the fluent syntax.
    /// 
    /// Example transformation:
    /// world.Query()           // Returns IQueryRoot
    ///      .With&lt;Position&gt;()  // Returns IQueryBuilder&lt;Position&gt;
    ///      .Without&lt;Player&gt;() // Continues with IQueryBuilder&lt;Position&gt;
    /// </remarks>
    public IQueryBuilder<TPrimary> With<TPrimary>()
        where TPrimary : IComponent
    {
        // Create a new typed QueryBuilder and include the primary component
        return new QueryBuilder<TPrimary>(_world);
    }
}