using Silk.NET.Maths;
using Rac.ECS.Core;

namespace Rac.Physics.Core;

// ═══════════════════════════════════════════════════════════════
// CORE PHYSICS DATA TYPES
// Educational note: These types represent fundamental physics concepts
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Defines how forces are applied to rigid bodies in the physics simulation.
/// Educational note: Different force modes affect velocity and acceleration differently.
/// </summary>
public enum ForceMode
{
    /// <summary>
    /// Apply force continuously over time. F = ma, affects acceleration.
    /// Educational note: Force accumulates and is integrated each physics step.
    /// </summary>
    Force,
    
    /// <summary>
    /// Apply impulse instantaneously. Directly changes momentum.
    /// Educational note: Impulse = change in momentum, affects velocity immediately.
    /// </summary>
    Impulse,
    
    /// <summary>
    /// Apply acceleration directly, ignoring mass.
    /// Educational note: a = F/m, but here we specify acceleration directly.
    /// </summary>
    Acceleration,
    
    /// <summary>
    /// Apply velocity change directly, scaling by mass.
    /// Educational note: Similar to impulse but specified as velocity change.
    /// </summary>
    VelocityChange
}

/// <summary>
/// Defines collision detection and response algorithms.
/// Educational note: Different collision types trade accuracy for performance.
/// </summary>
public enum CollisionType
{
    /// <summary>
    /// Axis-Aligned Bounding Box collision detection.
    /// Educational note: Fast but limited to box shapes aligned with coordinate axes.
    /// </summary>
    AABB,
    
    /// <summary>
    /// External Bepu Physics engine integration.
    /// Educational note: High-performance 3D physics with complex shapes.
    /// </summary>
    Bepu,
    
    /// <summary>
    /// Custom collision implementation.
    /// Educational note: Allows specialized collision algorithms for specific games.
    /// </summary>
    Custom
}

/// <summary>
/// Defines gravity models for physics simulation.
/// Educational note: Different gravity types simulate different physical environments.
/// </summary>
public enum GravityType
{
    /// <summary>
    /// No gravitational forces applied.
    /// Educational note: Suitable for space games or top-down games.
    /// </summary>
    None,
    
    /// <summary>
    /// Constant gravitational acceleration (e.g., Earth's surface gravity).
    /// Educational note: g = 9.81 m/s² downward, uniform field approximation.
    /// </summary>
    Constant,
    
    /// <summary>
    /// Realistic N-body gravitational physics using Newton's law.
    /// Educational note: F = G*m1*m2/r², computationally expensive but accurate.
    /// </summary>
    Realistic,
    
    /// <summary>
    /// Planetary gravity with distance falloff from a center point.
    /// Educational note: Radial gravity field, suitable for planet surface games.
    /// </summary>
    Planetary
}

/// <summary>
/// Defines fluid interaction models for drag and buoyancy effects.
/// Educational note: Models how objects move through different fluid environments.
/// </summary>
public enum FluidType
{
    /// <summary>
    /// No fluid interactions.
    /// Educational note: Objects move in vacuum with no resistance.
    /// </summary>
    None,
    
    /// <summary>
    /// Linear drag model: F_drag = -b*v.
    /// Educational note: Stokes' law for low Reynolds number flows.
    /// </summary>
    LinearDrag,
    
    /// <summary>
    /// Quadratic drag model: F_drag = -½ρv²C_dA.
    /// Educational note: More realistic at higher velocities, like air resistance.
    /// </summary>
    QuadraticDrag,
    
    /// <summary>
    /// Water environment with buoyancy and viscous drag.
    /// Educational note: Includes Archimedes' principle and fluid viscosity.
    /// </summary>
    Water,
    
    /// <summary>
    /// Air environment with atmospheric drag.
    /// Educational note: Standard air density and drag coefficients.
    /// </summary>
    Air
}

/// <summary>
/// Configuration for rigid body creation and properties.
/// Educational note: Encapsulates all physical properties needed for rigid body simulation.
/// </summary>
public record struct RigidBodyConfig
{
    /// <summary>
    /// Mass of the rigid body in kilograms.
    /// Educational note: Mass affects inertia and response to forces (F = ma).
    /// </summary>
    public float Mass { get; init; }
    
    /// <summary>
    /// Initial position in world coordinates.
    /// Educational note: Starting location for physics simulation.
    /// </summary>
    public Vector3D<float> Position { get; init; }
    
    /// <summary>
    /// Initial velocity in world coordinates.
    /// Educational note: Rate of position change, affects momentum.
    /// </summary>
    public Vector3D<float> Velocity { get; init; }
    
    /// <summary>
    /// Whether the body is static (immovable) or dynamic (responds to forces).
    /// Educational note: Static bodies have infinite mass and don't integrate motion.
    /// </summary>
    public bool IsStatic { get; init; }
    
