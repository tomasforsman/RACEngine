using Rac.Physics.Core;
using Rac.ECS.Core;
using Silk.NET.Maths;

namespace Rac.Physics.Core;

// ═══════════════════════════════════════════════════════════════
// RIGID BODY IMPLEMENTATION
// Educational note: Basic rigid body implementation for physics simulation
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Basic rigid body implementation for the physics simulation.
/// Educational note: Stores physical properties and handles force accumulation.
/// This represents a solid object that can't deform and responds to forces according to Newton's laws.
/// </summary>
internal class RigidBody : IRigidBody
{
    private Vector3D<float> _accumulatedForce;

    /// <summary>
    /// Creates a rigid body with the specified configuration.
    /// </summary>
    /// <param name="entity">Entity identifier for ECS integration</param>
    /// <param name="config">Physical properties and initial state</param>
    public RigidBody(Entity entity, RigidBodyConfig config)
    {
        Entity = entity;
        Position = config.Position;
        Velocity = config.Velocity;
        Mass = config.Mass;
        IsStatic = config.IsStatic;
        UseGravity = config.UseGravity;
        Restitution = config.Restitution;
        Friction = config.Friction;
        _accumulatedForce = Vector3D<float>.Zero;
    }

    /// <summary>
    /// Entity identifier linking this physics body to the ECS system.
    /// </summary>
    public Entity Entity { get; }

    /// <summary>
    /// Current world-space position of the body's center of mass.
    /// Educational note: Updated by physics integration each frame.
    /// </summary>
    public Vector3D<float> Position { get; set; }

    /// <summary>
    /// Current velocity vector in world coordinates.
    /// Educational note: Rate of change of position, affects momentum (p = mv).
    /// </summary>
    public Vector3D<float> Velocity { get; set; }

    /// <summary>
    /// Mass of the rigid body in kilograms.
    /// Educational note: Affects inertia (resistance to acceleration). F = ma.
    /// </summary>
    public float Mass { get; }

    /// <summary>
    /// Whether this body is static (immovable) or dynamic.
    /// Educational note: Static bodies have infinite mass and don't integrate motion.
    /// </summary>
    public bool IsStatic { get; }

    /// <summary>
    /// Whether this body should be affected by gravity modules.
    /// Educational note: Allows selective gravity application.
    /// </summary>
    public bool UseGravity { get; }

    /// <summary>
    /// Coefficient of restitution controlling bounce behavior.
    /// Educational note: 0 = no bounce (inelastic), 1 = perfect bounce (elastic).
    /// </summary>
    public float Restitution { get; }

    /// <summary>
    /// Friction coefficient for surface interactions.
    /// Educational note: Affects sliding resistance between surfaces.
    /// </summary>
    public float Friction { get; }

    /// <summary>
    /// Accumulated force for this physics step.
    /// Educational note: Forces accumulate during frame, then integrate into velocity.
    /// </summary>
    public Vector3D<float> AccumulatedForce => _accumulatedForce;

    /// <summary>
    /// Adds force to be applied during next physics integration.
    /// Educational note: Forces accumulate and are cleared after integration.
    /// </summary>
    /// <param name="force">Force vector in world coordinates (Newtons)</param>
    public void AddForce(Vector3D<float> force)
    {
        if (!IsStatic)
        {
            _accumulatedForce += force;
        }
        // Educational note: Static bodies ignore forces (infinite mass)
    }

    /// <summary>
    /// Adds impulse directly to velocity.
    /// Educational note: Impulse = change in momentum, affects velocity immediately.
    /// J = Δp = m*Δv, therefore Δv = J/m
    /// </summary>
    /// <param name="impulse">Impulse vector in world coordinates (Newton-seconds)</param>
    public void AddImpulse(Vector3D<float> impulse)
    {
        if (!IsStatic)
        {
            // Impulse directly changes velocity: Δv = J/m
            Velocity += impulse / Mass;
        }
        // Educational note: Static bodies ignore impulses (infinite mass)
    }

    /// <summary>
    /// Clears accumulated forces, typically called after integration.
    /// Educational note: Prevents forces from accumulating across frames.
    /// </summary>
    public void ClearForces()
    {
        _accumulatedForce = Vector3D<float>.Zero;
    }

    /// <summary>
    /// Gets the axis-aligned bounding box for this body.
    /// Educational note: Simple AABB approximation for basic collision detection.
    /// In a full implementation, this would be based on actual geometry.
    /// </summary>
    /// <returns>AABB encompassing the body's shape</returns>
    public Box3D<float> GetAABB()
    {
        // For basic implementation, assume unit cube around position
        // Educational note: Real implementation would use actual geometry
        var halfSize = new Vector3D<float>(0.5f, 0.5f, 0.5f);
        return new Box3D<float>(Position - halfSize, Position + halfSize);
    }
}