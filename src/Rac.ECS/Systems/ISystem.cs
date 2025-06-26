namespace Rac.ECS.Systems;

/// <summary>
/// Defines the interface for all systems in the Entity-Component-System architecture.
/// Systems contain the logic that operates on entities with specific component combinations.
/// </summary>
/// <remarks>
/// The ISystem interface establishes the contract for ECS systems, which are responsible
/// for all game logic and behavior. Systems follow these core principles:
/// 
/// - Systems are stateless and operate only on World data
/// - Systems query entities based on required component types
/// - Systems update component data but don't store state themselves
/// - Systems can be ordered and scheduled for optimal execution
/// 
/// ECS Update Loop:
/// 1. Systems query the World for entities with required components
/// 2. Systems iterate through matching entities and process their components
/// 3. Systems modify component data to reflect behavior changes
/// 4. The process repeats each frame with delta time for frame-rate independence
/// 
/// Common System Categories:
/// - Input Systems: Process user input and update input-related components
/// - Movement Systems: Update positions based on velocity and physics
/// - Rendering Systems: Submit drawable entities to the renderer
/// - Logic Systems: Implement game-specific behaviors and AI
/// - Cleanup Systems: Remove expired entities and components
/// 
/// Performance Considerations:
/// - Systems should minimize memory allocations during updates
/// - Use component queries efficiently to avoid unnecessary iterations
/// - Consider system execution order for dependencies (e.g., movement before rendering)
/// - Batch similar operations when possible for better cache performance
/// </remarks>
/// <example>
/// <code>
/// // Example system implementation
/// public class MovementSystem : ISystem
/// {
///     public void Update(float delta)
///     {
///         foreach (var entity in world.Query&lt;PositionComponent, VelocityComponent&gt;())
///         {
///             var (position, velocity) = world.GetComponents&lt;PositionComponent, VelocityComponent&gt;(entity);
///             var newPosition = new PositionComponent(position.Position + velocity.Velocity * delta);
///             world.SetComponent(entity, newPosition);
///         }
///     }
/// }
/// 
/// // System registration and execution
/// var movementSystem = new MovementSystem();
/// gameLoop.RegisterSystem(movementSystem);
/// 
/// // In game loop
/// movementSystem.Update(deltaTime);
/// </code>
/// </example>
public interface ISystem
{
    /// <summary>
    /// Updates the system logic for a single frame with the specified time delta.
    /// </summary>
    /// <param name="delta">
    /// The time elapsed since the last update in seconds.
    /// Used to make updates frame-rate independent by scaling movement and animation speeds.
    /// Typically ranges from 0.016 (60 FPS) to 0.033 (30 FPS) seconds.
    /// </param>
    /// <remarks>
    /// This method is called once per frame by the game loop or system scheduler.
    /// The delta parameter enables frame-rate independent updates by providing the
    /// actual time elapsed since the last frame.
    /// 
    /// Frame-Rate Independence:
    /// - Multiply movement speeds by delta to maintain consistent motion regardless of FPS
    /// - Scale animation progress by delta for smooth animations
    /// - Use delta for time-based logic like cooldowns and timers
    /// 
    /// Performance Guidelines:
    /// - Keep updates efficient as this method is called frequently (30-120+ times per second)
    /// - Minimize memory allocations to reduce garbage collection pressure
    /// - Process entities in batches when possible for better cache performance
    /// - Avoid expensive operations like file I/O or network calls in the update loop
    /// </remarks>
    /// <example>
    /// <code>
    /// public void Update(float delta)
    /// {
    ///     // Frame-rate independent movement
    ///     foreach (var entity in world.Query&lt;PositionComponent, VelocityComponent&gt;())
    ///     {
    ///         var (pos, vel) = world.GetComponents&lt;PositionComponent, VelocityComponent&gt;(entity);
    ///         
    ///         // Scale velocity by delta time for consistent movement speed
    ///         var newPosition = pos.Position + (vel.Velocity * delta);
    ///         world.SetComponent(entity, new PositionComponent(newPosition));
    ///     }
    /// }
    /// </code>
    /// </example>
    void Update(float delta);
}