    /// <summary>
    /// Whether the body is affected by gravitational forces.
    /// Educational note: Allows selective gravity application (e.g., floating objects).
    /// </summary>
    public bool UseGravity { get; init; }
    
    /// <summary>
    /// Coefficient of restitution (bounciness) from 0 (no bounce) to 1 (perfect bounce).
    /// Educational note: Controls energy conservation in collisions.
    /// </summary>
    public float Restitution { get; init; }
    
    /// <summary>
    /// Surface friction coefficient affecting sliding resistance.
    /// Educational note: μ in F_friction = μ * N, affects contact forces.
    /// </summary>
    public float Friction { get; init; }
    
    /// <summary>
    /// Creates a RigidBodyConfig with default values.
    /// </summary>
    public RigidBodyConfig()
    {
        Mass = 1.0f;
        Position = Vector3D<float>.Zero;
        Velocity = Vector3D<float>.Zero;
        IsStatic = false;
        UseGravity = true;
        Restitution = 0.5f;
        Friction = 0.5f;
    }
}

/// <summary>
/// Contains information about a collision between two physics bodies.
/// Educational note: Provides data needed for collision response calculations.
/// </summary>
public readonly record struct CollisionInfo(
    Entity BodyA,
    Entity BodyB,
    Vector3D<float> ContactPoint,
    Vector3D<float> Normal,
    float Penetration
)
{
    /// <summary>
    /// Entity identifier for the first colliding body.
    /// </summary>
    public Entity BodyA { get; } = BodyA;
    
    /// <summary>
    /// Entity identifier for the second colliding body.
    /// </summary>
    public Entity BodyB { get; } = BodyB;
    
    /// <summary>
    /// World-space contact point where collision occurred.
    /// Educational note: Used for applying impulses and calculating torque.
    /// </summary>
    public Vector3D<float> ContactPoint { get; } = ContactPoint;
    
    /// <summary>
    /// Collision normal vector pointing from BodyA toward BodyB.
    /// Educational note: Defines the separation direction for collision response.
    /// </summary>
    public Vector3D<float> Normal { get; } = Normal;
    
    /// <summary>
    /// Penetration depth of the collision.
    /// Educational note: How far objects overlap, used for separation impulse.
    /// </summary>
    public float Penetration { get; } = Penetration;
}

/// <summary>
/// Represents a potential collision pair from broad-phase detection.
/// Educational note: Broad-phase finds potential collisions, narrow-phase confirms them.
/// </summary>
public readonly record struct CollisionPair(Entity BodyA, Entity BodyB);

/// <summary>
/// Contains raycast hit information for spatial queries.
/// Educational note: Essential for line-of-sight, bullet trajectories, and mouse picking.
/// </summary>
public readonly record struct RaycastHit(
    Entity Entity,
    Vector3D<float> Point,
    Vector3D<float> Normal,
    float Distance
)
{
    /// <summary>
    /// Entity that was hit by the raycast.
    /// </summary>
    public Entity Entity { get; } = Entity;
    
    /// <summary>
    /// World-space point where the ray intersected the entity.
    /// Educational note: Exact contact point for hit effects and calculations.
    /// </summary>
    public Vector3D<float> Point { get; } = Point;
    
    /// <summary>
    /// Surface normal at the hit point.
    /// Educational note: Used for reflection calculations and surface alignment.
    /// </summary>
    public Vector3D<float> Normal { get; } = Normal;
    
    /// <summary>
    /// Distance from ray origin to hit point.
    /// Educational note: Used for sorting multiple hits and range checking.
    /// </summary>
    public float Distance { get; } = Distance;
}

/// <summary>
/// Physics layer system for collision filtering.
/// Educational note: Enables selective collision (e.g., bullets don't hit other bullets).
/// </summary>
public readonly record struct LayerMask(uint Mask = 0xFFFFFFFF)
{
    /// <summary>
    /// Bitmask representing which layers are included.
    /// Educational note: Each bit represents a layer, allows efficient filtering.
    /// </summary>
    public uint Mask { get; } = Mask;
    
    /// <summary>
    /// Default layer mask that includes all layers.
    /// </summary>
    public static LayerMask All => new(0xFFFFFFFF);
    
    /// <summary>
    /// Layer mask that includes no layers.
    /// </summary>
    public static LayerMask None => new(0);
    
    /// <summary>
    /// Checks if the given layer is included in this mask.
    /// </summary>
    /// <param name="layer">Layer index (0-31)</param>
    /// <returns>True if layer is included in the mask</returns>
    public bool Contains(int layer) => (Mask & (1u << layer)) != 0;
    
    /// <summary>
    /// Creates a layer mask for a single layer.
    /// </summary>
    /// <param name="layer">Layer index (0-31)</param>
    /// <returns>Layer mask containing only the specified layer</returns>
    public static LayerMask Single(int layer) => new(1u << layer);
}