using Rac.Physics.Core;
using Rac.Physics.Modules;
using Rac.ECS.Core;
using Silk.NET.Maths;

namespace Rac.Physics.Services;

// ═══════════════════════════════════════════════════════════════
// MODULAR PHYSICS SERVICE
// Educational note: Composes physics modules into complete physics system
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Modular physics service implementation that composes modules into complete physics system.
/// Educational note: Demonstrates composition over inheritance for flexible architecture.
/// This is the main implementation of IPhysicsService using the modular design.
/// </summary>
public class ModularPhysicsService : IPhysicsService
{
    private readonly IPhysicsWorld _world;
    private readonly IGravityModule _gravityModule;
    private readonly ICollisionModule _collisionModule;
    private readonly IFluidModule? _fluidModule;
    
    private bool _disposed;
    private int _nextBodyId = 1;

    /// <summary>
    /// Creates modular physics service with specified modules.
    /// Educational note: Dependency injection enables testing and module swapping.
    /// </summary>
    /// <param name="gravityModule">Gravity implementation (required)</param>
    /// <param name="collisionModule">Collision detection implementation (required)</param>
    /// <param name="fluidModule">Fluid dynamics implementation (optional)</param>
    public ModularPhysicsService(
        IGravityModule gravityModule,
        ICollisionModule collisionModule,
        IFluidModule? fluidModule = null)
    {
        _gravityModule = gravityModule ?? throw new ArgumentNullException(nameof(gravityModule));
        _collisionModule = collisionModule ?? throw new ArgumentNullException(nameof(collisionModule));
        _fluidModule = fluidModule; // Optional module

        _world = new PhysicsWorld();

        // Initialize all modules with world access
        _gravityModule.Initialize(_world);
        _collisionModule.Initialize(_world);
        _fluidModule?.Initialize(_world);
    }

    // ═══════════════════════════════════════════════════════════════
    // SIMPLE PHYSICS INTERFACE (from existing IPhysicsService)
    // Educational note: Bridge between simple API and modular implementation
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initialize the physics world (no-op for modular implementation).
    /// Educational note: Modules are initialized in constructor.
    /// </summary>
    public void Initialize()
    {
        // Educational note: Modules already initialized in constructor
        // This method maintained for compatibility with existing IPhysicsService
    }

    /// <summary>
    /// Step the physics simulation by the given time delta.
    /// Educational note: Executes physics pipeline with modular components.
    /// </summary>
    /// <param name="deltaTime">Time step in seconds (typically 1/60 for 60fps)</param>
    public void Update(float deltaTime)
    {
        ThrowIfDisposed();

        var bodies = _world.GetAllBodies();

        // Phase 1: Apply external forces (gravity, fluid effects)
        _gravityModule.ApplyGravity(bodies, deltaTime);
        _fluidModule?.ApplyFluidDrag(bodies, deltaTime);
        _fluidModule?.ApplyBuoyancy(bodies, deltaTime);

        // Phase 2: Integrate forces into velocities and positions
        IntegrateMotion(bodies, deltaTime);

        // Phase 3: Collision detection and response
        var collisionPairs = _collisionModule.BroadPhase(bodies);
        var collisions = collisionPairs
            .Select(pair => _collisionModule.NarrowPhase(pair))
            .Where(collision => collision.HasValue)
            .Select(collision => collision.Value);

        _collisionModule.ResolveCollisions(collisions);
    }

    /// <summary>
    /// Add a static box collider to the world.
    /// Educational note: Converts simple API to modular ECS-based system.
    /// </summary>
    public int AddStaticBox(float x, float y, float z, float width, float height, float depth)
    {
        ThrowIfDisposed();

        var bodyId = _nextBodyId++;
        var entity = new Entity(bodyId, true);
        
        var config = new RigidBodyConfig
        {
            Position = new Vector3D<float>(x, y, z),
            IsStatic = true,
            UseGravity = false // Static bodies don't need gravity
        };

        _world.AddBody(entity, config);
        return bodyId;
    }

