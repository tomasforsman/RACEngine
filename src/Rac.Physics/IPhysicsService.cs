namespace Rac.Physics;

/// <summary>
/// Physics service interface providing both simple and advanced physics functionality.
/// </summary>
public interface IPhysicsService
{
    // Simple physics methods
    /// <summary>Initialize the physics world.</summary>
    void Initialize();
    
    /// <summary>Step the physics simulation by the given time delta.</summary>
    void Update(float deltaTime);
    
    /// <summary>Add a static box collider to the world.</summary>
    int AddStaticBox(float x, float y, float z, float width, float height, float depth);
    
    /// <summary>Add a dynamic sphere to the world.</summary>
    int AddDynamicSphere(float x, float y, float z, float radius, float mass = 1.0f);
    
    /// <summary>Remove a physics body from the world.</summary>
    void RemoveBody(int bodyId);
    
    /// <summary>Set gravity for the physics world.</summary>
    void SetGravity(float x, float y, float z);
    
    // Advanced physics methods
    /// <summary>Add a dynamic box with advanced properties.</summary>
    int AddDynamicBox(float x, float y, float z, float width, float height, float depth, 
        float mass, float friction = 0.5f, float restitution = 0.3f);
    
    /// <summary>Add a capsule collider.</summary>
    int AddCapsule(float x, float y, float z, float radius, float height, float mass, bool isStatic = false);
    
    /// <summary>Apply force to a dynamic body.</summary>
    void ApplyForce(int bodyId, float forceX, float forceY, float forceZ);
    
    /// <summary>Apply impulse to a dynamic body.</summary>
    void ApplyImpulse(int bodyId, float impulseX, float impulseY, float impulseZ);
    
    /// <summary>Set body velocity directly.</summary>
    void SetVelocity(int bodyId, float velocityX, float velocityY, float velocityZ);
    
    /// <summary>Get body position.</summary>
    void GetPosition(int bodyId, out float x, out float y, out float z);
    
    /// <summary>Set body position.</summary>
    void SetPosition(int bodyId, float x, float y, float z);
    
    /// <summary>Get body rotation as quaternion.</summary>
    void GetRotation(int bodyId, out float x, out float y, out float z, out float w);
    
    /// <summary>Set body rotation from quaternion.</summary>
    void SetRotation(int bodyId, float x, float y, float z, float w);
    
    /// <summary>Perform a raycast and return the first hit.</summary>
    bool Raycast(float fromX, float fromY, float fromZ, float toX, float toY, float toZ, out int hitBodyId);
    
    /// <summary>Set collision filtering for a body.</summary>
    void SetCollisionFilter(int bodyId, int collisionGroup, int collisionMask);
    
    /// <summary>Shutdown and cleanup the physics world.</summary>
    void Shutdown();
}