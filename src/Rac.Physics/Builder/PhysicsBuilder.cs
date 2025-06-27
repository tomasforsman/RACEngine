using Rac.Physics.Core;
using Rac.Physics.Modules;
using Rac.Physics.Modules.Gravity;
using Rac.Physics.Modules.Collision;
using Rac.Physics.Modules.Fluid;
using Rac.Physics.Services;
using Silk.NET.Maths;

namespace Rac.Physics.Builder;

// ═══════════════════════════════════════════════════════════════
// PHYSICS BUILDER API - WEEK 9-10 FOUNDATION
// Educational note: Fluent builder pattern for creating physics services
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Fluent builder for creating modular physics services.
/// Educational note: Builder pattern enables complex object construction with validation.
/// Follows the Gang of Four Builder pattern for constructing complex physics configurations.
/// </summary>
public class PhysicsBuilder
{
    private IGravityModule? _gravityModule;
    private ICollisionModule? _collisionModule;
    private IFluidModule? _fluidModule;

    /// <summary>
    /// Creates new physics builder with default configuration.
    /// Educational note: Factory method provides clean starting point for configuration.
    /// </summary>
    public static PhysicsBuilder Create() => new PhysicsBuilder();

    /// <summary>
    /// Private constructor enforces use of Create() factory method.
    /// Educational note: Encapsulates object creation and ensures proper initialization.
    /// </summary>
    private PhysicsBuilder() { }

    /// <summary>
    /// Configures gravity module for physics simulation.
    /// Educational note: Allows custom gravity implementations for different game types.
    /// </summary>
    /// <param name="gravityModule">Custom gravity module implementation</param>
    /// <returns>Builder instance for method chaining</returns>
    public PhysicsBuilder WithGravity(IGravityModule gravityModule)
    {
        _gravityModule = gravityModule ?? throw new ArgumentNullException(nameof(gravityModule));
        return this;
    }

    /// <summary>
    /// Convenience method for common gravity types.
    /// Educational note: Provides easy access to standard gravity implementations.
    /// </summary>
    /// <param name="type">Type of gravity to apply</param>
    /// <param name="gravity">Gravity vector (defaults to Earth gravity)</param>
    /// <returns>Builder instance for method chaining</returns>
    public PhysicsBuilder WithGravity(GravityType type, Vector3D<float>? gravity = null)
    {
        gravity ??= new Vector3D<float>(0, -9.81f, 0); // Default Earth gravity

        _gravityModule = type switch
        {
            GravityType.None => new NoGravityModule(),
            GravityType.Constant => new ConstantGravityModule(gravity.Value),
            GravityType.Realistic => throw new NotImplementedException("Realistic gravity not implemented in Week 9-10 foundation"),
            GravityType.Planetary => throw new NotImplementedException("Planetary gravity not implemented in Week 9-10 foundation"),
            _ => throw new ArgumentException($"Unknown gravity type: {type}")
        };

        return this;
    }

    /// <summary>
    /// Configures collision detection module.
    /// Educational note: Separates collision strategy from physics service implementation.
    /// </summary>
    /// <param name="collisionModule">Custom collision module implementation</param>
    /// <returns>Builder instance for method chaining</returns>
    public PhysicsBuilder WithCollision(ICollisionModule collisionModule)
    {
        _collisionModule = collisionModule ?? throw new ArgumentNullException(nameof(collisionModule));
        return this;
    }

    /// <summary>
    /// Convenience method for common collision types.
    /// Educational note: Provides standard collision detection algorithms.
    /// </summary>
    /// <param name="type">Type of collision detection to use</param>
    /// <returns>Builder instance for method chaining</returns>
    public PhysicsBuilder WithCollision(CollisionType type)
    {
        _collisionModule = type switch
        {
            CollisionType.AABB => new SimpleAABBCollisionModule(),
            CollisionType.Bepu => throw new NotImplementedException("Bepu collision not implemented in Week 9-10 foundation"),
            CollisionType.Custom => throw new NotImplementedException("Custom collision requires specific implementation"),
            _ => throw new ArgumentException($"Unknown collision type: {type}")
        };

        return this;
    }

    /// <summary>
    /// Configures fluid dynamics module (optional).
    /// Educational note: Fluid effects are optional for many game types.
    /// </summary>
    /// <param name="fluidModule">Custom fluid module implementation</param>
    /// <returns>Builder instance for method chaining</returns>
    public PhysicsBuilder WithFluid(IFluidModule? fluidModule)
    {
        _fluidModule = fluidModule;
        return this;
    }

