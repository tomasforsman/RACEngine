using Rac.ECS.Core;

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
    /// Initializes the system with access to the ECS world.
    /// Called once when the system is registered with the SystemScheduler.
    /// </summary>
    /// <param name="world">The ECS world for entity and component operations.</param>
    /// <remarks>
    /// This method is called during system registration, before any Update() calls.
    /// Use this method for:
    /// - Setting up system state and caches
    /// - Registering event handlers or callbacks
    /// - Creating global/singleton components
    /// - Validating system dependencies
    /// 
    /// SYSTEM LIFECYCLE:
    /// 1. Initialize() - Called once when system is added to scheduler
    /// 2. Update() - Called every frame by the scheduler
    /// 3. Shutdown() - Called once when system is removed or scheduler is cleared
    /// 
    /// EDUCATIONAL NOTES:
    /// - Initialization phases are common in system architectures
    /// - Separation of initialization from update logic improves maintainability
    /// - Access to IWorld enables systems to set up required components or state
    /// </remarks>
    /// <example>
    /// <code>
    /// public void Initialize(IWorld world)
    /// {
    ///     // Set up global configuration component
    ///     world.SetComponent(world.CreateEntity(), new PhysicsConfigComponent 
    ///     { 
    ///         Gravity = -9.81f 
    ///     });
    ///     
    ///     // Cache frequently used queries for performance
    ///     _movableEntitiesQuery = world.QueryBuilder&lt;VelocityComponent&gt;()
    ///         .With&lt;PositionComponent&gt;()
    ///         .Build();
    /// }
    /// </code>
    /// </example>
    void Initialize(IWorld world) { }

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

    /// <summary>
    /// Cleans up system resources and state before the system is removed.
    /// Called once when the system is unregistered from the SystemScheduler.
    /// </summary>
    /// <param name="world">The ECS world for final cleanup operations.</param>
    /// <remarks>
    /// This method is called during system removal or scheduler shutdown.
    /// Use this method for:
    /// - Disposing of system resources and caches
    /// - Unregistering event handlers or callbacks
    /// - Removing global/singleton components if needed
    /// - Performing final state cleanup
    /// 
    /// CLEANUP GUIDELINES:
    /// - Always dispose of managed resources (file handles, network connections)
    /// - Clear event subscriptions to prevent memory leaks
    /// - Remove temporary components or entities created during initialization
    /// - Log shutdown completion for debugging purposes
    /// 
    /// EDUCATIONAL NOTES:
    /// - Proper cleanup prevents resource leaks in long-running applications
    /// - Symmetric initialization/shutdown patterns improve system reliability
    /// - Cleanup phases enable graceful degradation during system changes
    /// </remarks>
    /// <example>
    /// <code>
    /// public void Shutdown(IWorld world)
    /// {
    ///     // Dispose of cached resources
    ///     _particleBuffer?.Dispose();
    ///     _renderTargets?.ForEach(rt => rt.Dispose());
    ///     
    ///     // Remove global components created during initialization
    ///     var configEntities = world.Query&lt;PhysicsConfigComponent&gt;().ToList();
    ///     world.DestroyEntities(configEntities.Select(e => e.Entity));
    ///     
    ///     // Log successful shutdown
    ///     Logger.Info($"{GetType().Name} shutdown completed successfully");
    /// }
    /// </code>
    /// </example>
    void Shutdown(IWorld world) { }
}
