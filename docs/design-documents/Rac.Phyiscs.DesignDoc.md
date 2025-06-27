---
title: "Rac.Physics Modular Design Document"
description: "Comprehensive design for modular physics system with composable modules and educational focus"
version: "1.0.0"
last_updated: "2025-06-27"
author: "RACEngine Team"
tags: ["physics", "modular", "design", "architecture"]
---

# Rac.Physics Modular Design Document

## Overview

Rac.Physics implements a revolutionary modular physics architecture that allows developers to compose custom physics solutions by selecting only the components they need. This design enables everything from simple 2D collision detection to sophisticated N-body gravitational simulations while maintaining educational clarity and optimal performance.

## Prerequisites

- Understanding of basic physics concepts (forces, collisions, gravity)
- Familiarity with RACEngine service patterns and ECS architecture
- Knowledge of [System Overview](../architecture/system-overview.md) for integration context

## Design Philosophy

### Core Principles

**🧩 Composability over Complexity**
- Developers build physics engines by selecting modules, not learning monolithic APIs
- Each module handles one physics concept with clear, focused responsibility
- Modules combine seamlessly without unexpected interactions

**📚 Educational Transparency**
- Each module documents the physics concepts it implements with academic references
- Multiple implementations of the same concept (simple vs realistic) enable progressive learning
- Clear performance trade-offs help developers make informed decisions

**⚡ Performance by Design**
- Pay-only-for-what-you-use architecture eliminates overhead from unused features
- Hot path optimization through compile-time module composition
- Zero-cost abstractions where possible, measured overhead where necessary

**🔬 Scientific Accuracy**
- Realistic modules implement actual physics equations from academic sources
- Educational modules demonstrate real physics concepts, not just game mechanics
- Reference implementations validate against known physics simulations

## Architecture Overview

### Service Interface Hierarchy

```csharp
/// <summary>
/// Core physics service interface following RACEngine service patterns
/// Educational note: Provides unified access to modular physics components
/// </summary>
public interface IPhysicsService : IDisposable
{
    // ═══════════════════════════════════════════════════════════════
    // CORE SIMULATION INTERFACE
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Advances physics simulation by one time step
    /// Educational note: Fixed timestep ensures deterministic physics
    /// </summary>
    /// <param name="deltaTime">Time step in seconds (typically 1/60 for 60fps)</param>
    void Step(float deltaTime);
    
    /// <summary>
    /// Adds rigid body to physics simulation
    /// </summary>
    /// <param name="entity">ECS entity to associate with physics body</param>
    /// <param name="config">Body configuration (mass, type, initial velocity)</param>
    void AddRigidBody(Entity entity, RigidBodyConfig config);
    
    /// <summary>
    /// Removes rigid body from physics simulation
    /// </summary>
    /// <param name="entity">Entity to remove from simulation</param>
    void RemoveRigidBody(Entity entity);
    
    // ═══════════════════════════════════════════════════════════════
    // COLLISION AND SPATIAL QUERIES
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Performs raycast query for line-of-sight and bullet collision
    /// Educational note: Essential for shooter games and AI line-of-sight
    /// </summary>
    RaycastHit? Raycast(Vector3 origin, Vector3 direction, float maxDistance, LayerMask layers = default);
    
    /// <summary>
    /// Queries all rigid bodies within spherical area
    /// Educational note: Useful for explosion effects and proximity detection
    /// </summary>
    IEnumerable<Entity> QuerySphere(Vector3 center, float radius, LayerMask layers = default);
    
    /// <summary>
    /// Queries all rigid bodies within axis-aligned bounding box
    /// Educational note: Efficient for rectangular selection and area effects
    /// </summary>
    IEnumerable<Entity> QueryAABB(Vector3 min, Vector3 max, LayerMask layers = default);
    
    // ═══════════════════════════════════════════════════════════════
    // FORCE APPLICATION
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Applies instantaneous force to rigid body
    /// Educational note: F = ma, force affects acceleration for one frame
    /// </summary>
    void AddForce(Entity entity, Vector3 force, ForceMode mode = ForceMode.Force);
    
    /// <summary>
    /// Applies instantaneous impulse to rigid body
    /// Educational note: Impulse = change in momentum, affects velocity directly
    /// </summary>
    void AddImpulse(Entity entity, Vector3 impulse, Vector3 point = default);
    
    // ═══════════════════════════════════════════════════════════════
    // MODULE CONFIGURATION
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Gets configuration of currently active physics modules
    /// </summary>
    PhysicsConfiguration Configuration { get; }
    
    /// <summary>
    /// Runtime module management for advanced scenarios
    /// Educational note: Allows switching physics behavior during gameplay
    /// </summary>
    void SetModule<T>(T module) where T : class, IPhysicsModule;
    T GetModule<T>() where T : class, IPhysicsModule;
}
```

### Module Interface Design

