using Rac.ECS.Components;
using Silk.NET.Maths;

namespace Rac.Physics.Components;

// ═══════════════════════════════════════════════════════════════
// ECS PHYSICS COMPONENTS - WEEK 9-10 FOUNDATION
// Educational note: Physics data for Entity-Component-System integration
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Core rigid body component for physics simulation.
/// Educational note: Stores fundamental physical properties of game objects.
/// Following ECS principles, this contains only data - no behavior.
/// </summary>
/// <param name="Mass">Mass of the body in kilograms (affects F = ma)</param>
/// <param name="Velocity">Current velocity vector in world coordinates</param>
/// <param name="IsStatic">Whether the body is static (immovable) or dynamic</param>
/// <param name="UseGravity">Whether the body should be affected by gravity</param>
/// <param name="Restitution">Bounciness from 0 (no bounce) to 1 (perfect bounce)</param>
/// <param name="Friction">Surface friction coefficient for sliding resistance</param>
public readonly record struct RigidBodyComponent(
    float Mass = 1.0f,
    Vector3D<float> Velocity = default,
    bool IsStatic = false,
    bool UseGravity = true,
    float Restitution = 0.5f,
    float Friction = 0.5f
) : IComponent
{
    /// <summary>
    /// Creates a static rigid body component (immovable).
    /// Educational note: Static bodies have infinite mass and don't respond to forces.
    /// </summary>
    /// <returns>Static rigid body configuration</returns>
    public static RigidBodyComponent Static() => new(IsStatic: true, UseGravity: false);

    /// <summary>
    /// Creates a dynamic rigid body component with specified mass.
    /// Educational note: Dynamic bodies respond to forces and participate in physics simulation.
    /// </summary>
    /// <param name="mass">Mass in kilograms</param>
    /// <returns>Dynamic rigid body configuration</returns>
    public static RigidBodyComponent Dynamic(float mass = 1.0f) => new(Mass: mass, IsStatic: false);

    /// <summary>
    /// Creates a kinematic rigid body component (controlled by script, not physics).
    /// Educational note: Kinematic bodies don't respond to forces but can affect other bodies.
    /// </summary>
    /// <returns>Kinematic rigid body configuration</returns>
    public static RigidBodyComponent Kinematic() => new(IsStatic: true, UseGravity: false);
}

/// <summary>
/// Collision shape component defining physical boundaries.
/// Educational note: Separates collision geometry from visual representation.
/// This allows invisible collision boxes or visual effects without collision.
/// </summary>
/// <param name="Size">Dimensions of the collision shape (width, height, depth)</param>
/// <param name="Offset">Local offset from entity position</param>
/// <param name="IsTrigger">Whether this is a trigger (detects but doesn't collide)</param>
public readonly record struct ColliderComponent(
    Vector3D<float> Size = default,
    Vector3D<float> Offset = default,
    bool IsTrigger = false
) : IComponent
{
    /// <summary>
    /// Creates a unit cube collider (1x1x1).
    /// Educational note: Useful for basic game objects like blocks or simple characters.
    /// </summary>
    /// <returns>Unit cube collider configuration</returns>
    public static ColliderComponent UnitCube() => new(Size: Vector3D<float>.One);

    /// <summary>
    /// Creates a 2D box collider for top-down games.
    /// Educational note: Z dimension is minimal for 2D physics in 3D space.
    /// </summary>
    /// <param name="width">Width of the box</param>
    /// <param name="height">Height of the box</param>
    /// <returns>2D box collider configuration</returns>
    public static ColliderComponent Box2D(float width, float height) => 
        new(Size: new Vector3D<float>(width, height, 0.1f));

    /// <summary>
    /// Creates a 3D box collider.
    /// Educational note: Full 3D collision detection for three-dimensional games.
    /// </summary>
    /// <param name="width">Width of the box</param>
    /// <param name="height">Height of the box</param>
    /// <param name="depth">Depth of the box</param>
    /// <returns>3D box collider configuration</returns>
    public static ColliderComponent Box3D(float width, float height, float depth) => 
        new(Size: new Vector3D<float>(width, height, depth));

    /// <summary>
    /// Creates a trigger collider that detects but doesn't physically collide.
    /// Educational note: Useful for pickup items, area triggers, and sensors.
    /// </summary>
    /// <param name="size">Size of the trigger area</param>
    /// <returns>Trigger collider configuration</returns>
    public static ColliderComponent Trigger(Vector3D<float> size) => new(Size: size, IsTrigger: true);
}

/// <summary>
/// Physics material component for advanced surface properties.
/// Educational note: Controls how surfaces interact during collisions.
/// Based on real-world physics material properties.
/// </summary>
/// <param name="StaticFriction">Friction when objects are at rest</param>
/// <param name="DynamicFriction">Friction when objects are sliding</param>
/// <param name="Restitution">Bounciness of the material</param>
/// <param name="Density">Material density in kg/m³</param>
public readonly record struct PhysicsMaterialComponent(
    float StaticFriction = 0.6f,
    float DynamicFriction = 0.4f,
    float Restitution = 0.3f,
    float Density = 1000f
) : IComponent
{
    /// <summary>
    /// Rubber material properties - high friction, high bounce.
    /// Educational note: Real rubber has high friction and restitution.
    /// </summary>
    public static PhysicsMaterialComponent Rubber => new(
        StaticFriction: 1.2f,
        DynamicFriction: 1.0f,
        Restitution: 0.9f,
        Density: 920f
    );

    /// <summary>
    /// Ice material properties - very low friction, low bounce.
    /// Educational note: Ice is slippery due to extremely low friction.
    /// </summary>
    public static PhysicsMaterialComponent Ice => new(
        StaticFriction: 0.1f,
        DynamicFriction: 0.05f,
        Restitution: 0.1f,
        Density: 917f
    );

    /// <summary>
    /// Metal material properties - medium friction, low bounce.
    /// Educational note: Steel has moderate friction and low restitution.
    /// </summary>
    public static PhysicsMaterialComponent Metal => new(
        StaticFriction: 0.7f,
        DynamicFriction: 0.5f,
        Restitution: 0.2f,
        Density: 7850f
    );

    /// <summary>
    /// Wood material properties - medium friction and bounce.
    /// Educational note: Wood has moderate physical properties.
    /// </summary>
    public static PhysicsMaterialComponent Wood => new(
        StaticFriction: 0.5f,
        DynamicFriction: 0.3f,
        Restitution: 0.4f,
        Density: 600f
    );
}