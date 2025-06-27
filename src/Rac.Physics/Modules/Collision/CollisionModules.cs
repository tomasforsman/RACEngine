using Rac.Physics.Modules;
using Rac.Physics.Core;
using Silk.NET.Maths;

namespace Rac.Physics.Modules.Collision;

// ═══════════════════════════════════════════════════════════════
// COLLISION MODULES - WEEK 9-10 FOUNDATION
// Educational note: Simple collision detection for basic physics systems
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Simple AABB (Axis-Aligned Bounding Box) collision detection module.
/// Educational note: Fast collision detection using rectangular bounding boxes.
/// Academic reference: Real-Time Collision Detection (Christer Ericson).
/// Performance: O(n²) broad-phase, suitable for small to medium entity counts.
/// Limitations: Only supports box shapes, no rotation support in collision detection.
/// </summary>
public class SimpleAABBCollisionModule : ICollisionModule
{
    private IPhysicsWorld? _world;

    /// <summary>
    /// Module name for debugging and configuration.
    /// </summary>
    public string Name => "Simple AABB Collision";

    /// <summary>
    /// Initializes the AABB collision module.
    /// </summary>
    /// <param name="world">Physics world for accessing body information</param>
    public void Initialize(IPhysicsWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    /// <summary>
    /// Simple O(n²) broad-phase collision detection.
    /// Educational note: Tests every body against every other body.
    /// This is inefficient but simple to understand and implement.
    /// For better performance, spatial partitioning would be used (Week 11-12).
    /// </summary>
    /// <param name="bodies">All bodies in the physics simulation</param>
    /// <returns>Potential collision pairs for narrow-phase testing</returns>
    public IEnumerable<CollisionPair> BroadPhase(IReadOnlyList<IRigidBody> bodies)
    {
        var pairs = new List<CollisionPair>();

        // Check every body against every other body (O(n²) algorithm)
        for (int i = 0; i < bodies.Count; i++)
        {
            for (int j = i + 1; j < bodies.Count; j++)
            {
                var bodyA = bodies[i];
                var bodyB = bodies[j];

                // Skip if both bodies are static (they can't move relative to each other)
                if (bodyA.IsStatic && bodyB.IsStatic)
                    continue;

                // Get AABBs for both bodies
                var aabbA = bodyA.GetAABB();
                var aabbB = bodyB.GetAABB();

                // Quick AABB overlap test
                if (AABBsOverlap(aabbA, aabbB))
                {
                    pairs.Add(new CollisionPair(bodyA.Entity, bodyB.Entity));
                }
            }
        }

        return pairs;
    }

    /// <summary>
    /// AABB vs AABB narrow-phase collision detection.
    /// Educational note: Determines exact contact information for overlapping AABBs.
    /// Calculates penetration depth and collision normal for response.
    /// </summary>
    /// <param name="pair">Potential collision pair from broad-phase</param>
    /// <returns>Collision information if bodies are intersecting, null otherwise</returns>
    public CollisionInfo? NarrowPhase(CollisionPair pair)
    {
        if (_world == null)
            throw new InvalidOperationException("Module not initialized");

        var bodyA = _world.GetBody(pair.BodyA);
        var bodyB = _world.GetBody(pair.BodyB);

        if (bodyA == null || bodyB == null)
            return null;

        var aabbA = bodyA.GetAABB();
        var aabbB = bodyB.GetAABB();

        // Verify overlap (should be true from broad-phase, but double-check)
        if (!AABBsOverlap(aabbA, aabbB))
            return null;

        // Calculate penetration on each axis
        var penetrationX = Math.Min(aabbA.Max.X - aabbB.Min.X, aabbB.Max.X - aabbA.Min.X);
        var penetrationY = Math.Min(aabbA.Max.Y - aabbB.Min.Y, aabbB.Max.Y - aabbA.Min.Y);
        var penetrationZ = Math.Min(aabbA.Max.Z - aabbB.Min.Z, aabbB.Max.Z - aabbA.Min.Z);

        // Find axis of minimum penetration (separation direction)
        Vector3D<float> normal;
        float penetration;

        if (penetrationX <= penetrationY && penetrationX <= penetrationZ)
        {
            // Separate along X axis
            normal = aabbA.Center.X < aabbB.Center.X ? new Vector3D<float>(-1, 0, 0) : new Vector3D<float>(1, 0, 0);
            penetration = penetrationX;
        }
        else if (penetrationY <= penetrationZ)
        {
            // Separate along Y axis  
            normal = aabbA.Center.Y < aabbB.Center.Y ? new Vector3D<float>(0, -1, 0) : new Vector3D<float>(0, 1, 0);
            penetration = penetrationY;
        }
        else
        {
            // Separate along Z axis
            normal = aabbA.Center.Z < aabbB.Center.Z ? new Vector3D<float>(0, 0, -1) : new Vector3D<float>(0, 0, 1);
            penetration = penetrationZ;
        }

        // Contact point is approximately at the overlap center
        var contactPoint = (aabbA.Center + aabbB.Center) * 0.5f;

        return new CollisionInfo(pair.BodyA, pair.BodyB, contactPoint, normal, penetration);
    }

    /// <summary>
    /// Simple impulse-based collision response.
    /// Educational note: Implements conservation of momentum for collision resolution.
    /// Uses positional correction to prevent overlap and velocity correction for bounce.
    /// </summary>
    /// <param name="collisions">All confirmed collisions from narrow-phase</param>
    public void ResolveCollisions(IEnumerable<CollisionInfo> collisions)
    {
        if (_world == null)
            throw new InvalidOperationException("Module not initialized");

        foreach (var collision in collisions)
        {
            var bodyA = _world.GetBody(collision.BodyA);
            var bodyB = _world.GetBody(collision.BodyB);

            if (bodyA == null || bodyB == null)
                continue;

            ResolveCollision(bodyA, bodyB, collision);
        }
    }

    /// <summary>
    /// Simple raycast implementation for AABB shapes.
    /// Educational note: Ray-AABB intersection using slab method.
    /// Academic reference: Real-Time Rendering, ray-box intersection.
    /// </summary>
    /// <param name="origin">Ray starting point in world coordinates</param>
    /// <param name="direction">Ray direction (should be normalized)</param>
    /// <param name="maxDistance">Maximum ray distance to check</param>
    /// <param name="layers">Layer mask for filtering (simplified - not implemented in basic version)</param>
    /// <returns>Hit information if ray intersects an object, null otherwise</returns>
    public RaycastHit? Raycast(Vector3D<float> origin, Vector3D<float> direction, float maxDistance, LayerMask layers)
    {
        if (_world == null)
            throw new InvalidOperationException("Module not initialized");

        RaycastHit? closestHit = null;
        float closestDistance = maxDistance;

        var bodies = _world.GetAllBodies();
        foreach (var body in bodies)
        {
            var aabb = body.GetAABB();
            if (RayAABBIntersection(origin, direction, aabb, out var distance) && distance < closestDistance)
            {
                closestDistance = distance;
                var hitPoint = origin + direction * distance;
                var normal = CalculateAABBNormal(hitPoint, aabb);
                closestHit = new RaycastHit(body.Entity, hitPoint, normal, distance);
            }
        }

        return closestHit;
    }

    /// <summary>
    /// Disposes the collision module.
    /// </summary>
    public void Dispose()
    {
        _world = null;
    }

    // ───────────────────────────────────────────────────────────────
    // PRIVATE HELPER METHODS
    // Educational note: Implementation details for collision algorithms
    // ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Tests if two AABBs overlap on all axes.
    /// Educational note: Two AABBs overlap if they overlap on ALL three axes.
    /// If they don't overlap on ANY axis, they don't intersect.
    /// </summary>
    private static bool AABBsOverlap(Box3D<float> a, Box3D<float> b)
    {
        return a.Max.X >= b.Min.X && a.Min.X <= b.Max.X &&
               a.Max.Y >= b.Min.Y && a.Min.Y <= b.Max.Y &&
               a.Max.Z >= b.Min.Z && a.Min.Z <= b.Max.Z;
    }

    /// <summary>
    /// Resolves collision between two rigid bodies using impulse method.
    /// Educational note: Separates overlapping bodies and applies collision impulse.
    /// </summary>
    private static void ResolveCollision(IRigidBody bodyA, IRigidBody bodyB, CollisionInfo collision)
    {
        // Step 1: Positional correction to separate overlapping bodies
        var correction = collision.Normal * collision.Penetration * 0.5f;
        if (!bodyA.IsStatic) bodyA.Position += correction;
        if (!bodyB.IsStatic) bodyB.Position -= correction;

        // Step 2: Velocity correction for bounce/restitution
        var relativeVelocity = bodyB.Velocity - bodyA.Velocity;
        var velocityAlongNormal = Vector3D.Dot(relativeVelocity, collision.Normal);

        // Don't resolve if objects are already separating
        if (velocityAlongNormal > 0) return;

        // Calculate collision impulse magnitude
        var restitution = (bodyA.Restitution + bodyB.Restitution) * 0.5f;
        var impulseScalar = -(1 + restitution) * velocityAlongNormal;

        // Account for mass (lighter objects get more velocity change)
        var massA = bodyA.IsStatic ? float.PositiveInfinity : bodyA.Mass;
        var massB = bodyB.IsStatic ? float.PositiveInfinity : bodyB.Mass;
        
        if (float.IsInfinity(massA) && float.IsInfinity(massB))
            return; // Both static, no response needed

        if (float.IsInfinity(massA))
            impulseScalar /= 1 / massB;
        else if (float.IsInfinity(massB))
            impulseScalar /= 1 / massA;
        else
            impulseScalar /= (1 / massA) + (1 / massB);

        var impulse = collision.Normal * impulseScalar;

        // Apply impulse to bodies (F = dp/dt, impulse = dp)
        if (!bodyA.IsStatic) bodyA.Velocity -= impulse / bodyA.Mass;
        if (!bodyB.IsStatic) bodyB.Velocity += impulse / bodyB.Mass;
    }

    /// <summary>
    /// Ray-AABB intersection test using the slab method.
    /// Educational note: Tests ray intersection with each axis-aligned plane pair.
    /// </summary>
    private static bool RayAABBIntersection(Vector3D<float> origin, Vector3D<float> direction, Box3D<float> aabb, out float distance)
    {
        distance = 0;

        float tMin = 0;
        float tMax = float.PositiveInfinity;

        // Test intersection with each axis slab
        for (int i = 0; i < 3; i++)
        {
            float rayOrigin = i == 0 ? origin.X : (i == 1 ? origin.Y : origin.Z);
            float rayDirection = i == 0 ? direction.X : (i == 1 ? direction.Y : direction.Z);
            float boxMin = i == 0 ? aabb.Min.X : (i == 1 ? aabb.Min.Y : aabb.Min.Z);
            float boxMax = i == 0 ? aabb.Max.X : (i == 1 ? aabb.Max.Y : aabb.Max.Z);

            if (Math.Abs(rayDirection) < 1e-6f)
            {
                // Ray parallel to slab - check if ray origin is within slab
                if (rayOrigin < boxMin || rayOrigin > boxMax)
                    return false;
            }
            else
            {
                // Calculate intersection distances
                float invDir = 1.0f / rayDirection;
                float t1 = (boxMin - rayOrigin) * invDir;
                float t2 = (boxMax - rayOrigin) * invDir;

                if (t1 > t2) (t1, t2) = (t2, t1); // Swap if needed

                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);

                if (tMin > tMax)
                    return false; // No intersection
            }
        }

        distance = tMin;
        return tMin >= 0;
    }