```csharp
/// <summary>
/// Base interface for all physics modules
/// Educational note: Modules implement single physics concepts for composability
/// </summary>
public interface IPhysicsModule
{
    /// <summary>
    /// Module initialization with access to physics world state
    /// </summary>
    void Initialize(IPhysicsWorld world);
    
    /// <summary>
    /// Module cleanup and resource disposal
    /// </summary>
    void Dispose();
    
    /// <summary>
    /// Module name for debugging and configuration
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Performance characteristics for profiling
    /// </summary>
    ModulePerformanceInfo PerformanceInfo { get; }
}

/// <summary>
/// Gravity module interface for different gravitational models
/// Educational note: Demonstrates how same concept can have multiple implementations
/// </summary>
public interface IGravityModule : IPhysicsModule
{
    /// <summary>
    /// Applies gravitational forces to all bodies in simulation
    /// </summary>
    /// <param name="bodies">All rigid bodies that can be affected by gravity</param>
    /// <param name="deltaTime">Time step for force integration</param>
    void ApplyGravity(IReadOnlyList<IRigidBody> bodies, float deltaTime);
    
    /// <summary>
    /// Gravitational configuration for this module
    /// </summary>
    GravityConfiguration Configuration { get; set; }
}

/// <summary>
/// Collision detection module interface
/// Educational note: Separates broad-phase and narrow-phase collision detection
/// </summary>
public interface ICollisionModule : IPhysicsModule
{
    /// <summary>
    /// Broad-phase collision detection to find potential collision pairs
    /// Educational note: Spatial acceleration structure reduces O(n²) to O(n log n)
    /// </summary>
    IEnumerable<CollisionPair> BroadPhase(IReadOnlyList<IRigidBody> bodies);
    
    /// <summary>
    /// Narrow-phase collision detection for exact contact information
    /// Educational note: Precise geometric intersection testing
    /// </summary>
    CollisionInfo? NarrowPhase(CollisionPair pair);
    
    /// <summary>
    /// Collision response and constraint resolution
    /// Educational note: Implements conservation of momentum and energy
    /// </summary>
    void ResolveCollisions(IEnumerable<CollisionInfo> collisions);
    
    /// <summary>
    /// Raycast implementation for spatial queries
    /// </summary>
    RaycastHit? Raycast(Vector3 origin, Vector3 direction, float maxDistance, LayerMask layers);
}

/// <summary>
/// Fluid dynamics module interface for drag and buoyancy effects
/// Educational note: Models interaction between solid bodies and fluid environments
/// </summary>
public interface IFluidModule : IPhysicsModule
{
    /// <summary>
    /// Applies fluid drag forces to bodies moving through fluid
    /// Educational note: F_drag = ½ρv²CdA for quadratic drag
    /// </summary>
    void ApplyFluidDrag(IReadOnlyList<IRigidBody> bodies, float deltaTime);
    
    /// <summary>
    /// Applies buoyancy forces to submerged bodies
    /// Educational note: F_buoyancy = ρ_fluid * V_displaced * g (Archimedes' principle)
    /// </summary>
    void ApplyBuoyancy(IReadOnlyList<IRigidBody> bodies, float deltaTime);
    
    /// <summary>
    /// Fluid environment configuration
    /// </summary>
    FluidConfiguration Configuration { get; set; }
}
```

### Modular Physics Service Implementation

```csharp
/// <summary>
/// Modular physics service implementation that composes modules into complete physics system
/// Educational note: Demonstrates composition over inheritance for flexible architecture
/// </summary>
public class ModularPhysicsService : IPhysicsService
{
    private readonly IPhysicsWorld _world;
    private readonly IGravityModule _gravityModule;
    private readonly ICollisionModule _collisionModule;
    private readonly IFluidModule _fluidModule;
    private readonly List<IConstraintModule> _constraintModules;
    
    private bool _disposed;
    
    /// <summary>
    /// Creates modular physics service with specified modules
    /// Educational note: Dependency injection enables testing and module swapping
    /// </summary>
    public ModularPhysicsService(
        IGravityModule gravityModule,
        ICollisionModule collisionModule,
        IFluidModule fluidModule = null,
        IEnumerable<IConstraintModule> constraintModules = null)
    {
        _world = new PhysicsWorld();
        _gravityModule = gravityModule ?? throw new ArgumentNullException(nameof(gravityModule));
        _collisionModule = collisionModule ?? throw new ArgumentNullException(nameof(collisionModule));
        _fluidModule = fluidModule; // Optional
        _constraintModules = constraintModules?.ToList() ?? new List<IConstraintModule>();
        
        // Initialize all modules with world access
        _gravityModule.Initialize(_world);
        _collisionModule.Initialize(_world);
        _fluidModule?.Initialize(_world);
        _constraintModules.ForEach(m => m.Initialize(_world));
    }
    
    /// <summary>
    /// Physics simulation step with modular execution
    /// Educational note: Order matters - forces before collision resolution
    /// </summary>
    public void Step(float deltaTime)
    {
        ThrowIfDisposed();
        
        var bodies = _world.GetAllBodies();
        
        // Phase 1: Apply external forces (gravity, fluid drag, etc.)
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
        
        // Phase 4: Constraint resolution (joints, springs, etc.)
        foreach (var constraintModule in _constraintModules)
        {
            constraintModule.ResolveConstraints(bodies, deltaTime);
        }
        
        // Phase 5: Update ECS components with new physics state
        SynchronizeWithECS();
    }
    
    /// <summary>
    /// Numerical integration of Newton's laws of motion
    /// Educational note: Uses Euler integration for simplicity (RK4 could be added as module)
    /// </summary>
    private void IntegrateMotion(IReadOnlyList<IRigidBody> bodies, float deltaTime)
    {
        foreach (var body in bodies)
        {
            if (body.IsStatic) continue;
            
            // F = ma, therefore a = F / m
            var acceleration = body.AccumulatedForce / body.Mass;
            
            // Integrate velocity: v = v₀ + at
            body.Velocity += acceleration * deltaTime;
            
            // Integrate position: x = x₀ + vt  
            body.Position += body.Velocity * deltaTime;
            
            // Clear accumulated forces for next frame
            body.ClearForces();
        }
    }
    
    public RaycastHit? Raycast(Vector3 origin, Vector3 direction, float maxDistance, LayerMask layers = default)
    {
        ThrowIfDisposed();
        return _collisionModule.Raycast(origin, direction, maxDistance, layers);
    }
    
    public void AddRigidBody(Entity entity, RigidBodyConfig config)
    {
        ThrowIfDisposed();
        _world.AddBody(entity, config);
    }
    
    public void RemoveRigidBody(Entity entity)
    {
        ThrowIfDisposed();
        _world.RemoveBody(entity);
    }
    
    public void AddForce(Entity entity, Vector3 force, ForceMode mode = ForceMode.Force)
    {
        ThrowIfDisposed();
        var body = _world.GetBody(entity);
        if (body != null)
        {
            switch (mode)
            {
                case ForceMode.Force:
                    body.AddForce(force);
                    break;
                case ForceMode.Impulse:
                    body.AddImpulse(force);
                    break;
                case ForceMode.Acceleration:
                    body.AddForce(force * body.Mass);
                    break;
                case ForceMode.VelocityChange:
                    body.AddImpulse(force * body.Mass);
                    break;
            }
        }
    }
    
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ModularPhysicsService));
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        _gravityModule?.Dispose();
        _collisionModule?.Dispose();
        _fluidModule?.Dispose();
        _constraintModules.ForEach(m => m.Dispose());
        _world?.Dispose();
        
        _disposed = true;
    }
}
```

