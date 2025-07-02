using Rac.ECS.Components;

namespace Rac.ECS.Core;

/// <summary>
/// Concrete implementation of IQueryBuilder for building advanced ECS queries with inclusion and exclusion filters.
/// 
/// This class provides a fluent interface for constructing complex entity queries that go beyond
/// simple component matching. It supports both positive (With) and negative (Without) filters
/// to create sophisticated entity selection criteria.
/// </summary>
/// <typeparam name="TPrimary">The primary component type that drives the query</typeparam>
/// <remarks>
/// IMPLEMENTATION ARCHITECTURE:
/// 
/// The QueryBuilder uses a filter accumulation approach where:
/// 1. **Include Types**: Components that entities MUST have (AND relationship)
/// 2. **Exclude Types**: Components that entities MUST NOT have (OR relationship for exclusions)
/// 3. **Deferred Execution**: Query building and execution are separate phases
/// 4. **Performance Optimization**: Leverages existing World.Query optimizations
/// 
/// EDUCATIONAL ALGORITHMS:
/// 
/// Filter Processing Algorithm:
/// 1. Start with all entities that have the primary component type
/// 2. For each entity, check inclusion criteria (hasAllRequired)
/// 3. For each entity, check exclusion criteria (hasAnyExcluded)
/// 4. Yield only entities that pass both inclusion and exclusion tests
/// 
/// Performance Characteristics:
/// - Time Complexity: O(n * (i + e)) where n = entities with primary component, i = inclusion filters, e = exclusion filters
/// - Space Complexity: O(i + e) for storing filter type lists
/// - Memory Efficiency: Uses yield return for lazy evaluation
/// 
/// DESIGN PATTERNS DEMONSTRATED:
/// - **Builder Pattern**: Step-by-step query construction
/// - **Fluent Interface**: Method chaining for readability
/// - **Strategy Pattern**: Different filtering strategies (With/Without)
/// - **Iterator Pattern**: Lazy evaluation with yield return
/// </remarks>
internal sealed class QueryBuilder<TPrimary> : IQueryBuilder<TPrimary>
    where TPrimary : IComponent
{
    // ═══════════════════════════════════════════════════════════════════════════
    // INTERNAL STATE AND DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════
    
    private readonly IWorld _world;
    private readonly List<Type> _includeTypes = new();
    private readonly List<Type> _excludeTypes = new();

    /// <summary>
    /// Initializes a new QueryBuilder instance for the specified world.
    /// </summary>
    /// <param name="world">The ECS world instance to query against</param>
    /// <exception cref="ArgumentNullException">Thrown when world is null</exception>
    internal QueryBuilder(IWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FLUENT QUERY BUILDING METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Adds an inclusion filter requiring entities to have the specified component type.
    /// Creates an AND relationship with other inclusion filters.
    /// </summary>
    /// <typeparam name="TWith">The component type that entities must have</typeparam>
    /// <returns>The same QueryBuilder instance for method chaining</returns>
    /// <remarks>
    /// Educational Note: Method Chaining Pattern
    /// 
    /// Returning 'this' enables fluent syntax where multiple method calls can be chained:
    /// query.With&lt;ComponentA&gt;().With&lt;ComponentB&gt;().Without&lt;ComponentC&gt;()
    /// 
    /// This pattern improves code readability and follows the principle of making
    /// invalid states unrepresentable through compile-time type safety.
    /// </remarks>
    public IQueryBuilder<TPrimary> With<TWith>()
        where TWith : IComponent
    {
        var componentType = typeof(TWith);
        if (!_includeTypes.Contains(componentType))
        {
            _includeTypes.Add(componentType);
        }
        return this;
    }

    /// <summary>
    /// Adds an exclusion filter rejecting entities that have the specified component type.
    /// Creates an OR relationship with other exclusion filters (entity excluded if it has ANY excluded component).
    /// </summary>
    /// <typeparam name="TWithout">The component type that entities must NOT have</typeparam>
    /// <returns>The same QueryBuilder instance for method chaining</returns>
    /// <remarks>
    /// Educational Note: Exclusion Logic
    /// 
    /// Exclusion filters use OR logic because an entity should be excluded if it has
    /// ANY of the unwanted components. This is different from inclusion filters which
    /// use AND logic (entity must have ALL required components).
    /// 
    /// Example: Without&lt;DeadComponent&gt;().Without&lt;DisabledComponent&gt;()
    /// Excludes entities that are either dead OR disabled (or both).
    /// </remarks>
    public IQueryBuilder<TPrimary> Without<TWithout>()
        where TWithout : IComponent
    {
        var componentType = typeof(TWithout);
        if (!_excludeTypes.Contains(componentType))
        {
            _excludeTypes.Add(componentType);
        }
        return this;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY EXECUTION AND FILTERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Executes the built query and returns matching entities with their primary components.
    /// Uses optimized filtering algorithm with early exit conditions.
    /// </summary>
    /// <returns>Enumerable of entities and their primary components that match all filters</returns>
    /// <remarks>
    /// ALGORITHM IMPLEMENTATION:
    /// 
    /// 1. **Base Query**: Start with all entities that have the primary component
    /// 2. **Inclusion Test**: Verify entity has all required components (AND logic)
    /// 3. **Exclusion Test**: Verify entity has none of the excluded components (OR logic)
    /// 4. **Yield Results**: Return only entities that pass both tests
    /// 
    /// PERFORMANCE OPTIMIZATIONS:
    /// - Early exit: Stop checking as soon as any inclusion requirement fails
    /// - Early exit: Stop checking as soon as any exclusion requirement is met
    /// - Lazy evaluation: Uses yield return to process entities on-demand
    /// - Leverages existing World.Query optimization (smallest pool first)
    /// 
    /// EDUCATIONAL NOTES:
    /// - Demonstrates how complex queries can be built from simple primitives
    /// - Shows practical application of set theory (intersection and difference operations)
    /// - Illustrates the power of iterator patterns for memory-efficient data processing
    /// </remarks>
    public IEnumerable<(Entity Entity, TPrimary Component)> Execute()
    {
        foreach (var (entity, component) in _world.Query<TPrimary>())
        {
            // ───────────────────────────────────────────────────────────────────
            // INCLUSION FILTER: Entity must have ALL required components
            // ───────────────────────────────────────────────────────────────────
            
            bool hasAllRequired = true;
            foreach (var includeType in _includeTypes)
            {
                if (!_world.HasComponent(entity, includeType))
                {
                    hasAllRequired = false;
                    break; // Early exit optimization
                }
            }
            
            if (!hasAllRequired)
                continue;

            // ───────────────────────────────────────────────────────────────────
            // EXCLUSION FILTER: Entity must have NONE of the excluded components
            // ───────────────────────────────────────────────────────────────────
            
            bool hasAnyExcluded = false;
            foreach (var excludeType in _excludeTypes)
            {
                if (_world.HasComponent(entity, excludeType))
                {
                    hasAnyExcluded = true;
                    break; // Early exit optimization  
                }
            }
            
            if (hasAnyExcluded)
                continue;

            // Entity passed all filters - include in results
            yield return (entity, component);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INTERNAL UTILITY EXTENSIONS
    // ═══════════════════════════════════════════════════════════════════════════
    // 
    // These extension methods provide a type-safe way to check for components
    // using Type objects while maintaining the generic interface benefits.
}