    /// <summary>
    /// Add a dynamic sphere to the world.
    /// Educational note: Creates dynamic body that responds to forces.
    /// </summary>
    public int AddDynamicSphere(float x, float y, float z, float radius, float mass = 1.0f)
    {
        ThrowIfDisposed();

        var bodyId = _nextBodyId++;
        var entity = new Entity(bodyId, true);
        
        var config = new RigidBodyConfig
        {
            Position = new Vector3D<float>(x, y, z),
            Mass = mass,
            IsStatic = false,
            UseGravity = true
        };

        _world.AddBody(entity, config);
        return bodyId;
    }

    /// <summary>
    /// Remove a physics body from the world.
    /// </summary>
    public void RemoveBody(int bodyId)
    {
        ThrowIfDisposed();
        
        var entity = new Entity(bodyId, true);
        _world.RemoveBody(entity);
    }

    /// <summary>
    /// Set gravity for the physics world.
    /// Educational note: Only works with ConstantGravityModule.
    /// </summary>
    public void SetGravity(float x, float y, float z)
    {
        ThrowIfDisposed();

        // Educational note: This is a limitation of the simple API
        // More advanced gravity configuration would use the builder pattern
        if (_gravityModule is Modules.Gravity.ConstantGravityModule constantGravity)
        {
            constantGravity.SetGravity(new Vector3D<float>(x, y, z));
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // ADVANCED PHYSICS INTERFACE (from existing IPhysicsService)
    // Educational note: More detailed control over physics bodies
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Add a dynamic box with advanced properties.
    /// </summary>
    public int AddDynamicBox(float x, float y, float z, float width, float height, float depth,
        float mass, float friction = 0.5f, float restitution = 0.3f)
    {
        ThrowIfDisposed();

        var bodyId = _nextBodyId++;
        var entity = new Entity(bodyId, true);
        
        var config = new RigidBodyConfig
        {
            Position = new Vector3D<float>(x, y, z),
            Mass = mass,
            Friction = friction,
            Restitution = restitution,
            IsStatic = false,
            UseGravity = true
        };

        _world.AddBody(entity, config);
        return bodyId;
    }

    /// <summary>
    /// Add a capsule collider.
    /// Educational note: Basic implementation treats as box for simplicity.
    /// </summary>
    public int AddCapsule(float x, float y, float z, float radius, float height, float mass, bool isStatic = false)
    {
        ThrowIfDisposed();

        var bodyId = _nextBodyId++;
        var entity = new Entity(bodyId, true);
        
        var config = new RigidBodyConfig
        {
            Position = new Vector3D<float>(x, y, z),
            Mass = mass,
            IsStatic = isStatic,
            UseGravity = !isStatic
        };

        _world.AddBody(entity, config);
        return bodyId;
    }

    /// <summary>
    /// Apply force to a dynamic body.
    /// Educational note: Demonstrates force application in modular system.
    /// </summary>
    public void ApplyForce(int bodyId, float forceX, float forceY, float forceZ)
    {
        ThrowIfDisposed();

        var entity = new Entity(bodyId, true);
        var body = _world.GetBody(entity);
        body?.AddForce(new Vector3D<float>(forceX, forceY, forceZ));
    }

    /// <summary>
    /// Apply impulse to a dynamic body.
    /// Educational note: Impulse directly changes velocity.
    /// </summary>
    public void ApplyImpulse(int bodyId, float impulseX, float impulseY, float impulseZ)
    {
        ThrowIfDisposed();

        var entity = new Entity(bodyId, true);
        var body = _world.GetBody(entity);
        body?.AddImpulse(new Vector3D<float>(impulseX, impulseY, impulseZ));
    }

    /// <summary>
    /// Set body velocity directly.
    /// Educational note: Direct velocity manipulation bypasses physics integration.
    /// </summary>
    public void SetVelocity(int bodyId, float velocityX, float velocityY, float velocityZ)
    {
        ThrowIfDisposed();

        var entity = new Entity(bodyId, true);
        var body = _world.GetBody(entity);
        if (body != null)
        {
            body.Velocity = new Vector3D<float>(velocityX, velocityY, velocityZ);
        }
    }

    /// <summary>
    /// Get the position of a physics body.
    /// </summary>
    public void GetPosition(int bodyId, out float x, out float y, out float z)
    {
        ThrowIfDisposed();

        var entity = new Entity(bodyId, true);
        var body = _world.GetBody(entity);
        if (body != null)
        {
            x = body.Position.X;
            y = body.Position.Y;
            z = body.Position.Z;
        }
        else
        {
            x = y = z = 0.0f;
        }
    }

    /// <summary>
    /// Set the position of a physics body.
    /// </summary>
    public void SetPosition(int bodyId, float x, float y, float z)
    {
        ThrowIfDisposed();

        var entity = new Entity(bodyId, true);
        var body = _world.GetBody(entity);
        if (body != null)
        {
            body.Position = new Vector3D<float>(x, y, z);
        }
    }

    /// <summary>
    /// Get the rotation of a physics body (simplified - returns identity).
    /// Educational note: Basic implementation doesn't support rotation.
    /// </summary>
    public void GetRotation(int bodyId, out float x, out float y, out float z, out float w)
    {
        // Basic implementation doesn't support rotation
        x = y = z = 0.0f;
        w = 1.0f; // Identity quaternion
    }

    /// <summary>
    /// Set the rotation of a physics body (no-op in basic implementation).
    /// Educational note: Basic implementation doesn't support rotation.
    /// </summary>
    public void SetRotation(int bodyId, float x, float y, float z, float w)
    {
        // Basic implementation doesn't support rotation
        // Educational note: Full implementation would update body orientation
    }

    /// <summary>
    /// Perform raycast using the collision module.
    /// Educational note: Delegates to collision module for spatial queries.
    /// </summary>
    public bool Raycast(float fromX, float fromY, float fromZ, float toX, float toY, float toZ, out int hitBodyId)
    {
        ThrowIfDisposed();

        var origin = new Vector3D<float>(fromX, fromY, fromZ);
        var target = new Vector3D<float>(toX, toY, toZ);
        var direction = Vector3D.Normalize(target - origin);
        var maxDistance = (target - origin).Length;

        var hit = _collisionModule.Raycast(origin, direction, maxDistance, LayerMask.All);
        if (hit.HasValue)
        {
            hitBodyId = hit.Value.Entity.Id;
            return true;
        }

        hitBodyId = -1;
        return false;
    }

    /// <summary>
    /// Set collision filtering for a body (not implemented in basic version).
    /// Educational note: Advanced feature for collision layers.
    /// </summary>
    public void SetCollisionFilter(int bodyId, int collisionGroup, int collisionMask)
    {
        // Educational note: Collision filtering would be implemented with layer system
        // This is beyond the scope of Week 9-10 foundation
    }

    /// <summary>
    /// Shutdown and cleanup the physics world.
    /// </summary>
    public void Shutdown()
    {
        Dispose();
    }

    // ═══════════════════════════════════════════════════════════════
    // PHYSICS INTEGRATION
    // Educational note: Core physics simulation step
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Numerical integration of Newton's laws of motion.
    /// Educational note: Uses Euler integration for simplicity.
    /// More accurate methods (RK4, Verlet) could be added as modules in advanced versions.
    /// </summary>
    /// <param name="bodies">All rigid bodies to integrate</param>
    /// <param name="deltaTime">Time step for integration</param>
    private static void IntegrateMotion(IReadOnlyList<IRigidBody> bodies, float deltaTime)
    {
        foreach (var body in bodies)
        {
            // Skip static bodies (infinite mass)
            if (body.IsStatic) continue;

            // F = ma, therefore a = F / m
            var acceleration = body.AccumulatedForce / body.Mass;

            // Integrate velocity: v = v₀ + at (Euler integration)
            body.Velocity += acceleration * deltaTime;

            // Integrate position: x = x₀ + vt
            body.Position += body.Velocity * deltaTime;

            // Clear accumulated forces for next frame
            body.ClearForces();
        }
    }

    /// <summary>
    /// Throws ObjectDisposedException if the service has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ModularPhysicsService));
        }
    }

    /// <summary>
    /// Disposes the physics service and all modules.
    /// Educational note: Follows standard .NET disposal pattern.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _gravityModule?.Dispose();
        _collisionModule?.Dispose();
        _fluidModule?.Dispose();
        _world?.Dispose();

        _disposed = true;
    }
}