    /// <summary>
    /// Convenience method for common fluid types.
    /// Educational note: Provides standard fluid interaction models.
    /// </summary>
    /// <param name="type">Type of fluid interaction to use</param>
    /// <returns>Builder instance for method chaining</returns>
    public PhysicsBuilder WithFluid(FluidType type)
    {
        _fluidModule = type switch
        {
            FluidType.None => null,
            FluidType.LinearDrag => throw new NotImplementedException("Linear drag not implemented in Week 9-10 foundation"),
            FluidType.QuadraticDrag => throw new NotImplementedException("Quadratic drag not implemented in Week 9-10 foundation"),
            FluidType.Water => throw new NotImplementedException("Water simulation not implemented in Week 9-10 foundation"),
            FluidType.Air => throw new NotImplementedException("Air simulation not implemented in Week 9-10 foundation"),
            _ => throw new ArgumentException($"Unknown fluid type: {type}")
        };

        return this;
    }

    /// <summary>
    /// Builds physics service with validation.
    /// Educational note: Validates required modules and creates composed service.
    /// </summary>
    /// <returns>Configured physics service ready for use</returns>
    /// <exception cref="InvalidOperationException">Thrown if required modules are missing</exception>
    public IPhysicsService Build()
    {
        // Validate required modules
        if (_gravityModule == null)
            throw new InvalidOperationException("Gravity module is required. Use WithGravity() or WithGravity(GravityType.None)");

        if (_collisionModule == null)
            throw new InvalidOperationException("Collision module is required. Use WithCollision()");

        // Educational note: Fluid module is optional, many games don't need fluid physics
        return new ModularPhysicsService(_gravityModule, _collisionModule, _fluidModule);
    }
}

/// <summary>
/// Common physics configurations for different game types.
/// Educational note: Presets demonstrate typical module combinations and best practices.
/// These represent real-world physics configurations for different genres.
/// </summary>
public static class PhysicsPresets
{
    /// <summary>
    /// Top-down 2D physics suitable for roguelike shooters and strategy games.
    /// Educational note: No gravity, emphasis on raycasting and collision detection.
    /// Perfect for games like roguelike shooters where gravity would be unwanted.
    /// </summary>
    /// <returns>Physics service configured for top-down 2D gameplay</returns>
    public static IPhysicsService TopDown2D()
    {
        return PhysicsBuilder.Create()
            .WithGravity(GravityType.None)              // No gravity for top-down view
            .WithCollision(CollisionType.AABB)          // Fast AABB collision detection
            .WithFluid(FluidType.None)                  // No fluid effects needed
            .Build();
    }

    /// <summary>
    /// Fast 2D physics suitable for platformers and arcade games.
    /// Educational note: Constant gravity with fast collision detection.
    /// Prioritizes performance and predictable behavior over realism.
    /// </summary>
    /// <returns>Physics service configured for 2D platformer gameplay</returns>
    public static IPhysicsService Platformer2D()
    {
        return PhysicsBuilder.Create()
            .WithGravity(GravityType.Constant, new Vector3D<float>(0, -9.81f, 0))  // Earth gravity
            .WithCollision(CollisionType.AABB)          // Fast AABB collision detection
            .WithFluid(FluidType.None)                  // No fluid effects for simplicity
            .Build();
    }

    /// <summary>
    /// Null physics implementation for testing and headless scenarios.
    /// Educational note: Null object pattern enables testing without simulation overhead.
    /// Useful for unit tests, server-side logic, and headless applications.
    /// </summary>
    /// <returns>Null physics service that performs no operations</returns>
    public static IPhysicsService Headless()
    {
        return new NullPhysicsService();
    }

    /// <summary>
    /// Simple physics for debugging and development.
    /// Educational note: Minimal configuration for testing physics integration.
    /// Useful during development when you need basic physics without complexity.
    /// </summary>
    /// <returns>Physics service with minimal configuration</returns>
    public static IPhysicsService Debug()
    {
        return PhysicsBuilder.Create()
            .WithGravity(GravityType.Constant, new Vector3D<float>(0, -1.0f, 0))   // Reduced gravity for easier debugging
            .WithCollision(CollisionType.AABB)          // Simple collision detection
            .Build();
    }
}