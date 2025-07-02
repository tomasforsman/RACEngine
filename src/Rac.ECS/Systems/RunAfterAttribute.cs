namespace Rac.ECS.Systems;

/// <summary>
/// Specifies that a system should run after another system type.
/// Used to declare system dependencies for automatic scheduling order.
/// </summary>
/// <remarks>
/// This attribute enables declarative dependency management in the ECS system scheduler.
/// Systems marked with [RunAfter(typeof(InputSystem))] will automatically be scheduled
/// to run after the specified system type during each update cycle.
/// 
/// DEPENDENCY RESOLUTION ALGORITHM:
/// The SystemScheduler uses topological sorting to resolve system dependencies:
/// 1. Build a dependency graph from RunAfter attributes
/// 2. Perform topological sort to determine execution order
/// 3. Execute systems in dependency-resolved order
/// 4. Detect and report circular dependencies
/// 
/// EDUCATIONAL NOTES:
/// - Topological sorting is a classic algorithm used in build systems, task scheduling
/// - Dependency injection and ordering is common in enterprise frameworks
/// - This pattern enables loose coupling between systems while maintaining execution order
/// 
/// USAGE EXAMPLES:
/// - Movement systems should run after input systems
/// - Rendering systems should run after transform systems
/// - Physics systems should run after movement systems
/// - Cleanup systems should run after logic systems
/// </remarks>
/// <example>
/// <code>
/// // System that depends on InputSystem running first
/// [RunAfter(typeof(InputSystem))]
/// public class MovementSystem : ISystem
/// {
///     public void Update(float delta) { /* ... */ }
/// }
/// 
/// // System with multiple dependencies
/// [RunAfter(typeof(MovementSystem))]
/// [RunAfter(typeof(PhysicsSystem))]
/// public class RenderingSystem : ISystem
/// {
///     public void Update(float delta) { /* ... */ }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RunAfterAttribute : Attribute
{
    /// <summary>
    /// Gets the system type that this system should run after.
    /// </summary>
    public Type SystemType { get; }

    /// <summary>
    /// Initializes a new instance of the RunAfterAttribute.
    /// </summary>
    /// <param name="systemType">The system type that this system should run after.</param>
    /// <exception cref="ArgumentNullException">Thrown when systemType is null.</exception>
    /// <exception cref="ArgumentException">Thrown when systemType does not implement ISystem.</exception>
    public RunAfterAttribute(Type systemType)
    {
        if (systemType == null)
            throw new ArgumentNullException(nameof(systemType));
        
        if (!typeof(ISystem).IsAssignableFrom(systemType))
            throw new ArgumentException($"Type {systemType.Name} must implement ISystem interface.", nameof(systemType));
        
        SystemType = systemType;
    }
}