    /// <summary>
    /// Calculates the surface normal at a point on an AABB.
    /// Educational note: Determines which face of the box was hit.
    /// </summary>
    private static Vector3D<float> CalculateAABBNormal(Vector3D<float> point, Box3D<float> aabb)
    {
        var center = aabb.Center;
        var extents = aabb.Size * 0.5f;

        // Find which face is closest to the hit point
        var distances = new[]
        {
            Math.Abs(point.X - (center.X - extents.X)), // Left face
            Math.Abs(point.X - (center.X + extents.X)), // Right face
            Math.Abs(point.Y - (center.Y - extents.Y)), // Bottom face
            Math.Abs(point.Y - (center.Y + extents.Y)), // Top face
            Math.Abs(point.Z - (center.Z - extents.Z)), // Back face
            Math.Abs(point.Z - (center.Z + extents.Z))  // Front face
        };

        var minIndex = Array.IndexOf(distances, distances.Min());

        return minIndex switch
        {
            0 => new Vector3D<float>(-1, 0, 0), // Left
            1 => new Vector3D<float>(1, 0, 0),  // Right
            2 => new Vector3D<float>(0, -1, 0), // Bottom
            3 => new Vector3D<float>(0, 1, 0),  // Top
            4 => new Vector3D<float>(0, 0, -1), // Back
            5 => new Vector3D<float>(0, 0, 1),  // Front
            _ => new Vector3D<float>(0, 1, 0)   // Default to up
        };
    }
}