## Module Catalog

### Gravity Modules

#### Constant Gravity Module
```csharp
/// <summary>
/// Simple constant gravity implementation for typical 2D/3D games
/// Educational note: Models uniform gravitational field (valid near planetary surface)
/// Academic reference: Classical mechanics, uniform fields approximation
/// </summary>
public class ConstantGravityModule : IGravityModule
{
    public string Name => "Constant Gravity";
    public GravityConfiguration Configuration { get; set; }
    
    /// <summary>
    /// Applies constant gravitational acceleration to all dynamic bodies
    /// Educational note: F = mg where g is constant acceleration vector
    /// </summary>
    public void ApplyGravity(IReadOnlyList<IRigidBody> bodies, float deltaTime)
    {
        var gravityForce = Configuration.GravityVector; // e.g., (0, -9.81, 0)
        
        foreach (var body in bodies)
        {
            if (!body.IsStatic && body.UseGravity)
            {
                // F = mg (Newton's second law with constant gravitational field)
                var force = gravityForce * body.Mass;
                body.AddForce(force);
            }
        }
    }
    
    public void Initialize(IPhysicsWorld world) { }
    public void Dispose() { }
    public ModulePerformanceInfo PerformanceInfo => new(O.N, memoryUsage: 0);
}
```

#### Realistic Gravity Module
```csharp
/// <summary>
/// N-body gravitational simulation using Newton's law of universal gravitation
/// Educational note: Models realistic gravitational interactions between celestial bodies
/// Academic reference: Newton's Principia, universal gravitation F = G*m1*m2/r²
/// Performance note: O(n²) complexity, suitable for space simulations with limited body count
/// </summary>
public class RealisticGravityModule : IGravityModule
{
    public string Name => "Realistic N-Body Gravity";
    public GravityConfiguration Configuration { get; set; }
    
    // Gravitational constant: 6.674×10⁻¹¹ m³⋅kg⁻¹⋅s⁻²
    private const float GravitationalConstant = 6.674e-11f;
    
    /// <summary>
    /// Applies gravitational forces between all body pairs
    /// Educational note: Every body attracts every other body with force proportional to their masses
    /// </summary>
    public void ApplyGravity(IReadOnlyList<IRigidBody> bodies, float deltaTime)
    {
        // O(n²) algorithm - check every pair of bodies
        for (int i = 0; i < bodies.Count; i++)
        {
            var bodyA = bodies[i];
            if (bodyA.IsStatic || !bodyA.UseGravity) continue;
            
            for (int j = 0; j < bodies.Count; j++)
            {
                if (i == j) continue; // Body doesn't attract itself
                
                var bodyB = bodies[j];
                
                // Calculate gravitational force between bodyA and bodyB
                var offset = bodyB.Position - bodyA.Position;
                var distanceSquared = offset.LengthSquared;
                
                // Avoid division by zero and unrealistic forces at tiny distances
                if (distanceSquared < Configuration.MinGravitationalDistance * Configuration.MinGravitationalDistance)
                    continue;
                
                // F = G * m1 * m2 / r²
                var forceMagnitude = GravitationalConstant * bodyA.Mass * bodyB.Mass / distanceSquared;
                
                // Force direction from bodyA toward bodyB
                var forceDirection = offset.Normalized();
                var gravitationalForce = forceDirection * forceMagnitude;
                
                // Apply equal and opposite forces (Newton's third law)
                bodyA.AddForce(gravitationalForce);
            }
        }
    }
    
    public void Initialize(IPhysicsWorld world) { }
    public void Dispose() { }
    public ModulePerformanceInfo PerformanceInfo => new(O.NSquared, memoryUsage: 0);
}
```

#### Planetary Gravity Module
```csharp
/// <summary>
/// Optimized gravity for planetary surface games with single dominant gravitational source
/// Educational note: Approximates planetary gravity as radial field from planet center
/// Performance note: O(n) complexity, suitable for large numbers of surface objects
/// </summary>
public class PlanetaryGravityModule : IGravityModule
{
    public string Name => "Planetary Surface Gravity";
    public GravityConfiguration Configuration { get; set; }
    
    /// <summary>
    /// Applies radial gravity toward planetary center with distance falloff
    /// Educational note: g = GM/r² where M is planet mass, r is distance from center
    /// </summary>
    public void ApplyGravity(IReadOnlyList<IRigidBody> bodies, float deltaTime)
    {
        var planetCenter = Configuration.PlanetCenter;
        var planetMass = Configuration.PlanetMass;
        var planetRadius = Configuration.PlanetRadius;
        
        foreach (var body in bodies)
        {
            if (body.IsStatic || !body.UseGravity) continue;
            
            var offsetFromCenter = body.Position - planetCenter;
            var distanceFromCenter = offsetFromCenter.Length;
            
            // Use planet radius as minimum distance to avoid infinite forces
            var effectiveDistance = Math.Max(distanceFromCenter, planetRadius);
            
            // g = GM/r² (gravitational acceleration)
            var gravitationalAcceleration = GravitationalConstant * planetMass / (effectiveDistance * effectiveDistance);
            
            // Force toward planet center
            var forceDirection = -offsetFromCenter.Normalized();
            var gravitationalForce = forceDirection * gravitationalAcceleration * body.Mass;
            
            body.AddForce(gravitationalForce);
        }
    }
    
    private const float GravitationalConstant = 6.674e-11f;
    
    public void Initialize(IPhysicsWorld world) { }
    public void Dispose() { }
    public ModulePerformanceInfo PerformanceInfo => new(O.N, memoryUsage: 0);
}
```

### Collision Modules

