using System;
using System.Collections.Generic;
using System.Linq;
using Rac.ECS.Core;
using Rac.ECS.Systems.HealthMonitoring;

namespace Rac.ECS.Systems;

/// <summary>
/// Holds and runs a collection of ECS systems each frame with lifecycle, dependency management, and health monitoring integration.
/// </summary>
/// <remarks>
/// The SystemScheduler manages the complete lifecycle of ECS systems with optional health monitoring:
///
/// SYSTEM LIFECYCLE:
/// 1. **Registration**: Systems are added and dependency order is resolved
/// 2. **Initialization**: Initialize(IWorld) is called once per system
/// 3. **Execution**: Update(float) is called each frame in dependency order with health checks
/// 4. **Shutdown**: Shutdown(IWorld) is called when systems are removed
///
/// HEALTH MONITORING INTEGRATION:
/// - Automatic health checks before system updates via CanUpdate()
/// - Optional automatic wrapping of systems with health monitoring
/// - Health status reporting and recovery management
/// - Configurable health monitoring per environment
///
/// DEPENDENCY MANAGEMENT:
/// - Systems can declare dependencies using [RunAfter(typeof(OtherSystem))] attributes
/// - Automatic topological sorting ensures systems run in dependency order
/// - Circular dependency detection prevents infinite loops
/// - Systems without dependencies run first, followed by dependent systems
///
/// EDUCATIONAL NOTES:
/// - Demonstrates composition of cross-cutting concerns (health monitoring) with core functionality
/// - Shows how to integrate optional features without breaking existing APIs
/// - Health monitoring integration follows open/closed principle - scheduler is open for extension
/// </remarks>
public sealed class SystemScheduler
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SCHEDULER STATE AND CONFIGURATION
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly List<ISystem> _systems = new();
    private readonly List<ISystem> _orderedSystems = new();
    private readonly IWorld _world;
    private readonly HealthMonitoringSchedulerConfig _healthConfig;
    private bool _isDirty = false;

    // Health monitoring state
    private readonly Dictionary<ISystem, DateTime> _lastHealthCheck = new();
    private readonly Dictionary<ISystem, int> _skippedUpdates = new();
    private int _totalUpdateCycles = 0;
    private int _healthCheckCycles = 0;

    /// <summary>
    /// Initializes a new SystemScheduler with access to the ECS world.
    /// Systems will receive the world instance during initialization and shutdown.
    /// </summary>
    /// <param name="world">The ECS world for system lifecycle operations.</param>
    /// <param name="healthConfig">Optional health monitoring configuration for automatic system wrapping.</param>
    /// <exception cref="ArgumentNullException">Thrown when world is null.</exception>
    public SystemScheduler(IWorld world, HealthMonitoringSchedulerConfig? healthConfig = null)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _healthConfig = healthConfig ?? HealthMonitoringSchedulerConfig.Disabled;
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

        // Initialize health monitoring tracking
        InitializeHealthTracking(system);

        // Resolve dependencies and rebuild execution order
        ResolveDependencies();
    }

    /// <summary>
    /// Registers a new system with automatic health monitoring applied.
    /// </summary>
    /// <param name="system">The ECS system to add with health monitoring.</param>
    /// <param name="environment">Health monitoring environment (Development, Testing, Production, etc.)</param>
    /// <exception cref="ArgumentNullException">Thrown when system is null.</exception>
    public void AddWithHealthMonitoring(ISystem system, string environment = "Production")
    {
        if (system == null)
            throw new ArgumentNullException(nameof(system));

        // Wrap system with health monitoring if not already monitored
        var monitoredSystem = system is IHealthMonitoredSystem
            ? system
            : system.WithHealthMonitoring(environment);

        Add(monitoredSystem);
    }

    /// <summary>
    /// Registers a new system with custom health monitoring configuration.
    /// </summary>
    /// <param name="system">The ECS system to add with health monitoring.</param>
    /// <param name="configure">Action to configure health monitoring.</param>
    /// <exception cref="ArgumentNullException">Thrown when system is null.</exception>
    public void AddWithHealthMonitoring(ISystem system, Action<HealthMonitoringConfigBuilder> configure)
    {
        if (system == null)
            throw new ArgumentNullException(nameof(system));

        var monitoredSystem = system is IHealthMonitoredSystem
            ? system
            : system.WithHealthMonitoring(configure);

        Add(monitoredSystem);
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

            // Initialize health monitoring tracking
            InitializeHealthTracking(system);
        }

        _isDirty = true;
        ResolveDependencies();
    }

    /// <summary>
    /// Registers multiple systems with automatic health monitoring applied.
    /// </summary>
    /// <param name="environment">Health monitoring environment for all systems.</param>
    /// <param name="systems">The systems to add with health monitoring.</param>
    public void AddSystemsWithHealthMonitoring(string environment, params ISystem[] systems)
    {
        if (systems == null)
            throw new ArgumentNullException(nameof(systems));

        var monitoredSystems = systems.Select(system =>
            system is IHealthMonitoredSystem ? system : system.WithHealthMonitoring(environment)
        ).ToArray();

        AddSystems(monitoredSystems);
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

            // Clean up health tracking
            CleanupHealthTracking(system);

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
        _lastHealthCheck.Clear();
        _skippedUpdates.Clear();
        _isDirty = false;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SYSTEM EXECUTION AND UPDATE LOOP WITH HEALTH MONITORING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calls Update() on each registered system in dependency order with health monitoring integration.
    /// Health-monitored systems are checked via CanUpdate() before execution.
    /// </summary>
    /// <param name="delta">Elapsed time in seconds since the last update.</param>
    public void Update(float delta)
    {
        // Ensure dependencies are resolved before updating
        if (_isDirty)
        {
            ResolveDependencies();
        }

        _totalUpdateCycles++;

        foreach (var system in _orderedSystems)
        {
            // Health monitoring integration
            if (system is IHealthMonitoredSystem healthSystem)
            {
                UpdateHealthMonitoredSystem(healthSystem, delta);
            }
            else
            {
                // Standard system update
                system.Update(delta);
            }
        }

        // Periodic health reporting
        if (_healthConfig.EnableHealthReporting && _totalUpdateCycles % _healthConfig.HealthReportingInterval == 0)
        {
            ReportSystemHealth();
        }
    }

    /// <summary>
    /// Updates a health-monitored system with health checks and recovery.
    /// </summary>
    private void UpdateHealthMonitoredSystem(IHealthMonitoredSystem healthSystem, float delta)
    {
        try
        {
            // Check if system can update
            if (healthSystem.CanUpdate())
            {
                healthSystem.Update(delta);

                // Reset skip counter on successful update
                _skippedUpdates[healthSystem] = 0;
            }
            else
            {
                // Track skipped updates
                _skippedUpdates[healthSystem] = _skippedUpdates.GetValueOrDefault(healthSystem, 0) + 1;

                // Attempt recovery if configured and skip threshold reached
                if (_healthConfig.EnableAutoRecovery &&
                    _skippedUpdates[healthSystem] >= _healthConfig.MaxSkippedUpdatesBeforeRecovery)
                {
                    AttemptSystemRecovery(healthSystem);
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception and attempt recovery if configured
            if (_healthConfig.EnableAutoRecovery)
            {
                Console.WriteLine($"⚠️ System {healthSystem.GetType().Name} threw exception: {ex.Message}");
                AttemptSystemRecovery(healthSystem);
            }
            else
            {
                throw; // Re-throw if auto-recovery is disabled
            }
        }
    }

    /// <summary>
    /// Attempts to recover a failed health-monitored system.
    /// </summary>
    private void AttemptSystemRecovery(IHealthMonitoredSystem healthSystem)
    {
        try
        {
            if (healthSystem.TryRecover())
            {
                Console.WriteLine($"✅ Successfully recovered system {healthSystem.GetType().Name}");
                _skippedUpdates[healthSystem] = 0;
            }
            else
            {
                Console.WriteLine($"❌ Failed to recover system {healthSystem.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Recovery attempt failed for {healthSystem.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Calls Update() on each system in the specified order.
    /// This method bypasses dependency resolution and health monitoring.
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
    // HEALTH MONITORING AND REPORTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets health information for all health-monitored systems.
    /// </summary>
    /// <returns>Collection of system health reports.</returns>
    public IEnumerable<SystemHealthReport> GetSystemHealthReports()
    {
        foreach (var system in _orderedSystems)
        {
            if (system is IHealthMonitoredSystem healthSystem)
            {
                var healthInfo = healthSystem.GetHealthInfo();
                var skippedCount = _skippedUpdates.GetValueOrDefault(system, 0);

                yield return new SystemHealthReport(
                    system.GetType().Name,
                    healthInfo,
                    skippedCount,
                    system
                );
            }
            else
            {
                // Non-monitored systems are assumed healthy
                yield return new SystemHealthReport(
                    system.GetType().Name,
                    new SystemHealthInfo(), // Default healthy state
                    0,
                    system
                );
            }
        }
    }

    /// <summary>
    /// Gets health information for a specific system.
    /// </summary>
    /// <param name="system">The system to get health information for.</param>
    /// <returns>Health report for the system, or null if system not found.</returns>
    public SystemHealthReport? GetSystemHealthReport(ISystem system)
    {
        if (!_systems.Contains(system))
            return null;

        if (system is IHealthMonitoredSystem healthSystem)
        {
            var healthInfo = healthSystem.GetHealthInfo();
            var skippedCount = _skippedUpdates.GetValueOrDefault(system, 0);

            return new SystemHealthReport(
                system.GetType().Name,
                healthInfo,
                skippedCount,
                system
            );
        }

        return new SystemHealthReport(
            system.GetType().Name,
            new SystemHealthInfo(),
            0,
            system
        );
    }

    /// <summary>
    /// Reports health status of all systems to console.
    /// </summary>
    private void ReportSystemHealth()
    {
        _healthCheckCycles++;

        var healthReports = GetSystemHealthReports().ToList();
        var healthyCount = healthReports.Count(r => r.HealthInfo.State == SystemState.Healthy);
        var degradedCount = healthReports.Count(r => r.HealthInfo.State == SystemState.Degraded);
        var criticalCount = healthReports.Count(r => r.HealthInfo.State == SystemState.Critical);
        var failedCount = healthReports.Count(r => r.HealthInfo.State == SystemState.Failed);

        Console.WriteLine($"📊 System Health Report (Cycle {_healthCheckCycles}):");
        Console.WriteLine($"   ✅ Healthy: {healthyCount}, ⚠️ Degraded: {degradedCount}, 🔶 Critical: {criticalCount}, 🚨 Failed: {failedCount}");

        // Report problematic systems
        foreach (var report in healthReports.Where(r => r.HealthInfo.NeedsAttention))
        {
            var icon = report.HealthInfo.State switch
            {
                SystemState.Degraded => "⚠️",
                SystemState.Critical => "🔶",
                SystemState.Failed => "🚨",
                _ => "❓"
            };

            Console.WriteLine($"   {icon} {report.SystemName}: {report.HealthInfo.StatusDescription}");
            if (report.SkippedUpdates > 0)
            {
                Console.WriteLine($"      Skipped updates: {report.SkippedUpdates}");
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HEALTH TRACKING HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    private void InitializeHealthTracking(ISystem system)
    {
        _lastHealthCheck[system] = DateTime.UtcNow;
        _skippedUpdates[system] = 0;
    }

    private void CleanupHealthTracking(ISystem system)
    {
        _lastHealthCheck.Remove(system);
        _skippedUpdates.Remove(system);
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

    /// <summary>
    /// Gets the number of health-monitored systems.
    /// </summary>
    public int HealthMonitoredSystemCount => _systems.Count(s => s is IHealthMonitoredSystem);

    /// <summary>
    /// Gets the total number of update cycles executed.
    /// </summary>
    public int TotalUpdateCycles => _totalUpdateCycles;

    /// <summary>
    /// Gets the current health configuration for the scheduler.
    /// </summary>
    public HealthMonitoringSchedulerConfig HealthConfig => _healthConfig;
}

/// <summary>
/// Configuration for health monitoring integration in SystemScheduler.
/// </summary>
public class HealthMonitoringSchedulerConfig
{
    /// <summary>Whether automatic recovery should be attempted on system failures.</summary>
    public bool EnableAutoRecovery { get; init; } = true;

    /// <summary>Whether periodic health reporting should be enabled.</summary>
    public bool EnableHealthReporting { get; init; } = true;

    /// <summary>Interval (in update cycles) between health reports.</summary>
    public int HealthReportingInterval { get; init; } = 300; // Every 5 seconds at 60 FPS

    /// <summary>Maximum number of skipped updates before attempting recovery.</summary>
    public int MaxSkippedUpdatesBeforeRecovery { get; init; } = 10;

    /// <summary>Disabled health monitoring configuration.</summary>
    public static HealthMonitoringSchedulerConfig Disabled => new()
    {
        EnableAutoRecovery = false,
        EnableHealthReporting = false,
        HealthReportingInterval = int.MaxValue,
        MaxSkippedUpdatesBeforeRecovery = int.MaxValue
    };

    /// <summary>Development-optimized configuration with frequent reporting.</summary>
    public static HealthMonitoringSchedulerConfig Development => new()
    {
        EnableAutoRecovery = true,
        EnableHealthReporting = true,
        HealthReportingInterval = 180, // Every 3 seconds at 60 FPS
        MaxSkippedUpdatesBeforeRecovery = 5
    };

    /// <summary>Production-optimized configuration with minimal overhead.</summary>
    public static HealthMonitoringSchedulerConfig Production => new()
    {
        EnableAutoRecovery = true,
        EnableHealthReporting = true,
        HealthReportingInterval = 1800, // Every 30 seconds at 60 FPS
        MaxSkippedUpdatesBeforeRecovery = 30
    };
}

/// <summary>
/// Comprehensive health report for a system managed by SystemScheduler.
/// </summary>
/// <param name="SystemName">Name of the system type.</param>
/// <param name="HealthInfo">Current health information from the system.</param>
/// <param name="SkippedUpdates">Number of consecutive updates skipped due to health issues.</param>
/// <param name="System">Reference to the actual system instance.</param>
public readonly record struct SystemHealthReport(
    string SystemName,
    SystemHealthInfo HealthInfo,
    int SkippedUpdates,
    ISystem System
);
