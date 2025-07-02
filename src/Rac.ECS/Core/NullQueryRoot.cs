using Rac.ECS.Components;

namespace Rac.ECS.Core;

/// <summary>
/// Null Object Pattern implementation of IQueryRoot for testing and headless scenarios.
/// 
/// This implementation provides safe no-op behavior for all QueryRoot operations,
/// maintaining consistency with the Null Object Pattern used throughout the NullWorld.
/// </summary>
/// <remarks>
/// Educational Pattern: Null Object Pattern Consistency
/// 
/// The NullQueryRoot ensures that even the progressive type specification pattern
/// works correctly in null scenarios. This maintains the same fluent interface
/// behavior while providing predictable empty results.
/// </remarks>
internal sealed class NullQueryRoot : IQueryRoot
{
    /// <summary>
    /// Creates a null query builder that produces empty results.
    /// </summary>
    /// <typeparam name="TPrimary">The primary component type to query for (ignored)</typeparam>
    /// <returns>A NullQueryBuilder that always returns empty results</returns>
    public IQueryBuilder<TPrimary> With<TPrimary>()
        where TPrimary : IComponent
    {
        return new NullQueryBuilder<TPrimary>();
    }
}