#### AABB Collision Module
```csharp
/// <summary>
/// Axis-Aligned Bounding Box collision detection for fast 2D games
/// Educational note: Simplest collision primitive, excellent performance
/// Academic reference: Real-Time Collision Detection (Christer Ericson)
/// </summary>
public class AABBCollisionModule : ICollisionModule
{
    public string Name => "AABB Collision Detection";
    private SpatialHashGrid _spatialGrid;
    
    public void Initialize(IPhysicsWorld world)
    {
        _spatialGrid = new SpatialHashGrid(cellSize: 64.0f);
    }
    
    /// <summary>
    /// Broad-phase collision detection using spatial hash grid
    /// Educational note: Reduces collision checks from O(n²) to O(n) average case
    /// </summary>
    public IEnumerable<CollisionPair> BroadPhase(IReadOnlyList<IRigidBody> bodies)
    {
        _spatialGrid.Clear();
        
        // Insert all bodies into spatial grid
        foreach (var body in bodies)
        {
            _spatialGrid.Insert(body, body.GetAABB());
        }
        
        // Query grid for potential collision pairs
        var pairs = new List<CollisionPair>();
        foreach (var body in bodies)
        {
            var candidates = _spatialGrid.Query(body.GetAABB());
            foreach (var candidate in candidates)
            {
                if (body != candidate && body.GetHashCode() < candidate.GetHashCode()) // Avoid duplicate pairs
                {
                    pairs.Add(new CollisionPair(body, candidate));
                }
            }
        }
        
        return pairs;
    }
    
    /// <summary>
    /// AABB vs AABB intersection test
    /// Educational note: Two AABBs intersect if they overlap on all axes
    /// </summary>
    public CollisionInfo? NarrowPhase(CollisionPair pair)
    {
        var aabbA = pair.BodyA.GetAABB();
        var aabbB = pair.BodyB.GetAABB();
        
        // Check for overlap on each axis
        if (aabbA.Max.X < aabbB.Min.X || aabbA.Min.X > aabbB.Max.X) return null; // No X overlap
        if (aabbA.Max.Y < aabbB.Min.Y || aabbA.Min.Y > aabbB.Max.Y) return null; // No Y overlap
        if (aabbA.Max.Z < aabbB.Min.Z || aabbA.Min.Z > aabbB.Max.Z) return null; // No Z overlap
        
        // Calculate penetration depth and normal
        var penetrationX = Math.Min(aabbA.Max.X - aabbB.Min.X, aabbB.Max.X - aabbA.Min.X);
        var penetrationY = Math.Min(aabbA.Max.Y - aabbB.Min.Y, aabbB.Max.Y - aabbA.Min.Y);
        var penetrationZ = Math.Min(aabbA.Max.Z - aabbB.Min.Z, aabbB.Max.Z - aabbA.Min.Z);
        
        // Find axis of minimum penetration (separation direction)
        Vector3 normal;
        float penetration;
        
        if (penetrationX <= penetrationY && penetrationX <= penetrationZ)
        {
            normal = aabbA.Center.X < aabbB.Center.X ? Vector3.UnitX : -Vector3.UnitX;
            penetration = penetrationX;
        }
        else if (penetrationY <= penetrationZ)
        {
            normal = aabbA.Center.Y < aabbB.Center.Y ? Vector3.UnitY : -Vector3.UnitY;
            penetration = penetrationY;
        }
        else
        {
            normal = aabbA.Center.Z < aabbB.Center.Z ? Vector3.UnitZ : -Vector3.UnitZ;
            penetration = penetrationZ;
        }
        
        var contactPoint = (aabbA.Center + aabbB.Center) * 0.5f;
        
        return new CollisionInfo(pair.BodyA, pair.BodyB, contactPoint, normal, penetration);
    }
    
    /// <summary>
    /// Simple impulse-based collision response
    /// Educational note: Implements conservation of momentum for elastic collisions
    /// </summary>
    public void ResolveCollisions(IEnumerable<CollisionInfo> collisions)
    {
        foreach (var collision in collisions)
        {
            ResolveCollision(collision);
        }
    }
    
    private void ResolveCollision(CollisionInfo collision)
    {
        var bodyA = collision.BodyA;
        var bodyB = collision.BodyB;
        
        // Separate overlapping bodies
        var separation = collision.Normal * collision.Penetration * 0.5f;
        if (!bodyA.IsStatic) bodyA.Position += separation;
        if (!bodyB.IsStatic) bodyB.Position -= separation;
        
        // Calculate relative velocity
        var relativeVelocity = bodyB.Velocity - bodyA.Velocity;
        var velocityAlongNormal = Vector3.Dot(relativeVelocity, collision.Normal);
        
        // Don't resolve if objects are separating
        if (velocityAlongNormal > 0) return;
        
        // Calculate collision impulse
        var restitution = (bodyA.Restitution + bodyB.Restitution) * 0.5f;
        var impulseScalar = -(1 + restitution) * velocityAlongNormal;
        impulseScalar /= (1 / bodyA.Mass) + (1 / bodyB.Mass);
        
        var impulse = collision.Normal * impulseScalar;
        
        // Apply impulse to bodies
        if (!bodyA.IsStatic) bodyA.Velocity -= impulse / bodyA.Mass;
        if (!bodyB.IsStatic) bodyB.Velocity += impulse / bodyB.Mass;
    }
    
    public RaycastHit? Raycast(Vector3 origin, Vector3 direction, float maxDistance, LayerMask layers)
    {
        // Simple AABB ray intersection test
        var candidates = _spatialGrid.QueryRay(origin, direction, maxDistance);
        
        RaycastHit? closest = null;
        float closestDistance = maxDistance;
        
        foreach (var body in candidates)
        {
            if (!layers.Contains(body.Layer)) continue;
            
            var aabb = body.GetAABB();
            if (RayAABBIntersection(origin, direction, aabb, out var distance) && distance < closestDistance)
            {
                closestDistance = distance;
                var hitPoint = origin + direction * distance;
                var normal = CalculateAABBNormal(hitPoint, aabb);
                closest = new RaycastHit(body.Entity, hitPoint, normal, distance);
            }
        }
        
        return closest;
    }
    
    public void Dispose()
    {
        _spatialGrid?.Dispose();
    }
    
    public ModulePerformanceInfo PerformanceInfo => new(O.N, memoryUsage: _spatialGrid?.MemoryUsage ?? 0);
}
```

