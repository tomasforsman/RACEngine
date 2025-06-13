using System;

namespace Rac.Physics;

/// <summary>
/// Null Object pattern implementation of IPhysicsService.
/// Provides safe no-op physics functionality for testing and fallback scenarios.
/// </summary>
public class NullPhysicsService : IPhysicsService
{
#if DEBUG
    private static bool _warningShown = false;
    
    private static void ShowWarningOnce()
    {
        if (!_warningShown)
        {
            _warningShown = true;
            Console.WriteLine("[DEBUG] Warning: NullPhysicsService is being used - no physics simulation will occur.");
        }
    }
#endif

    // Simple physics methods
    public void Initialize()
    {
#if DEBUG
        ShowWarningOnce();
#endif
        // No-op: no physics world to initialize
    }
    
    public void Update(float deltaTime)
    {
        // No-op: no simulation to update
    }
    
    public int AddStaticBox(float x, float y, float z, float width, float height, float depth)
    {
        // Return dummy body ID
        return -1;
    }
    
    public int AddDynamicSphere(float x, float y, float z, float radius, float mass = 1.0f)
    {
        // Return dummy body ID
        return -1;
    }
    
    public void RemoveBody(int bodyId)
    {
        // No-op: no bodies to remove
    }
    
    public void SetGravity(float x, float y, float z)
    {
        // No-op: no gravity to set
    }
    
    // Advanced physics methods
    public int AddDynamicBox(float x, float y, float z, float width, float height, float depth, 
        float mass, float friction = 0.5f, float restitution = 0.3f)
    {
        // Return dummy body ID
        return -1;
    }
    
    public int AddCapsule(float x, float y, float z, float radius, float height, float mass, bool isStatic = false)
    {
        // Return dummy body ID
        return -1;
    }
    
    public void ApplyForce(int bodyId, float forceX, float forceY, float forceZ)
    {
        // No-op: no forces to apply
    }
    
    public void ApplyImpulse(int bodyId, float impulseX, float impulseY, float impulseZ)
    {
        // No-op: no impulses to apply
    }
    
    public void SetVelocity(int bodyId, float velocityX, float velocityY, float velocityZ)
    {
        // No-op: no velocities to set
    }
    
    public void GetPosition(int bodyId, out float x, out float y, out float z)
    {
        // Return default position
        x = 0.0f;
        y = 0.0f;
        z = 0.0f;
    }
    
    public void SetPosition(int bodyId, float x, float y, float z)
    {
        // No-op: no positions to set
    }
    
    public void GetRotation(int bodyId, out float x, out float y, out float z, out float w)
    {
        // Return identity quaternion
        x = 0.0f;
        y = 0.0f;
        z = 0.0f;
        w = 1.0f;
    }
    
    public void SetRotation(int bodyId, float x, float y, float z, float w)
    {
        // No-op: no rotations to set
    }
    
    public bool Raycast(float fromX, float fromY, float fromZ, float toX, float toY, float toZ, out int hitBodyId)
    {
        // No hits in null physics world
        hitBodyId = -1;
        return false;
    }
    
    public void SetCollisionFilter(int bodyId, int collisionGroup, int collisionMask)
    {
        // No-op: no collision filters to set
    }
    
    public void Shutdown()
    {
        // No-op: nothing to shutdown
    }
}