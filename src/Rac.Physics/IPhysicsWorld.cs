using Rac.Physics.Core;
using Rac.ECS.Core;
using Silk.NET.Maths;

namespace Rac.Physics;

/// <summary>
/// Represents a rigid body in the physics simulation.
/// Educational note: Rigid bodies have mass, velocity, and respond to forces.
/// This interface provides access to physics properties for modules.
/// </summary>
public interface IRigidBody
{
    /// <summary>
    /// Entity identifier linking this physics body to the ECS system.
    /// Educational note: Enables communication between physics and game logic.
    /// </summary>
    Entity Entity { get; }
    
    /// <summary>
    /// Current world-space position of the body's center of mass.
    /// Educational note: Updated by physics integration each frame.
    /// </summary>
    Vector3D<float> Position { get; set; }
    
    /// <summary>
    /// Current velocity vector in world coordinates.
    /// Educational note: Rate of change of position, affects momentum.
    /// </summary>
    Vector3D<float> Velocity { get; set; }
    
    /// <summary>
    /// Mass of the rigid body in kilograms.
    /// Educational note: Affects inertia (resistance to acceleration). F = ma.
    /// </summary>
    float Mass { get; }
    
    /// <summary>
    /// Whether this body is static (immovable) or dynamic.
    /// Educational note: Static bodies have infinite mass and don't integrate motion.
    /// </summary>
    bool IsStatic { get; }
    
    /// <summary>
    /// Whether this body should be affected by gravity modules.
    /// Educational note: Allows selective gravity (e.g., magic floating objects).
    /// </summary>
    bool UseGravity { get; }
    
    /// <summary>
    /// Coefficient of restitution controlling bounce behavior.
    /// Educational note: 0 = no bounce (inelastic), 1 = perfect bounce (elastic).
    /// </summary>
    float Restitution { get; }
    
    /// <summary>
    /// Friction coefficient for surface interactions.
    /// Educational note: Affects sliding resistance between surfaces.
    /// </summary>
    float Friction { get; }
    
    /// <summary>
    /// Accumulated force for this physics step.
    /// Educational note: Forces accumulate during frame, then integrate into velocity.
    /// </summary>
    Vector3D<float> AccumulatedForce { get; }
    
    /// <summary>
    /// Adds force to be applied during next physics integration.
    /// Educational note: Forces accumulate and are cleared after integration.
    /// </summary>
    /// <param name="force">Force vector in world coordinates</param>
    void AddForce(Vector3D<float> force);
    
    /// <summary>
    /// Adds impulse directly to velocity.
    /// Educational note: Impulse = change in momentum, affects velocity immediately.
    /// </summary>
    /// <param name="impulse">Impulse vector in world coordinates</param>
    void AddImpulse(Vector3D<float> impulse);
    
    /// <summary>
    /// Clears accumulated forces, typically called after integration.
    /// Educational note: Prevents forces from accumulating across frames.
    /// </summary>
    void ClearForces();
    
    /// <summary>
    /// Gets the axis-aligned bounding box for this body.
    /// Educational note: Used for broad-phase collision detection and spatial queries.
    /// </summary>
    /// <returns>AABB encompassing the body's shape</returns>
    Box3D<float> GetAABB();
}

/// <summary>
/// Physics world interface providing access to all physics bodies and simulation state.
/// Educational note: Centralized access point for physics modules to query simulation state.
/// This is the main interface already referenced by the existing IPhysicsService.
/// </summary>
public interface IPhysicsWorld : IDisposable
{
    /// <summary>
    /// Gets all rigid bodies currently in the physics simulation.
    /// Educational note: Used by modules to iterate over bodies for force application.
    /// </summary>
    /// <returns>Read-only collection of all active rigid bodies</returns>
    IReadOnlyList<IRigidBody> GetAllBodies();
    
    /// <summary>
    /// Gets a specific rigid body by entity identifier.
    /// Educational note: Enables targeted force application and queries.
    /// </summary>
    /// <param name="entity">Entity identifier</param>
    /// <returns>Rigid body associated with entity, or null if not found</returns>
    IRigidBody? GetBody(Entity entity);
    
    /// <summary>
    /// Adds a new rigid body to the physics simulation.
    /// Educational note: Called when entities with physics components are created.
    /// </summary>
    /// <param name="entity">Entity identifier</param>
    /// <param name="config">Configuration for the rigid body</param>
    void AddBody(Entity entity, RigidBodyConfig config);
    
    /// <summary>
    /// Removes a rigid body from the physics simulation.
    /// Educational note: Called when entities are destroyed or physics components removed.
    /// </summary>
    /// <param name="entity">Entity identifier</param>
    void RemoveBody(Entity entity);
}