### Fluid Modules

#### Realistic Drag Module
```csharp
/// <summary>
/// Realistic fluid drag implementation supporting linear and quadratic drag models
/// Educational note: Models interaction between moving objects and fluid environment
/// Academic reference: Fluid mechanics, drag equations
/// </summary>
public class RealisticDragModule : IFluidModule
{
    public string Name => "Realistic Fluid Drag";
    public FluidConfiguration Configuration { get; set; }
    
    /// <summary>
    /// Applies fluid drag forces based on velocity and object properties
    /// Educational note: F_drag = ½ρv²C_dA for quadratic drag, F_drag = bv for linear drag
    /// </summary>
    public void ApplyFluidDrag(IReadOnlyList<IRigidBody> bodies, float deltaTime)
    {
        foreach (var body in bodies)
        {
            if (body.IsStatic || body.Velocity.LengthSquared < 1e-6f) continue;
            
            var velocity = body.Velocity;
            var speed = velocity.Length;
            
            // Get body's drag properties
            var dragCoefficient = body.DragCoefficient;
            var crossSectionalArea = body.CrossSectionalArea;
            
            Vector3 dragForce;
            
            if (Configuration.UseQuadraticDrag)
            {
                // Quadratic drag: F = ½ρv²C_dA
                var dragMagnitude = 0.5f * Configuration.FluidDensity * speed * speed * dragCoefficient * crossSectionalArea;
                dragForce = -velocity.Normalized() * dragMagnitude;
            }
            else
            {
                // Linear drag: F = bv (Stokes' law for low Reynolds numbers)
                var dragMagnitude = Configuration.LinearDragCoefficient * speed;
                dragForce = -velocity.Normalized() * dragMagnitude;
            }
            
            body.AddForce(dragForce);
        }
    }
    
    /// <summary>
    /// Applies buoyancy forces to submerged objects
    /// Educational note: F_buoyancy = ρ_fluid * V_displaced * g (Archimedes' principle)
    /// </summary>
    public void ApplyBuoyancy(IReadOnlyList<IRigidBody> bodies, float deltaTime)
    {
        if (Configuration.FluidLevel == null) return;
        
        var fluidLevel = Configuration.FluidLevel.Value;
        var fluidDensity = Configuration.FluidDensity;
        var gravity = Configuration.GravityMagnitude;
        
        foreach (var body in bodies)
        {
            if (body.IsStatic) continue;
            
            // Check if body is partially or fully submerged
            var bodyBottom = body.Position.Y - body.Size.Y * 0.5f;
            var bodyTop = body.Position.Y + body.Size.Y * 0.5f;
            
            if (bodyBottom >= fluidLevel) continue; // Not submerged
            
            // Calculate submerged volume fraction
            var submergedHeight = Math.Min(fluidLevel - bodyBottom, body.Size.Y);
            var submergedFraction = submergedHeight / body.Size.Y;
            var displacedVolume = body.Volume * submergedFraction;
            
            // Apply buoyancy force upward
            var buoyancyMagnitude = fluidDensity * displacedVolume * gravity;
            var buoyancyForce = Vector3.UnitY * buoyancyMagnitude;
            
            body.AddForce(buoyancyForce);
        }
    }
    
    public void Initialize(IPhysicsWorld world) { }
    public void Dispose() { }
    public ModulePerformanceInfo PerformanceInfo => new(O.N, memoryUsage: 0);
}
```

## Physics Builder API

### Fluent Builder Pattern
```csharp
/// <summary>
/// Fluent builder for creating modular physics services
/// Educational note: Builder pattern enables complex object construction with validation
/// </summary>
public class PhysicsBuilder
{
    private IGravityModule _gravityModule;
    private ICollisionModule _collisionModule;
    private IFluidModule _fluidModule;
    private readonly List<IConstraintModule> _constraintModules = new();
    
    private PhysicsBuilder() { }
    
    /// <summary>
    /// Creates new physics builder with default configuration
    /// </summary>
    public static PhysicsBuilder Create() => new PhysicsBuilder();
    
    /// <summary>
    /// Configures gravity module for physics simulation
    /// </summary>
    public PhysicsBuilder WithGravity(IGravityModule gravityModule)
    {
        _gravityModule = gravityModule ?? throw new ArgumentNullException(nameof(gravityModule));
        return this;
    }
    
    /// <summary>
    /// Convenience method for common gravity types
    /// </summary>
    public PhysicsBuilder WithGravity(GravityType type, GravityConfiguration config = null)
    {
        config ??= GravityConfiguration.Default;
        
        _gravityModule = type switch
        {
            GravityType.None => new NoGravityModule(),
            GravityType.Constant => new ConstantGravityModule { Configuration = config },
            GravityType.Realistic => new RealisticGravityModule { Configuration = config },
            GravityType.Planetary => new PlanetaryGravityModule { Configuration = config },
            _ => throw new ArgumentException($"Unknown gravity type: {type}")
        };
        
        return this;
    }
    
    /// <summary>
    /// Configures collision detection module
    /// </summary>
    public PhysicsBuilder WithCollision(ICollisionModule collisionModule)
    {
        _collisionModule = collisionModule ?? throw new ArgumentNullException(nameof(collisionModule));
        return this;
    }
    
    /// <summary>
    /// Convenience method for common collision types
    /// </summary>
    public PhysicsBuilder WithCollision(CollisionType type, CollisionConfiguration config = null)
    {
        config ??= CollisionConfiguration.Default;
        
        _collisionModule = type switch
        {
            CollisionType.AABB => new AABBCollisionModule { Configuration = config },
            CollisionType.Bepu => new BepuCollisionModule { Configuration = config },
            CollisionType.Custom => new CustomCollisionModule { Configuration = config },
            _ => throw new ArgumentException($"Unknown collision type: {type}")
        };
        
        return this;
    }
    
    /// <summary>
    /// Configures fluid dynamics module (optional)
    /// </summary>
    public PhysicsBuilder WithFluid(IFluidModule fluidModule)
    {
        _fluidModule = fluidModule;
        return this;
    }
    
    /// <summary>
    /// Convenience method for common fluid types
    /// </summary>
    public PhysicsBuilder WithFluid(FluidType type, FluidConfiguration config = null)
    {
        if (type == FluidType.None)
        {
            _fluidModule = null;
            return this;
        }
        
        config ??= FluidConfiguration.Default;
        
        _fluidModule = type switch
        {
            FluidType.LinearDrag => new LinearDragModule { Configuration = config },
            FluidType.QuadraticDrag => new RealisticDragModule { Configuration = config },
            FluidType.Water => new WaterModule { Configuration = config },
            FluidType.Air => new AirModule { Configuration = config },
            _ => throw new ArgumentException($"Unknown fluid type: {type}")
        };
        
        return this;
    }
    
    /// <summary>
    /// Adds constraint module for joints, springs, etc.
    /// </summary>
    public PhysicsBuilder WithConstraint(IConstraintModule constraintModule)
    {
        if (constraintModule != null)
            _constraintModules.Add(constraintModule);
        return this;
    }
    
    /// <summary>
    /// Builds physics service with validation
    /// </summary>
    public IPhysicsService Build()
    {
        // Validate required modules
        if (_gravityModule == null)
            throw new InvalidOperationException("Gravity module is required. Use WithGravity() or WithGravity(GravityType.None)");
        
        if (_collisionModule == null)
            throw new InvalidOperationException("Collision module is required. Use WithCollision()");
        
        return new ModularPhysicsService(_gravityModule, _collisionModule, _fluidModule, _constraintModules);
    }
}
```

