using Rac.Physics.Core;
using Silk.NET.Maths;
using Rac.ECS.Core;

namespace Rac.Physics.Modules;

// ═══════════════════════════════════════════════════════════════
// PHYSICS MODULE INTERFACES
// Educational note: Modular design allows composing custom physics systems
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Base interface for all physics modules.
/// Educational note: Modules implement single physics concepts for composability.
/// Following the Single Responsibility Principle, each module handles one aspect of physics.
/// </summary>
public interface IPhysicsModule : IDisposable
{
    /// <summary>
    /// Module initialization with access to physics world state.
    /// Called once when the physics service is created.
    /// </summary>
    /// <param name="world">Physics world for accessing bodies and state</param>
    void Initialize(IPhysicsWorld world);
    
    /// <summary>
    /// Module name for debugging and configuration display.
    /// Educational note: Helps identify which modules are active in complex systems.
    /// </summary>
    string Name { get; }
}

/// <summary>
/// Gravity module interface for different gravitational models.
/// Educational note: Demonstrates how same concept can have multiple implementations.
/// Examples: Constant gravity (games), N-body gravity (space sim), No gravity (top-down).
/// </summary>
public interface IGravityModule : IPhysicsModule
{
    /// <summary>
    /// Applies gravitational forces to all bodies in simulation.
    /// Educational note: Called each physics step before motion integration.
    /// </summary>
    /// <param name="bodies">All rigid bodies that can be affected by gravity</param>
    /// <param name="deltaTime">Time step for force integration (typically 1/60 second)</param>
    void ApplyGravity(IReadOnlyList<IRigidBody> bodies, float deltaTime);
}

/// <summary>
/// Collision detection module interface.
/// Educational note: Separates broad-phase and narrow-phase collision detection for performance.
/// Broad-phase finds potential pairs quickly, narrow-phase determines exact contact information.
/// </summary>
public interface ICollisionModule : IPhysicsModule
{
    /// <summary>
    /// Broad-phase collision detection to find potential collision pairs.
    /// Educational note: Spatial acceleration structure reduces O(n²) to O(n log n).
    /// Common approaches: Spatial hash, Octree, Sweep and Prune.
    /// </summary>
    /// <param name="bodies">All bodies in the physics simulation</param>
    /// <returns>Potential collision pairs for narrow-phase testing</returns>
    IEnumerable<CollisionPair> BroadPhase(IReadOnlyList<IRigidBody> bodies);
    
    /// <summary>
    /// Narrow-phase collision detection for exact contact information.
    /// Educational note: Precise geometric intersection testing between shapes.
    /// </summary>
    /// <param name="pair">Potential collision pair from broad-phase</param>
    /// <returns>Collision information if bodies are actually intersecting, null otherwise</returns>
    CollisionInfo? NarrowPhase(CollisionPair pair);
    
    /// <summary>
    /// Collision response and constraint resolution.
    /// Educational note: Implements conservation of momentum and energy.
    /// Uses impulse-based method for stable collision response.
    /// </summary>
    /// <param name="collisions">All confirmed collisions from narrow-phase</param>
    void ResolveCollisions(IEnumerable<CollisionInfo> collisions);
    
    /// <summary>
    /// Raycast implementation for spatial queries.
    /// Educational note: Essential for line-of-sight, bullet trajectories, mouse picking.
    /// </summary>
    /// <param name="origin">Ray starting point in world coordinates</param>
    /// <param name="direction">Ray direction (should be normalized)</param>
    /// <param name="maxDistance">Maximum ray distance to check</param>
    /// <param name="layers">Layer mask for filtering which objects can be hit</param>
    /// <returns>Hit information if ray intersects an object, null otherwise</returns>
    RaycastHit? Raycast(Vector3D<float> origin, Vector3D<float> direction, float maxDistance, LayerMask layers);
}

/// <summary>
/// Fluid dynamics module interface for drag and buoyancy effects.
/// Educational note: Models interaction between solid bodies and fluid environments.
/// Covers atmospheric drag, underwater physics, and other fluid-body interactions.
/// </summary>
public interface IFluidModule : IPhysicsModule
{
    /// <summary>
    /// Applies fluid drag forces to bodies moving through fluid.
    /// Educational note: F_drag = ½ρv²C_dA for quadratic drag, F_drag = bv for linear drag.
    /// Quadratic drag dominates at high speeds, linear drag at low speeds.
    /// </summary>
    /// <param name="bodies">All bodies that can experience fluid drag</param>
    /// <param name="deltaTime">Physics time step</param>
    void ApplyFluidDrag(IReadOnlyList<IRigidBody> bodies, float deltaTime);
    
    /// <summary>
    /// Applies buoyancy forces to submerged bodies.
    /// Educational note: F_buoyancy = ρ_fluid * V_displaced * g (Archimedes' principle).
    /// Displaced volume depends on how much of the object is submerged.
    /// </summary>
    /// <param name="bodies">All bodies that can experience buoyancy</param>
    /// <param name="deltaTime">Physics time step</param>
    void ApplyBuoyancy(IReadOnlyList<IRigidBody> bodies, float deltaTime);
}

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