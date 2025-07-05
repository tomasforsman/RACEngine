namespace Rac.Physics;

/// <summary>
/// Physics service interface providing both simple and advanced physics functionality.
/// 
/// This interface abstracts physics simulation to support multiple backends (e.g., Bepu Physics, Bullet Physics).
/// Implementations follow the Service pattern with Null Object fallback for graceful degradation.
/// </summary>
/// <remarks>
/// Educational Notes:
/// - Physics simulation typically operates at fixed timesteps (60Hz) for deterministic behavior
/// - Real-time physics involves numerical integration of forces, velocities, and positions
/// - Collision detection uses spatial partitioning (octrees, broad-phase/narrow-phase) for performance
/// - Modern physics engines support multithreading and SIMD optimizations
/// 
/// Performance Characteristics:
/// - Collision detection: O(n log n) with spatial partitioning, O(n²) without
/// - Integration: O(n) for all dynamic bodies
/// - Constraint solving: O(n) to O(n³) depending on solver complexity
/// </remarks>
public interface IPhysicsService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CORE SIMULATION LIFECYCLE
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Initialize the physics world and prepare for simulation.
    /// </summary>
    /// <remarks>
    /// Sets up collision detection systems, memory pools, and simulation parameters.
    /// Must be called before any other physics operations.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when initialization fails due to system constraints</exception>
    void Initialize();
    
    /// <summary>
    /// Step the physics simulation forward by the specified time delta.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds. 
    /// Recommended: 1/60 (16.67ms) for stable simulation</param>
    /// <remarks>
    /// Educational Note: Physics timesteps should be consistent for stability.
    /// Variable timesteps can cause integration errors and simulation instability.
    /// Many engines use fixed timesteps with interpolation for smooth rendering.
    /// 
    /// Performance: O(n) for integration + O(n log n) for collision detection
    /// </remarks>
    void Update(float deltaTime);
    
    /// <summary>
    /// Shutdown and cleanup the physics world, releasing all resources.
    /// </summary>
    /// <remarks>
    /// Frees native memory, stops simulation threads, and cleans up collision data structures.
    /// Should be called when the physics system is no longer needed.
    /// </remarks>
    void Shutdown();

    // ═══════════════════════════════════════════════════════════════════════════
    // SIMPLE PHYSICS BODY CREATION
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Add a static box collider to the world for level geometry and obstacles.
    /// </summary>
    /// <param name="x">X position of the box center in world coordinates</param>
    /// <param name="y">Y position of the box center in world coordinates</param>
    /// <param name="z">Z position of the box center in world coordinates</param>
    /// <param name="width">Width of the box along X axis</param>
    /// <param name="height">Height of the box along Y axis</param>
    /// <param name="depth">Depth of the box along Z axis</param>
    /// <returns>Unique body ID for referencing this physics body in future operations</returns>
    /// <remarks>
    /// Educational Note: Static bodies have infinite mass and never move during simulation.
    /// They're computationally cheaper than dynamic bodies and ideal for level geometry.
    /// </remarks>
    int AddStaticBox(float x, float y, float z, float width, float height, float depth);
    
    /// <summary>
    /// Add a dynamic sphere to the world for balls, projectiles, and round objects.
    /// </summary>
    /// <param name="x">X position of the sphere center in world coordinates</param>
    /// <param name="y">Y position of the sphere center in world coordinates</param>
    /// <param name="z">Z position of the sphere center in world coordinates</param>
    /// <param name="radius">Radius of the sphere</param>
    /// <param name="mass">Mass of the sphere affecting acceleration (F = ma)</param>
    /// <returns>Unique body ID for referencing this physics body in future operations</returns>
    /// <remarks>
    /// Educational Note: Spheres have the simplest collision detection (distance test)
    /// and uniform inertia tensor. They're ideal for balls and particles.
    /// </remarks>
    int AddDynamicSphere(float x, float y, float z, float radius, float mass = 1.0f);
    
    /// <summary>
    /// Remove a physics body from the world and free its resources.
    /// </summary>
    /// <param name="bodyId">ID of the body to remove</param>
    /// <remarks>
    /// Invalidates the body ID and removes all associated collision data.
    /// References to this body ID become invalid after removal.
    /// </remarks>
    void RemoveBody(int bodyId);
    
    /// <summary>
    /// Set gravity acceleration for the physics world affecting all dynamic bodies.
    /// </summary>
    /// <param name="x">Gravity acceleration along X axis (typically 0)</param>
    /// <param name="y">Gravity acceleration along Y axis (typically -9.81 for Earth gravity)</param>
    /// <param name="z">Gravity acceleration along Z axis (typically 0)</param>
    /// <remarks>
    /// Educational Note: Earth's gravity is approximately 9.81 m/s² downward.
    /// Different coordinate systems use different "up" directions:
    /// - Y-up: gravity = (0, -9.81, 0)
    /// - Z-up: gravity = (0, 0, -9.81)
    /// </remarks>
    void SetGravity(float x, float y, float z);

    // ═══════════════════════════════════════════════════════════════════════════
    // ADVANCED PHYSICS BODY CREATION
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Add a dynamic box with advanced material properties for realistic object behavior.
    /// </summary>
    /// <param name="x">X position of the box center in world coordinates</param>
    /// <param name="y">Y position of the box center in world coordinates</param>
    /// <param name="z">Z position of the box center in world coordinates</param>
    /// <param name="width">Width of the box along X axis</param>
    /// <param name="height">Height of the box along Y axis</param>
    /// <param name="depth">Depth of the box along Z axis</param>
    /// <param name="mass">Mass of the box affecting acceleration and momentum</param>
    /// <param name="friction">Surface friction coefficient (0 = ice, 1 = rubber)</param>
    /// <param name="restitution">Bounciness coefficient (0 = clay, 1 = superball)</param>
    /// <returns>Unique body ID for referencing this physics body in future operations</returns>
    /// <remarks>
    /// Educational Note: Material properties significantly affect object behavior:
    /// - Friction: Coulomb friction model (F_friction ≤ μ * F_normal)
    /// - Restitution: Coefficient of restitution in collisions (v_separation = -e * v_approach)
    /// </remarks>
    int AddDynamicBox(float x, float y, float z, float width, float height, float depth, 
        float mass, float friction = 0.5f, float restitution = 0.3f);
    
    /// <summary>
    /// Add a capsule collider combining cylindrical body with hemispherical caps.
    /// </summary>
    /// <param name="x">X position of the capsule center in world coordinates</param>
    /// <param name="y">Y position of the capsule center in world coordinates</param>
    /// <param name="z">Z position of the capsule center in world coordinates</param>
    /// <param name="radius">Radius of the cylindrical section and hemispherical caps</param>
    /// <param name="height">Total height including both hemispherical caps</param>
    /// <param name="mass">Mass of the capsule</param>
    /// <param name="isStatic">True for static capsules, false for dynamic behavior</param>
    /// <returns>Unique body ID for referencing this physics body in future operations</returns>
    /// <remarks>
    /// Educational Note: Capsules are excellent for character controllers because:
    /// - Smooth collision response (no edge catching)
    /// - Stable upright orientation
    /// - Efficient collision detection with curved surfaces
    /// </remarks>
    int AddCapsule(float x, float y, float z, float radius, float height, float mass, bool isStatic = false);

    // ═══════════════════════════════════════════════════════════════════════════
    // FORCE AND MOTION CONTROL
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Apply a continuous force to a dynamic body following Newton's Second Law.
    /// </summary>
    /// <param name="bodyId">ID of the target body</param>
    /// <param name="forceX">Force magnitude along X axis in Newtons</param>
    /// <param name="forceY">Force magnitude along Y axis in Newtons</param>
    /// <param name="forceZ">Force magnitude along Z axis in Newtons</param>
    /// <remarks>
    /// Educational Note: Forces integrate over time to change velocity (F = ma).
    /// Applied each frame until removed, providing realistic acceleration.
    /// Use for: wind, thrust, magnetic fields, continuous applied forces.
    /// </remarks>
    void ApplyForce(int bodyId, float forceX, float forceY, float forceZ);
    
    /// <summary>
    /// Apply an instantaneous impulse to a dynamic body for immediate velocity changes.
    /// </summary>
    /// <param name="bodyId">ID of the target body</param>
    /// <param name="impulseX">Impulse magnitude along X axis in Newton-seconds</param>
    /// <param name="impulseY">Impulse magnitude along Y axis in Newton-seconds</param>
    /// <param name="impulseZ">Impulse magnitude along Z axis in Newton-seconds</param>
    /// <remarks>
    /// Educational Note: Impulses provide instant momentum change (J = Δp = m * Δv).
    /// Applied once per call, ideal for explosions, collisions, jumping.
    /// Use for: instant velocity changes, collision responses, jump mechanics.
    /// </remarks>
    void ApplyImpulse(int bodyId, float impulseX, float impulseY, float impulseZ);
    
    /// <summary>
    /// Set the velocity of a dynamic body directly, bypassing physics integration.
    /// </summary>
    /// <param name="bodyId">ID of the target body</param>
    /// <param name="velocityX">Velocity along X axis in meters per second</param>
    /// <param name="velocityY">Velocity along Y axis in meters per second</param>
    /// <param name="velocityZ">Velocity along Z axis in meters per second</param>
    /// <remarks>
    /// Educational Note: Direct velocity manipulation can break physics realism.
    /// Bypasses force integration and may violate energy conservation.
    /// Use sparingly for: teleportation, immediate speed changes, debugging.
    /// </remarks>
    void SetVelocity(int bodyId, float velocityX, float velocityY, float velocityZ);

    // ═══════════════════════════════════════════════════════════════════════════
    // POSITION AND ORIENTATION QUERIES
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Retrieve the current world position of a physics body.
    /// </summary>
    /// <param name="bodyId">ID of the target body</param>
    /// <param name="x">Output X position in world coordinates</param>
    /// <param name="y">Output Y position in world coordinates</param>
    /// <param name="z">Output Z position in world coordinates</param>
    void GetPosition(int bodyId, out float x, out float y, out float z);
    
    /// <summary>
    /// Set the world position of a physics body directly.
    /// </summary>
    /// <param name="bodyId">ID of the target body</param>
    /// <param name="x">New X position in world coordinates</param>
    /// <param name="y">New Y position in world coordinates</param>
    /// <param name="z">New Z position in world coordinates</param>
    /// <remarks>
    /// Educational Note: Direct position changes can cause tunneling effects
    /// where fast-moving objects pass through thin barriers between frames.
    /// Consider using velocity-based movement for continuous collision detection.
    /// </remarks>
    void SetPosition(int bodyId, float x, float y, float z);
    
    /// <summary>
    /// Retrieve the current orientation of a physics body as a quaternion.
    /// </summary>
    /// <param name="bodyId">ID of the target body</param>
    /// <param name="x">Output quaternion X component</param>
    /// <param name="y">Output quaternion Y component</param>
    /// <param name="z">Output quaternion Z component</param>
    /// <param name="w">Output quaternion W component</param>
    /// <remarks>
    /// Educational Note: Quaternions represent 3D rotations using 4 components.
    /// Benefits over Euler angles: no gimbal lock, smooth interpolation, compact representation.
    /// Identity quaternion (0,0,0,1) represents no rotation from the initial orientation.
    /// </remarks>
    void GetRotation(int bodyId, out float x, out float y, out float z, out float w);
    
    /// <summary>
    /// Set the orientation of a physics body from quaternion components.
    /// </summary>
    /// <param name="bodyId">ID of the target body</param>
    /// <param name="x">Quaternion X component</param>
    /// <param name="y">Quaternion Y component</param>
    /// <param name="z">Quaternion Z component</param>
    /// <param name="w">Quaternion W component</param>
    /// <remarks>
    /// Educational Note: Ensure quaternion is normalized (|q| = 1) for valid rotation.
    /// Non-normalized quaternions can introduce scaling artifacts.
    /// Use quaternion normalization: q_normalized = q / |q|
    /// </remarks>
    void SetRotation(int bodyId, float x, float y, float z, float w);

    // ═══════════════════════════════════════════════════════════════════════════
    // SPATIAL QUERIES AND COLLISION DETECTION
    // ═══════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Perform a raycast query to find the first intersection along a ray.
    /// </summary>
    /// <param name="fromX">Ray origin X coordinate in world space</param>
    /// <param name="fromY">Ray origin Y coordinate in world space</param>
    /// <param name="fromZ">Ray origin Z coordinate in world space</param>
    /// <param name="toX">Ray end point X coordinate in world space</param>
    /// <param name="toY">Ray end point Y coordinate in world space</param>
    /// <param name="toZ">Ray end point Z coordinate in world space</param>
    /// <param name="hitBodyId">ID of the first body hit by the ray, -1 if no hit</param>
    /// <returns>True if the ray intersected a physics body, false otherwise</returns>
    /// <remarks>
    /// Educational Note: Raycasting uses ray-object intersection algorithms:
    /// - Ray-sphere: quadratic equation solving (fast)
    /// - Ray-box: slab method with axis-aligned tests
    /// - Ray-triangle: Möller-Trumbore algorithm for mesh collision
    /// 
    /// Performance: O(log n) with spatial partitioning, O(n) without optimization.
    /// Common uses: line-of-sight, mouse picking, weapon collision, pathfinding.
    /// </remarks>
    bool Raycast(float fromX, float fromY, float fromZ, float toX, float toY, float toZ, out int hitBodyId);
    
    /// <summary>
    /// Set collision filtering for a physics body using bitmask groups.
    /// </summary>
    /// <param name="bodyId">ID of the target body</param>
    /// <param name="collisionGroup">Bitmask representing which collision groups this body belongs to</param>
    /// <param name="collisionMask">Bitmask defining which collision groups this body can interact with</param>
    /// <remarks>
    /// Educational Note: Collision filtering uses bitwise AND operations for efficiency:
    /// - Bodies collide if: (groupA &amp; maskB) != 0 AND (groupB &amp; maskA) != 0
    /// - Example groups: Player=1, Enemy=2, Environment=4, Powerup=8
    /// - Example: Player (group=1, mask=6) collides with Enemy+Environment but not other Players
    /// 
    /// This enables complex layer-based collision systems common in game engines.
    /// </remarks>
    void SetCollisionFilter(int bodyId, int collisionGroup, int collisionMask);
}