### Preset Configurations
```csharp
/// <summary>
/// Common physics configurations for different game types
/// Educational note: Presets demonstrate typical module combinations
/// </summary>
public static class PhysicsPresets
{
    /// <summary>
    /// Fast 2D physics suitable for platformers and arcade games
    /// Educational note: Prioritizes performance and predictable behavior
    /// </summary>
    public static IPhysicsService Platformer2D()
    {
        return PhysicsBuilder.Create()
            .WithGravity(GravityType.Constant, new GravityConfiguration 
            { 
                GravityVector = new Vector3(0, -9.81f, 0) 
            })
            .WithCollision(CollisionType.AABB, new CollisionConfiguration
            {
                BroadPhaseType = BroadPhaseType.SpatialHash,
                EnableContinuousCollision = false
            })
            .Build();
    }
    
    /// <summary>
    /// Top-down 2D physics for roguelikes and strategy games
    /// Educational note: No gravity, emphasis on raycasting and spatial queries
    /// </summary>
    public static IPhysicsService TopDown2D()
    {
        return PhysicsBuilder.Create()
            .WithGravity(GravityType.None)
            .WithCollision(CollisionType.AABB, new CollisionConfiguration
            {
                BroadPhaseType = BroadPhaseType.SpatialHash,
                EnableRaycasting = true,
                RaycastPrecision = RaycastPrecision.High
            })
            .Build();
    }
    
    /// <summary>
    /// Realistic space simulation with N-body gravity and fluid drag
    /// Educational note: Demonstrates scientific accuracy over game performance
    /// </summary>
    public static IPhysicsService SpaceSimulation()
    {
        return PhysicsBuilder.Create()
            .WithGravity(GravityType.Realistic, new GravityConfiguration
            {
                MinGravitationalDistance = 1000.0f, // Avoid singularities
                UseRelativeUnits = true // Scale for game-appropriate forces
            })
            .WithCollision(CollisionType.AABB, new CollisionConfiguration
            {
                BroadPhaseType = BroadPhaseType.Octree // Better for 3D space
            })
            .WithFluid(FluidType.QuadraticDrag, new FluidConfiguration
            {
                FluidDensity = 1.225f, // Air density at sea level
                UseQuadraticDrag = true
            })
            .Build();
    }
    
    /// <summary>
    /// High-performance physics using Bepu Physics 2 backend
    /// Educational note: Leverages external engine for complex scenarios
    /// </summary>
    public static IPhysicsService HighPerformance3D()
    {
        return PhysicsBuilder.Create()
            .WithGravity(GravityType.Constant, new GravityConfiguration
            {
                GravityVector = new Vector3(0, -9.81f, 0)
            })
            .WithCollision(CollisionType.Bepu, new CollisionConfiguration
            {
                EnableMultithreading = true,
                SolverIterations = 8
            })
            .WithFluid(FluidType.Air) // Basic air resistance
            .Build();
    }
    
    /// <summary>
    /// Null physics implementation for testing and headless scenarios
    /// Educational note: Null object pattern enables testing without simulation overhead
    /// </summary>
    public static IPhysicsService Headless()
    {
        return new NullPhysicsService();
    }
}
```

## ECS Integration

