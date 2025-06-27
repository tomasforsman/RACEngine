using Rac.Physics.Modules;

namespace Rac.Physics.Modules.Fluid;

// ═══════════════════════════════════════════════════════════════
// FLUID MODULES - WEEK 9-10 FOUNDATION
// Educational note: Simple fluid interactions for basic physics systems
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// No drag module for vacuum or frictionless environments.
/// Educational note: Perfect for space games or idealized physics simulations.
/// Examples: Space shooters, physics puzzles, scenarios requiring perfect conservation of momentum.
/// </summary>
public class NoDragModule : IFluidModule
{
    /// <summary>
    /// Module name for debugging and configuration.
    /// </summary>
    public string Name => "No Drag";

    /// <summary>
    /// Applies no fluid drag forces (no-op implementation).
    /// Educational note: In a perfect vacuum, there's no fluid resistance.
    /// This represents Newton's first law in action - objects maintain velocity without external forces.
    /// </summary>
    /// <param name="bodies">All rigid bodies (ignored)</param>
    /// <param name="deltaTime">Time step (ignored)</param>
    public void ApplyFluidDrag(IReadOnlyList<IRigidBody> bodies, float deltaTime)
    {
        // No drag forces applied - bodies maintain their velocities
        // Educational note: This is ideal for space environments or theoretical physics simulations
    }

    /// <summary>
    /// Applies no buoyancy forces (no-op implementation).
    /// Educational note: No fluid means no buoyancy effects.
    /// </summary>
    /// <param name="bodies">All rigid bodies (ignored)</param>
    /// <param name="deltaTime">Time step (ignored)</param>
    public void ApplyBuoyancy(IReadOnlyList<IRigidBody> bodies, float deltaTime)
    {
        // No buoyancy forces applied - no fluid environment exists
        // Educational note: Archimedes' principle doesn't apply without a fluid medium
    }

    /// <summary>
    /// Initializes the no drag module (no-op).
    /// </summary>
    /// <param name="world">Physics world (not needed for no drag)</param>
    public void Initialize(IPhysicsWorld world)
    {
        // No initialization required for no drag environment
    }

    /// <summary>
    /// Disposes the no drag module (no-op).
    /// </summary>
    public void Dispose()
    {
        // No resources to dispose for no drag environment
    }
}