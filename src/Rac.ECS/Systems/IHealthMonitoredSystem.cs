
using Rac.ECS.Systems;

/// <summary>
/// Extended ISystem interface that adds health monitoring and error resilience capabilities.
///
/// PROGRESSIVE COMPLEXITY DESIGN:
/// - Existing ISystem implementations continue to work (backward compatible)
/// - Systems can opt-in to health monitoring by implementing IHealthMonitoredSystem
/// - Advanced systems can provide custom health logic and recovery mechanisms
///
/// EDUCATIONAL BENEFITS:
/// - Demonstrates interface segregation principle (ISP)
/// - Shows how to extend existing interfaces without breaking changes
/// - Teaches robust system design patterns from distributed systems
/// </summary>
public interface IHealthMonitoredSystem : ISystem
{
    /// <summary>
    /// Checks if the system can safely perform an update operation.
    /// Called by SystemScheduler before each Update() to prevent failures.
    /// </summary>
    /// <returns>True if system is ready for update, false if update should be skipped</returns>
    /// <remarks>
    /// PERFORMANCE CONSIDERATIONS:
    /// - This method is called every frame, so keep it lightweight (< 0.1ms)
    /// - Avoid expensive operations like file I/O or network calls
    /// - Cache results when possible and refresh only periodically
    ///
    /// COMMON HEALTH CHECK PATTERNS:
    /// - Memory usage within acceptable limits
    /// - Required resources are available and accessible
    /// - No critical exceptions in recent update cycles
    /// - Performance metrics within normal ranges
    ///
    /// IMPLEMENTATION GUIDELINES:
    /// - Return false conservatively - better to skip one frame than crash
    /// - Log reasons for health check failures for debugging
    /// - Consider system dependencies (e.g., rendering needs graphics context)
    /// </remarks>
    /// <example>
    /// <code>
    /// public bool CanUpdate()
    /// {
    ///     // Check memory usage
    ///     if (GC.GetTotalMemory(false) > _memoryThreshold)
    ///         return false;
    ///
    ///     // Check recent failure rate
    ///     if (_recentFailures > 3)
    ///         return false;
    ///
    ///     // Check required dependencies
    ///     return _renderer?.IsInitialized == true;
    /// }
    /// </code>
    /// </example>
    bool CanUpdate();

    /// <summary>
    /// Gets the current health state and diagnostic information for the system.
    /// Used by monitoring tools, debuggers, and system administrators.
    /// </summary>
    /// <returns>Comprehensive health information including state, metrics, and diagnostics</returns>
    /// <remarks>
    /// MONITORING AND DIAGNOSTICS:
    /// - Provides real-time insight into system performance and health
    /// - Enables proactive identification of potential issues
    /// - Supports automated monitoring and alerting systems
    /// - Helps with capacity planning and performance optimization
    ///
    /// INFORMATION TO INCLUDE:
    /// - Current operational state (healthy, degraded, critical, failed)
    /// - Performance metrics (average update time, memory usage, etc.)
    /// - Error information (last exception, failure count, etc.)
    /// - Resource status (dependencies, external connections, etc.)
    /// - Diagnostic messages for troubleshooting
    /// </remarks>
    /// <example>
    /// <code>
    /// public SystemHealthInfo GetHealthInfo()
    /// {
    ///     var state = _recentFailures > 5 ? SystemState.Critical :
    ///                 _recentFailures > 2 ? SystemState.Degraded :
    ///                 SystemState.Healthy;
    ///
    ///     return new SystemHealthInfo(
    ///         state,
    ///         _lastUpdateTime,
    ///         _recentFailures,
    ///         _averageUpdateTime,
    ///         _lastException?.Message,
    ///         $"Processed {_entitiesProcessed} entities last update"
    ///     );
    /// }
    /// </code>
    /// </example>
    SystemHealthInfo GetHealthInfo();

    /// <summary>
    /// Attempts to recover the system from a failed or degraded state.
    /// Called by SystemScheduler when automatic recovery is enabled.
    /// </summary>
    /// <returns>True if recovery was successful and system can resume normal operation</returns>
    /// <remarks>
    /// RECOVERY STRATEGIES:
    /// - Clear cached data that might be corrupted
    /// - Reset internal state to known good values
    /// - Reconnect to external resources or dependencies
    /// - Reduce operational scope (e.g., process fewer entities)
    /// - Perform garbage collection to free memory
    ///
    /// RECOVERY GUIDELINES:
    /// - Keep recovery operations lightweight and fast
    /// - Don't attempt recovery too frequently (implement backoff)
    /// - Log recovery attempts and outcomes for analysis
    /// - Have multiple levels of recovery (soft reset, hard reset, etc.)
    /// - Know when to give up and remain in failed state
    ///
    /// EDUCATIONAL NOTES:
    /// - Recovery patterns are common in distributed systems and network programming
    /// - Shows resilience engineering principles applied to game engines
    /// - Demonstrates graceful degradation vs. complete failure
    /// </remarks>
    /// <example>
    /// <code>
    /// public bool TryRecover()
    /// {
    ///     try
    ///     {
    ///         // Clear potentially corrupted cache
    ///         _entityCache.Clear();
    ///
    ///         // Reset failure counters
    ///         _recentFailures = 0;
    ///         _lastException = null;
    ///
    ///         // Perform basic functionality test
    ///         return CanUpdate();
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         _logger.Error($"Recovery failed: {ex.Message}");
    ///         return false;
    ///     }
    /// }
    /// </code>
    /// </example>
    bool TryRecover();
}