### Physics Components
```csharp
/// <summary>
/// Core rigid body component for physics simulation
/// Educational note: Stores fundamental physical properties of game objects
/// </summary>
public readonly record struct RigidBodyComponent(
    float Mass,
    Vector3 Velocity,
    Vector3 AngularVelocity,
    bool IsStatic,
    bool UseGravity = true,
    float Restitution = 0.5f,  // Bounciness (0 = no bounce, 1 = perfect bounce)
    float Friction = 0.5f,     // Surface friction coefficient
    BodyType Type = BodyType.Dynamic
) : IComponent;

/// <summary>
/// Collision shape component defining physical boundaries
/// Educational note: Separates collision geometry from visual representation
/// </summary>
public readonly record struct ColliderComponent(
    ColliderType Type,         // AABB, Sphere, Capsule, Mesh
    Vector3 Size,             // Dimensions for box/capsule, radius for sphere
    Vector3 Offset = default, // Local offset from entity position
    PhysicsLayer Layer = PhysicsLayer.Default,
    bool IsTrigger = false    // Trigger colliders detect but don't physically collide
) : IComponent;

/// <summary>
/// Celestial body component for realistic gravitational physics
/// Educational note: Enables N-body gravitational simulations
/// </summary>
public readonly record struct CelestialBodyComponent(
    float Mass,               // Astronomical mass (kg)
    float Radius,            // Physical radius (m)  
    bool GeneratesGravity = true,
    CelestialBodyType Type = CelestialBodyType.Planet
) : IComponent;

/// <summary>
/// Fluid interaction component for drag and buoyancy effects
/// Educational note: Models how objects interact with fluid environments
/// </summary>
public readonly record struct FluidBodyComponent(
    float DragCoefficient,    // Cd value for drag calculation (0.47 for sphere)
    float CrossSectionalArea, // Area perpendicular to motion (m²)
    float Volume,            // Object volume for buoyancy (m³)
    FluidType SurroundingFluid = FluidType.Air
) : IComponent;

/// <summary>
/// Thrust component for spacecraft and vehicles
/// Educational note: Models propulsion systems with fuel consumption
/// </summary>
public readonly record struct ThrusterComponent(
    float MaxThrust,         // Maximum thrust force (N)
    Vector3 ThrustDirection, // Local thrust direction
    float FuelMass,         // Current fuel mass (kg)
    float FuelConsumptionRate, // Fuel consumption per Newton-second
    bool IsActive = false
) : IComponent;
```

### Physics System Integration
```csharp
/// <summary>
/// ECS system that synchronizes physics simulation with entity transforms
/// Educational note: Bridges ECS world state with physics world state
/// </summary>
public class PhysicsSystem : ISystem
{
    private readonly IPhysicsService _physicsService;
    private readonly Dictionary<Entity, PhysicsBodyHandle> _physicsBodyMap;
    
    public PhysicsSystem(IPhysicsService physicsService)
    {
        _physicsService = physicsService ?? throw new ArgumentNullException(nameof(physicsService));
        _physicsBodyMap = new Dictionary<Entity, PhysicsBodyHandle>();
    }
    
    public void Update(World world, float deltaTime)
    {
        // Step 1: Sync new rigid bodies to physics world
        SyncNewRigidBodies(world);
        
        // Step 2: Update physics bodies with current transform data
        SyncTransformToPhysics(world);
        
        // Step 3: Apply forces from thrust components
        ApplyThrustForces(world);
        
        // Step 4: Step physics simulation
        _physicsService.Step(deltaTime);
        
        // Step 5: Update transform components with physics results
        SyncPhysicsToTransform(world);
        
        // Step 6: Handle collision events
        ProcessCollisionEvents(world);
        
        // Step 7: Clean up destroyed entities
        CleanupDestroyedEntities(world);
    }
    
    private void SyncNewRigidBodies(World world)
    {
        foreach (var (entity, rigidBody, transform) in world.Query<RigidBodyComponent, TransformComponent>())
        {
            if (_physicsBodyMap.ContainsKey(entity)) continue;
            
            var config = new RigidBodyConfig
            {
                Mass = rigidBody.Mass,
                Position = transform.Position,
                Velocity = rigidBody.Velocity,
                IsStatic = rigidBody.IsStatic,
                UseGravity = rigidBody.UseGravity,
                Restitution = rigidBody.Restitution,
                Friction = rigidBody.Friction
            };
            
            // Add collider if present
            var collider = world.GetComponent<ColliderComponent>(entity);
            if (collider.HasValue)
            {
                config.ColliderType = collider.Value.Type;
                config.ColliderSize = collider.Value.Size;
                config.ColliderOffset = collider.Value.Offset;
                config.Layer = collider.Value.Layer;
                config.IsTrigger = collider.Value.IsTrigger;
            }
            
            _physicsService.AddRigidBody(entity, config);
            _physicsBodyMap[entity] = new PhysicsBodyHandle(entity);
        }
    }
    
    private void ApplyThrustForces(World world)
    {
        foreach (var (entity, thruster) in world.Query<ThrusterComponent>())
        {
            if (!thruster.IsActive || thruster.FuelMass <= 0) continue;
            
            var transform = world.GetComponent<TransformComponent>(entity);
            if (!transform.HasValue) continue;
            
            // Calculate thrust force in world space
            var worldThrustDirection = Vector3.Transform(thruster.ThrustDirection, transform.Value.Rotation);
            var thrustForce = worldThrustDirection * thruster.MaxThrust;
            
            _physicsService.AddForce(entity, thrustForce, ForceMode.Force);
            
            // Consume fuel based on thrust usage
            var fuelConsumed = thruster.MaxThrust * thruster.FuelConsumptionRate * Time.deltaTime;
            var newFuelMass = Math.Max(0, thruster.FuelMass - fuelConsumed);
            
            world.SetComponent(entity, thruster with { FuelMass = newFuelMass });
        }
    }
    
    private void SyncPhysicsToTransform(World world)
    {
        foreach (var (entity, rigidBody, transform) in world.Query<RigidBodyComponent, TransformComponent>())
        {
            if (!_physicsBodyMap.ContainsKey(entity)) continue;
            
            var physicsBody = _physicsService.GetRigidBody(entity);
            if (physicsBody != null)
            {
                // Update transform with physics results
                world.SetComponent(entity, new TransformComponent(
                    physicsBody.Position,
                    physicsBody.Rotation,
                    transform.Scale
                ));
                
                // Update rigid body component with new velocity
                world.SetComponent(entity, rigidBody with 
                { 
                    Velocity = physicsBody.Velocity,
                    AngularVelocity = physicsBody.AngularVelocity
                });
            }
        }
    }
}
```

## Usage Examples

### Simple 2D Roguelike Setup
```csharp
// Perfect for your roguelike shooter target
var physics = PhysicsBuilder.Create()
    .WithGravity(GravityType.None)           // Top-down game, no gravity
    .WithCollision(CollisionType.AABB)       // Fast AABB collision
    .Build();

// Add to ECS entity
var player = world.CreateEntity();
world.SetComponent(player, new RigidBodyComponent(Mass: 1.0f, Velocity: Vector3.Zero, IsStatic: false));
world.SetComponent(player, new ColliderComponent(ColliderType.AABB, Size: new Vector3(32, 32, 1)));

// Raycasting for bullets and line-of-sight
var hit = physics.Raycast(bulletStart, bulletDirection, bulletRange, LayerMask.Enemies);
if (hit.HasValue)
{
    var enemy = hit.Value.Entity;
    DamageEnemy(enemy, bulletDamage);
}
```

