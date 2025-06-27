using Rac.Physics.Modules;
using Silk.NET.Maths;

namespace Rac.Physics.Modules.Gravity;

// ═══════════════════════════════════════════════════════════════
// GRAVITY MODULES - WEEK 9-10 FOUNDATION
// Educational note: Different gravity implementations for various game types
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// No gravity module for top-down games and space environments.
/// Educational note: Perfect for roguelike shooters where gravity would be unwanted.
/// Examples: Top-down shooters, strategy games, space games without gravity.
/// </summary>
public class NoGravityModule : IGravityModule
{
    /// <summary>
    /// Module name for debugging and configuration.
    /// </summary>
    public string Name => "No Gravity";

    /// <summary>
    /// Applies no gravitational forces (no-op implementation).
    /// Educational note: Sometimes the best solution is to do nothing!
    /// </summary>
    /// <param name="bodies">All rigid bodies (ignored)</param>
    /// <param name="deltaTime">Time step (ignored)</param>
    public void ApplyGravity(IReadOnlyList<IRigidBody> bodies, float deltaTime)
    {
        // No gravitational forces applied - bodies maintain their current velocities
        // Educational note: This is actually Newton's first law - objects in motion stay in motion
    }

    /// <summary>
    /// Initializes the no gravity module (no-op).
    /// </summary>
    /// <param name="world">Physics world (not needed for no gravity)</param>
    public void Initialize(IPhysicsWorld world)
    {
        // No initialization required for no gravity
    }

    /// <summary>
    /// Disposes the no gravity module (no-op).
    /// </summary>
    public void Dispose()
    {
        // No resources to dispose for no gravity
    }
}

/// <summary>
/// Simple constant gravity implementation for typical 2D/3D games.
/// Educational note: Models uniform gravitational field (valid near planetary surface).
/// Academic reference: Classical mechanics, uniform field approximation.
/// Performance: O(n) - scales linearly with number of bodies.
/// </summary>
public class ConstantGravityModule : IGravityModule
{
    private Vector3D<float> _gravityVector;

    /// <summary>
    /// Module name for debugging and configuration.
    /// </summary>
    public string Name => "Constant Gravity";

    /// <summary>
    /// Creates a constant gravity module with specified gravity vector.
    /// </summary>
    /// <param name="gravityVector">Gravity acceleration vector (e.g., (0, -9.81, 0) for Earth)</param>
    public ConstantGravityModule(Vector3D<float> gravityVector)
    {
        _gravityVector = gravityVector;
    }

    /// <summary>
    /// Creates a constant gravity module with default Earth gravity.
    /// Educational note: Standard Earth gravity is 9.81 m/s² downward.
    /// </summary>
    public ConstantGravityModule() : this(new Vector3D<float>(0, -9.81f, 0))
    {
    }

    /// <summary>
    /// Applies constant gravitational acceleration to all dynamic bodies.
    /// Educational note: F = mg where g is constant acceleration vector.
    /// This is valid near Earth's surface where gravitational field is approximately uniform.
    /// </summary>
    /// <param name="bodies">All rigid bodies that can be affected by gravity</param>
    /// <param name="deltaTime">Time step for force integration</param>
    public void ApplyGravity(IReadOnlyList<IRigidBody> bodies, float deltaTime)
    {
        foreach (var body in bodies)
        {
            // Skip static bodies (infinite mass) and bodies that opt out of gravity
            if (body.IsStatic || !body.UseGravity)
                continue;

            // F = mg (Newton's second law with constant gravitational field)
            // Educational note: Gravitational force is proportional to mass
            // This means all objects fall at the same rate regardless of mass (Galileo's insight)
            var gravitationalForce = _gravityVector * body.Mass;
            body.AddForce(gravitationalForce);
        }
    }

    /// <summary>
    /// Sets the gravity vector for this module.
    /// Educational note: Allows runtime changes to gravity direction and magnitude.
    /// </summary>
    /// <param name="gravityVector">New gravity acceleration vector</param>
    public void SetGravity(Vector3D<float> gravityVector)
    {
        _gravityVector = gravityVector;
    }

    /// <summary>
    /// Gets the current gravity vector.
    /// </summary>
    /// <returns>Current gravity acceleration vector</returns>
    public Vector3D<float> GetGravity() => _gravityVector;

    /// <summary>
    /// Initializes the constant gravity module.
    /// </summary>
    /// <param name="world">Physics world (not needed for constant gravity)</param>
    public void Initialize(IPhysicsWorld world)
    {
        // No special initialization required for constant gravity
    }

    /// <summary>
    /// Disposes the constant gravity module.
    /// </summary>
    public void Dispose()
    {
        // No resources to dispose for constant gravity
    }
}