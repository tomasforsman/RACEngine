using Rac.ECS.Components;

namespace Rac.ECS.Core;

/// <summary>
/// Extension methods for IWorld to support advanced query operations.
/// 
/// These extensions provide utility methods that bridge the gap between
/// generic type-safe operations and runtime Type-based operations needed
/// for advanced query scenarios.
/// </summary>
/// <remarks>
/// Educational Pattern: Extension Methods for Interface Enhancement
/// 
/// Extension methods allow us to add functionality to existing interfaces
/// without modifying their definition. This is particularly useful for:
/// - Adding convenience methods that don't belong in the core interface
/// - Providing bridge methods between different type paradigms
/// - Maintaining backwards compatibility while extending functionality
/// 
/// In this case, we provide Type-based component checking that supports
/// the QueryBuilder's runtime filtering requirements while maintaining
/// the type safety of the main IWorld interface.
/// </remarks>
internal static class WorldExtensions
{
    /// <summary>
    /// Checks if an entity has a component of the specified type.
    /// This overload accepts a Type parameter for runtime type checking.
    /// </summary>
    /// <param name="world">The world instance to query</param>
    /// <param name="entity">The entity to check</param>
    /// <param name="componentType">The Type of component to check for</param>
    /// <returns>True if the entity has the component; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when world or componentType is null</exception>
    /// <exception cref="ArgumentException">Thrown when componentType does not implement IComponent</exception>
    /// <remarks>
    /// This method bridges the gap between compile-time generic type safety and
    /// runtime type flexibility needed for the QueryBuilder's filtering operations.
    /// 
    /// Performance Note: This method uses reflection internally and may be slower
    /// than the generic HasComponent&lt;T&gt; method. It should primarily be used
    /// in advanced query scenarios where runtime type determination is necessary.
    /// </remarks>
    internal static bool HasComponent(this IWorld world, Entity entity, Type componentType)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        if (componentType == null)
            throw new ArgumentNullException(nameof(componentType));
        if (!typeof(IComponent).IsAssignableFrom(componentType))
            throw new ArgumentException($"Type {componentType.Name} does not implement IComponent", nameof(componentType));

        // Use reflection to call the generic HasComponent<T> method
        var method = typeof(IWorld).GetMethod(nameof(IWorld.HasComponent));
        var genericMethod = method?.MakeGenericMethod(componentType);
        
        if (genericMethod == null)
            return false;
            
        try
        {
            return (bool)(genericMethod.Invoke(world, new object[] { entity }) ?? false);
        }
        catch
        {
            // If reflection fails for any reason, assume component doesn't exist
            return false;
        }
    }
}