### Space Simulation Setup
```csharp
// Realistic orbital mechanics
var physics = PhysicsBuilder.Create()
    .WithGravity(GravityType.Realistic)      // N-body gravitational physics
    .WithCollision(CollisionType.AABB)       // Basic collision for demonstration
    .WithFluid(FluidType.QuadraticDrag)     // Realistic atmospheric drag
    .Build();

// Create Earth
var earth = world.CreateEntity();
world.SetComponent(earth, new CelestialBodyComponent(
    Mass: 5.972e24f,        // Earth's mass in kg
    Radius: 6.371e6f,       // Earth's radius in meters
    GeneratesGravity: true
));

// Create spacecraft
var spacecraft = world.CreateEntity();
world.SetComponent(spacecraft, new RigidBodyComponent(
    Mass: 1000.0f,          // 1 ton spacecraft
    Velocity: new Vector3(7800, 0, 0), // Orbital velocity
    IsStatic: false,
    UseGravity: true
));
world.SetComponent(spacecraft, new ThrusterComponent(
    MaxThrust: 10000.0f,    // 10 kN thruster
    ThrustDirection: Vector3.UnitX,
    FuelMass: 500.0f,       // 500 kg fuel
    FuelConsumptionRate: 0.001f
));
```

### Performance Comparison Setup
```csharp
// Educational: Compare different gravity implementations
var simplePhysics = PhysicsBuilder.Create()
    .WithGravity(GravityType.Constant)      // O(n) performance
    .WithCollision(CollisionType.AABB)
    .Build();

var realisticPhysics = PhysicsBuilder.Create()
    .WithGravity(GravityType.Realistic)     // O(n²) performance
    .WithCollision(CollisionType.AABB)
    .Build();

// Students can see performance difference with many objects
CreateManyBodies(simplePhysics, count: 1000);    // Runs at 60fps
CreateManyBodies(realisticPhysics, count: 100);  // Runs at 60fps (10x fewer objects)
```

## Implementation Timeline

### Week 9-10: Foundation and Basic Modules
- **Core Interfaces**: IPhysicsService, module interfaces, builder pattern
- **Basic Modules**: ConstantGravityModule, AABBCollisionModule, NoDragModule
- **ECS Integration**: Physics components and system integration
- **Simple Preset**: TopDown2D for roguelike shooter

### Week 11-12: Advanced Modules
- **Realistic Physics**: Port your gravity and fluid drag modules
- **Spatial Optimization**: Spatial hash grid for collision broad-phase
- **Builder Enhancement**: Complete preset system and validation
- **Educational Content**: Documentation with physics explanations

### Week 13-14: External Integration and Polish
- **Bepu Integration**: BepuCollisionModule as external engine option
- **Performance Profiling**: Module performance monitoring and optimization
- **Advanced Features**: Constraint modules, thrust systems
- **Sample Games**: Space simulation and physics playground

## Performance Considerations

### Module Overhead Analysis
```csharp
// Performance characteristics of different module combinations
TopDown2D():         O(n) gravity + O(n log n) collision = ~60fps with 1000 objects
Platformer2D():      O(n) gravity + O(n log n) collision = ~60fps with 1000 objects  
SpaceSimulation():   O(n²) gravity + O(n log n) collision = ~60fps with 100 objects
HighPerformance3D(): O(n) gravity + O(n) collision (Bepu) = ~60fps with 5000 objects
```

### Memory Usage Optimization
- **Module composition**: Modules only allocated when used
- **Spatial structures**: Hash grids and octrees use memory proportional to occupied space
- **Component caching**: Physics bodies cached for fast ECS synchronization

### Profiling Integration
```csharp
public class ModulePerformanceProfiler
{
    public void ProfilePhysicsStep(IPhysicsService physics, int frameCount = 100)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < frameCount; i++)
        {
            physics.Step(1.0f / 60.0f);
        }
        
        stopwatch.Stop();
        
        var avgFrameTime = stopwatch.ElapsedMilliseconds / (float)frameCount;
        Console.WriteLine($"Average physics step: {avgFrameTime:F2}ms");
        Console.WriteLine($"Estimated FPS limit: {1000.0f / avgFrameTime:F0}fps");
    }
}
```

## Extension Points

### Custom Module Development
```csharp
// Developers can create custom modules following established interfaces
public class MagnetismModule : IForceModule
{
    public void ApplyForces(IReadOnlyList<IRigidBody> bodies, float deltaTime)
    {
        // Custom magnetic force implementation
        foreach (var body in bodies)
        {
            if (body.HasComponent<MagneticComponent>())
            {
                ApplyMagneticForces(body, bodies);
            }
        }
    }
}

// Register custom module
var physics = PhysicsBuilder.Create()
    .WithGravity(GravityType.None)
    .WithCollision(CollisionType.AABB)
    .WithCustomModule(new MagnetismModule())
    .Build();
```

### Future Enhancement Opportunities
- **Visual debugging**: Module-specific debug rendering and profiling overlays
- **Networking integration**: Deterministic physics for multiplayer games
- **Advanced constraints**: Rope, cloth, and soft body physics modules
- **Procedural generation**: Physics-based world generation and destruction
- **Machine learning**: AI-driven physics parameter optimization

## See Also

- [System Overview](../architecture/system-overview.md) - RACEngine architecture context
- [ECS Architecture](../architecture/ecs-architecture.md) - Component and system patterns
- [Performance Considerations](../architecture/performance-considerations.md) - Optimization strategies
- [Getting Started Tutorial](../educational-material/getting-started-tutorial.md) - Practical usage examples

## Changelog

- 2025-06-27: Comprehensive modular physics design document with educational focus and implementation roadmap