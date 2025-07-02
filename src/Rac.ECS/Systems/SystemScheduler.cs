using Rac.ECS.Core;

namespace Rac.ECS.Systems;

/// <summary>
/// Holds and runs a collection of ECS systems each frame with lifecycle and dependency management.
/// </summary>
/// <remarks>
/// The SystemScheduler manages the complete lifecycle of ECS systems:
/// 
/// SYSTEM LIFECYCLE:
/// 1. **Registration**: Systems are added and dependency order is resolved
/// 2. **Initialization**: Initialize(IWorld) is called once per system
/// 3. **Execution**: Update(float) is called each frame in dependency order
/// 4. **Shutdown**: Shutdown(IWorld) is called when systems are removed
/// 
/// DEPENDENCY MANAGEMENT:
/// - Systems can declare dependencies using [RunAfter(typeof(OtherSystem))] attributes
/// - Automatic topological sorting ensures systems run in dependency order
/// - Circular dependency detection prevents infinite loops
/// - Systems without dependencies run first, followed by dependent systems
/// 
/// EDUCATIONAL NOTES:
/// - Dependency injection and lifecycle management are common in enterprise frameworks
/// - Topological sorting is used in build systems, task schedulers, and module loaders
/// - Proper lifecycle management prevents resource leaks and initialization order issues
/// - This pattern enables modular system design with clear execution contracts
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - Dependency resolution occurs only during system registration/removal
/// - Runtime execution follows pre-computed order for optimal performance
/// - Batch lifecycle operations (initialization, shutdown) for efficiency
/// </remarks>
public sealed class SystemScheduler
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SCHEDULER STATE AND CONFIGURATION
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly List<ISystem> _systems = new();
    private readonly List<ISystem> _orderedSystems = new();
    private readonly IWorld _world;
    private bool _isDirty = false;

    /// <summary>
    /// Initializes a new SystemScheduler with access to the ECS world.
    /// Systems will receive the world instance during initialization and shutdown.
    /// </summary>
    /// <param name="world">The ECS world for system lifecycle operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when world is null.</exception>
    public SystemScheduler(IWorld world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SYSTEM REGISTRATION AND LIFECYCLE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Registers a new system to be updated each frame.
    /// Calls Initialize() if world is available and resolves system dependencies.
    /// </summary>
    /// <param name="system">The ECS system to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when system is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when adding the system would create circular dependencies.</exception>
    public void Add(ISystem system)
    {
        if (system == null)
            throw new ArgumentNullException(nameof(system));

        _systems.Add(system);
        _isDirty = true;

        // Initialize the system
        system.Initialize(_world);

        // Resolve dependencies and rebuild execution order
        ResolveDependencies();
    }

    /// <summary>
    /// Registers multiple systems in a single batch operation.
    /// More efficient than calling Add() multiple times.
    /// </summary>
    /// <param name="systems">The systems to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when systems collection or any system is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when adding the systems would create circular dependencies.</exception>
    public void AddSystems(params ISystem[] systems)
    {
        if (systems == null)
            throw new ArgumentNullException(nameof(systems));

        foreach (var system in systems)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system), "All systems must be non-null");

            _systems.Add(system);

            // Initialize the system
            system.Initialize(_world);
        }

        _isDirty = true;
        ResolveDependencies();
    }

    /// <summary>
    /// Unregisters a system so it no longer runs.
    /// Calls Shutdown() if world is available.
    /// </summary>
    /// <param name="system">The ECS system to remove.</param>
    /// <returns>True if the system was found and removed.</returns>
    public bool Remove(ISystem system)
    {
        if (system == null)
            return false;

        var removed = _systems.Remove(system);
        if (removed)
        {
            // Shutdown the system
            system.Shutdown(_world);

            _isDirty = true;
            ResolveDependencies();
        }

        return removed;
    }

    /// <summary>
    /// Clears all registered systems.
    /// Calls Shutdown() on all systems.
    /// </summary>
    public void Clear()
    {
        // Shutdown all systems in reverse order
        for (int i = _orderedSystems.Count - 1; i >= 0; i--)
        {
            _orderedSystems[i].Shutdown(_world);
        }

        _systems.Clear();
        _orderedSystems.Clear();
        _isDirty = false;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SYSTEM EXECUTION AND UPDATE LOOP
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calls <see cref="ISystem.Update" /> on each registered system in dependency order.
    /// </summary>
    /// <param name="delta">Elapsed time in seconds since the last update.</param>
    public void Update(float delta)
    {
        // Ensure dependencies are resolved before updating
        if (_isDirty)
        {
            ResolveDependencies();
        }

        foreach (var system in _orderedSystems)
        {
            system.Update(delta);
        }
    }

    /// <summary>
    /// Calls <see cref="ISystem.Update" /> on each system in the specified order.
    /// This method bypasses dependency resolution and uses the provided order.
    /// </summary>
    /// <param name="delta">Elapsed time in seconds since the last update.</param>
    /// <param name="systems">Systems to update in the specified order.</param>
    public void Update(float delta, IEnumerable<ISystem> systems)
    {
        foreach (var system in systems)
        {
            system.Update(delta);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCY RESOLUTION AND INTERNAL MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Resolves system dependencies and rebuilds the execution order.
    /// Uses topological sorting to handle RunAfter attribute dependencies.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when circular dependencies are detected.</exception>
    private void ResolveDependencies()
    {
        if (!_isDirty || _systems.Count == 0)
        {
            _isDirty = false;
            return;
        }

        try
        {
            _orderedSystems.Clear();
            _orderedSystems.AddRange(SystemDependencyResolver.ResolveDependencies(_systems));
            _isDirty = false;
        }
        catch (InvalidOperationException ex)
        {
            // Re-throw with additional context about system scheduler
            throw new InvalidOperationException(
                "Failed to resolve system dependencies in SystemScheduler. " + ex.Message,
                ex);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC PROPERTIES AND INSPECTION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the number of registered systems.
    /// </summary>
    public int Count => _systems.Count;

    /// <summary>
    /// Gets the registered systems in their current execution order.
    /// If dependencies are dirty, resolves them first.
    /// </summary>
    /// <returns>Systems in dependency-resolved execution order.</returns>
    public IReadOnlyList<ISystem> GetExecutionOrder()
    {
        if (_isDirty)
        {
            ResolveDependencies();
        }
        return _orderedSystems.AsReadOnly();
    }

    /// <summary>
    /// Gets all registered systems in registration order (not execution order).
    /// </summary>
    /// <returns>Systems in the order they were registered.</returns>
    public IReadOnlyList<ISystem> GetRegisteredSystems()
    {
        return _systems.AsReadOnly();
    }
}
