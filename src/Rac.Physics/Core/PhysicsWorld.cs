using Rac.Physics.Core;
using Rac.ECS.Core;
using System.Collections.Concurrent;

namespace Rac.Physics.Core;

// ═══════════════════════════════════════════════════════════════
// PHYSICS WORLD IMPLEMENTATION
// Educational note: Basic physics world for managing rigid bodies
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Basic physics world implementation for managing rigid bodies.
/// Educational note: Centralized storage and access for all physics bodies in the simulation.
/// Thread-safe implementation using concurrent collections for potential multi-threading.
/// </summary>
internal class PhysicsWorld : IPhysicsWorld
{
    private readonly ConcurrentDictionary<Entity, RigidBody> _bodies;
    private readonly object _bodiesLock = new();
    private bool _disposed;

    /// <summary>
    /// Creates a new physics world.
    /// </summary>
    public PhysicsWorld()
    {
        _bodies = new ConcurrentDictionary<Entity, RigidBody>();
    }

    /// <summary>
    /// Gets all rigid bodies currently in the physics simulation.
    /// Educational note: Returns a snapshot to avoid collection modification during iteration.
    /// </summary>
    /// <returns>Read-only collection of all active rigid bodies</returns>
    public IReadOnlyList<IRigidBody> GetAllBodies()
    {
        ThrowIfDisposed();
        
        lock (_bodiesLock)
        {
            // Return a snapshot to avoid concurrent modification issues
            return _bodies.Values.Cast<IRigidBody>().ToList();
        }
    }

    /// <summary>
    /// Gets a specific rigid body by entity identifier.
    /// Educational note: O(1) lookup using concurrent dictionary.
    /// </summary>
    /// <param name="entity">Entity identifier</param>
    /// <returns>Rigid body associated with entity, or null if not found</returns>
    public IRigidBody? GetBody(Entity entity)
    {
        ThrowIfDisposed();
        
        return _bodies.TryGetValue(entity, out var body) ? body : null;
    }

    /// <summary>
    /// Adds a new rigid body to the physics simulation.
    /// Educational note: Creates internal RigidBody from configuration.
    /// </summary>
    /// <param name="entity">Entity identifier</param>
    /// <param name="config">Configuration for the rigid body</param>
    /// <exception cref="ArgumentException">Thrown if entity already has a rigid body</exception>
    public void AddBody(Entity entity, RigidBodyConfig config)
    {
        ThrowIfDisposed();
        
        var rigidBody = new RigidBody(entity, config);
        
        if (!_bodies.TryAdd(entity, rigidBody))
        {
            throw new ArgumentException($"Entity {entity.Id} already has a rigid body in the physics world");
        }
    }

    /// <summary>
    /// Removes a rigid body from the physics simulation.
    /// Educational note: Safe removal even if entity doesn't exist.
    /// </summary>
    /// <param name="entity">Entity identifier</param>
    public void RemoveBody(Entity entity)
    {
        ThrowIfDisposed();
        
        _bodies.TryRemove(entity, out _);
        // Educational note: TryRemove is safe even if key doesn't exist
    }

    /// <summary>
    /// Gets the current number of rigid bodies in the simulation.
    /// Educational note: Useful for performance monitoring and debugging.
    /// </summary>
    public int BodyCount => _bodies.Count;

    /// <summary>
    /// Checks if the physics world contains a body for the given entity.
    /// </summary>
    /// <param name="entity">Entity identifier to check</param>
    /// <returns>True if the entity has a rigid body in this world</returns>
    public bool ContainsBody(Entity entity)
    {
        ThrowIfDisposed();
        return _bodies.ContainsKey(entity);
    }

    /// <summary>
    /// Clears all rigid bodies from the physics world.
    /// Educational note: Useful for scene transitions or cleanup.
    /// </summary>
    public void ClearAllBodies()
    {
        ThrowIfDisposed();
        
        lock (_bodiesLock)
        {
            _bodies.Clear();
        }
    }

    /// <summary>
    /// Throws ObjectDisposedException if the world has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(PhysicsWorld));
        }
    }

    /// <summary>
    /// Disposes the physics world and cleans up resources.
    /// Educational note: Follows standard .NET disposal pattern.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        lock (_bodiesLock)
        {
            _bodies.Clear();
        }

        _disposed = true;
    }
}