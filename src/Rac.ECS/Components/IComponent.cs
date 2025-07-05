namespace Rac.ECS.Components;

/// <summary>
/// Defines the base interface for all component types in the Entity-Component-System architecture.
/// Components represent pure data containers that can be attached to entities.
/// </summary>
/// <remarks>
/// The IComponent interface serves as a marker interface for the ECS system, following the
/// composition over inheritance principle. Components should contain only data and no behavior.
/// All logic is handled by systems that operate on entities with specific component combinations.
/// 
/// ECS Architecture Principles:
/// - Components are data containers (no methods, only properties)
/// - Entities are lightweight ID containers that group components
/// - Systems contain all logic and operate on entities with specific component sets
/// 
/// This design pattern is widely used in game engines because it:
/// - Enables flexible composition of game object behaviors
/// - Improves cache efficiency by grouping similar data
/// - Supports data-oriented design for better performance
/// - Makes code more maintainable and testable
/// 
/// Implementation Guidelines:
/// - Implement as readonly record structs for immutability and performance
/// - Keep components small and focused on single concerns
/// - Avoid references to other components or entities within component data
/// - Use value types when possible to minimize garbage collection
/// </remarks>
/// <example>
/// <code>
/// // Example component implementation
/// public readonly record struct PositionComponent(Vector2D&lt;float&gt; Position) : IComponent;
/// 
/// public readonly record struct VelocityComponent(Vector2D&lt;float&gt; Velocity) : IComponent;
/// 
/// public readonly record struct HealthComponent(int Current, int Maximum) : IComponent;
/// 
/// // Usage in ECS system
/// var entity = world.CreateEntity();
/// world.SetComponent(entity, new PositionComponent(Vector2D&lt;float&gt;.Zero));
/// world.SetComponent(entity, new VelocityComponent(new Vector2D&lt;float&gt;(1.0f, 0.0f)));
/// </code>
/// </example>
public interface IComponent
{
    // Marker interface for ECS components - no members required
    // Components should be implemented as readonly record structs for performance and immutability
}
