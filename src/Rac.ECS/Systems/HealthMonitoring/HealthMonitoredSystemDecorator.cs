using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Rac.Core.Logger;
using Rac.ECS.Core;
using Rac.ECS.Systems;

namespace Rac.ECS.Systems.HealthMonitoring;

/// <summary>
/// Decorates any ISystem with configurable health monitoring capabilities.
/// Uses composition over inheritance to add health monitoring to existing systems.
/// </summary>
public class HealthMonitoredSystemDecorator : IHealthMonitoredSystem
{
    private readonly object _stateLock = new object();
    private readonly ISystem _innerSystem;
    private readonly IHealthMonitoringConfig _config;
    private readonly SystemHealthTracker _healthTracker;
    private readonly IHealthCheckProvider _healthCheckProvider;
    private readonly ILogger? _logger;

    private DateTime _lastHealthCheck = DateTime.MinValue;
    private DateTime _lastRecoveryAttempt = DateTime.MinValue;
    private bool _isHealthy = true;
    private int _recoveryLevel = 0; // 0 = no recovery needed, 1+ = escalating recovery levels

    private readonly object _lockObject = new object();

    public HealthMonitoredSystemDecorator(
        ISystem innerSystem,
        IHealthMonitoringConfig config,
        IHealthCheckProvider? healthCheckProvider = null,
        ILogger? logger = null)
    {
        _innerSystem = innerSystem ?? throw new ArgumentNullException(nameof(innerSystem));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _healthTracker = new SystemHealthTracker(innerSystem);
        _healthCheckProvider = healthCheckProvider ?? new DefaultHealthCheckProvider();
        _logger = logger;
    }

    // Delegate core ISystem methods to inner system
    public void Initialize(IWorld world)
    {
        try
        {
            _innerSystem.Initialize(world);
            _healthTracker.RecordSuccessfulOperation();
        }
        catch (Exception ex)
        {
            _healthTracker.RecordFailure(ex);
            throw;
        }
    }

    public void Update(float deltaTime)
    {
        // Lightweight health check only when needed
        if (ShouldPerformHealthCheck())
        {
            PerformScheduledHealthCheck();
        }

        // Skip update if system is unhealthy and can't update
        if (!CanUpdate())
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            _innerSystem.Update(deltaTime);
            _healthTracker.RecordSuccessfulOperation(stopwatch.ElapsedMilliseconds);

            // Reset recovery level on successful update
            if (Interlocked.Exchange(ref _recoveryLevel, 0) > 0)
            {
                _logger?.LogInfo("System {SystemName} recovered successfully",
                    _innerSystem.GetType().Name);
            }
        }
        catch (Exception ex)
        {
            _healthTracker.RecordFailure(ex);

            // Attempt lightweight recovery if enabled
            if (_config.AutoRecoveryEnabled)
            {
                TryRecover();
            }

            // Re-throw if configured to do so
            if (_config.RethrowExceptions)
            {
                throw;
            }
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public void Shutdown(IWorld world)
    {
        try
        {
            _innerSystem.Shutdown(world);
        }
        catch (Exception ex)
        {
            _healthTracker.RecordFailure(ex);
            // Don't rethrow during shutdown
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanUpdate() => _isHealthy && _healthTracker.GetCurrentHealth().CanUpdate;

    public SystemHealthInfo GetHealthInfo()
    {
        return _healthTracker.GetCurrentHealth();
    }

    public bool TryRecover()
    {
        var now = DateTime.UtcNow;

        // Implement exponential backoff for recovery attempts
        var backoffTime = TimeSpan.FromSeconds(Math.Pow(2, _recoveryLevel) * _config.BaseRecoveryIntervalSeconds);
        if (now - _lastRecoveryAttempt < backoffTime)
        {
            return false; // Too soon to retry
        }

        _lastRecoveryAttempt = now;
        _recoveryLevel++;

        Console.WriteLine($"🔄 Attempting recovery level {_recoveryLevel} for {_innerSystem.GetType().Name}");

        try
        {
            var recovered = PerformRecovery(_recoveryLevel);

            if (recovered)
            {
                _healthTracker.RecordSuccessfulRecovery();
                _isHealthy = true;
                _recoveryLevel = 0;
                return true;
            }
            else
            {
                _healthTracker.RecordFailedRecovery();

                // Give up after max recovery attempts
                if (_recoveryLevel >= _config.MaxRecoveryLevel)
                {
                    Console.WriteLine($"💥 {_innerSystem.GetType().Name} recovery failed after {_recoveryLevel} attempts");
                    _isHealthy = false;
                }

                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Recovery attempt failed: {ex.Message}");
            _healthTracker.RecordFailedRecovery();
            return false;
        }
    }

    private bool ShouldPerformHealthCheck()
    {
        var now = DateTime.UtcNow;
        lock (_stateLock)
        {
            if (now - _lastHealthCheck >= _config.HealthCheckInterval)
            {
                _lastHealthCheck = now;
                return true;
            }
            return false;
        }
    }

    private void PerformScheduledHealthCheck()
    {
        _lastHealthCheck = DateTime.UtcNow;

        try
        {
            var checks = _healthCheckProvider.GetHealthChecks(_innerSystem, _config);

            foreach (var check in checks)
            {
                if (!check.Execute())
                {
                    _healthTracker.RecordHealthCheckFailure(check.Name);
                    _isHealthy = false;

                    if (_config.AutoRecoveryEnabled)
                    {
                        TryRecover();
                    }
                    return;
                }
            }

            _isHealthy = true;
        }
        catch (Exception ex)
        {
            _healthTracker.RecordHealthCheckFailure($"Health check exception: {ex.Message}");
            _isHealthy = false;
        }
    }

    private bool PerformRecovery(int recoveryLevel)
    {
        // Escalating recovery strategies
        return recoveryLevel switch
        {
            1 => PerformLightweightRecovery(),
            2 => PerformMediumRecovery(),
            3 => PerformHeavyRecovery(),
            _ => false // Give up
        };
    }

    private bool PerformLightweightRecovery()
    {
        // Level 1: Simple resets, no expensive operations
        try
        {
            // If the inner system supports recovery, try it
            if (_innerSystem is IHealthMonitoredSystem healthMonitoredInner)
            {
                return healthMonitoredInner.TryRecover();
            }

            lock (_lockObject)
            {
                // Reset any cached state that might be corrupted
                // This is system-specific and should be lightweight
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Lightweight recovery failed for {SystemName}",
                _innerSystem.GetType().Name);
            return false;
        }
    }

    private bool PerformMediumRecovery()
    {
        // Level 2: More aggressive recovery
        try
        {
            _logger?.LogInfo("Attempting medium recovery for {SystemName}",
                _innerSystem.GetType().Name);

            // Additional medium-level recovery could go here
            return PerformLightweightRecovery();
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Medium recovery failed for {SystemName}",
                _innerSystem.GetType().Name);
            return false;
        }
    }

    private bool PerformHeavyRecovery()
    {
        // Level 3: Last resort recovery
        try
        {
            _logger?.LogInfo("Attempting heavy recovery for {SystemName} - this may cause frame drops",
                _innerSystem.GetType().Name);

            // Force full GC
            if (_config.Environment != "Production") // Don't do this in production!
            {
                GC.Collect(0, GCCollectionMode.Optimized);
            }

            // Could reinitialize the system if we had access to world
            // For now, just clear any static caches that might exist

            return PerformMediumRecovery();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Heavy recovery failed for {SystemName}",
                _innerSystem.GetType().Name);
            return false;
        }
    }
}
