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