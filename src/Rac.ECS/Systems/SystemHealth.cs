using System;

namespace Rac.ECS.Systems;

/// <summary>
/// Represents the operational state of a system for health monitoring and error recovery.
///
/// EDUCATIONAL NOTES:
/// System health monitoring is crucial in production game engines because:
/// - Games run for hours without restart opportunities
/// - System failures can cascade and crash the entire engine
/// - Performance degradation needs early detection
/// - Resource exhaustion must be caught before system-wide failure
///
/// HEALTH STATE PROGRESSION:
/// Healthy → Degraded → Critical → Failed → Recovering → Healthy
///
/// ACADEMIC REFERENCE:
/// This pattern follows "Circuit Breaker" and "Health Check" patterns from
/// "Release It!" by Michael T. Nygard, commonly used in distributed systems.
/// </summary>
public enum SystemState
{
    /// <summary>
    /// System is operating normally with no detected issues.
    /// All updates execute successfully within expected timeframes.
    /// </summary>
    Healthy = 0,

    /// <summary>
    /// System is functional but showing signs of performance degradation.
    /// May have occasional failures or slower than normal execution times.
    /// Updates continue but system should be monitored closely.
    /// </summary>
    Degraded = 1,

    /// <summary>
    /// System is experiencing significant issues but still attempting to operate.
    /// High failure rate or severe performance problems detected.
    /// Updates may be throttled or simplified to prevent total failure.
    /// </summary>
    Critical = 2,

    /// <summary>
    /// System has failed and cannot perform updates safely.
    /// Updates are skipped to prevent cascading failures to other systems.
    /// System remains in scheduler but is effectively disabled.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// System is attempting to recover from a failed state.
    /// May perform reduced functionality or self-repair operations.
    /// Transitions to Healthy if recovery succeeds, back to Failed if not.
    /// </summary>
    Recovering = 4
}

/// <summary>
/// Comprehensive health information for a system including state, metrics, and diagnostics.
/// </summary>
/// <param name="State">Current operational state of the system</param>
/// <param name="LastUpdateTime">Timestamp of the last successful update</param>
/// <param name="SuccessiveFailures">Number of consecutive update failures</param>
/// <param name="AverageUpdateTime">Rolling average of update execution time in milliseconds</param>
/// <param name="LastError">Most recent exception or error information</param>
/// <param name="DiagnosticMessage">Human-readable status or diagnostic information</param>
public readonly record struct SystemHealthInfo(
    SystemState State,
    DateTime LastUpdateTime,
    int SuccessiveFailures,
    float AverageUpdateTime,
    string? LastError,
    string? DiagnosticMessage
)
{
    /// <summary>
    /// Creates a healthy system health info with default values.
    /// </summary>
    public SystemHealthInfo() : this(
        SystemState.Healthy,
        DateTime.UtcNow,
        0,
        0.0f,
        null,
        "System initialized successfully"
    ) { }

    /// <summary>
    /// Checks if the system is in a state where updates should be allowed.
    /// </summary>
    public bool CanUpdate => State != SystemState.Failed;

    /// <summary>
    /// Checks if the system needs attention (degraded, critical, or failed).
    /// </summary>
    public bool NeedsAttention => State >= SystemState.Degraded;

    /// <summary>
    /// Gets a user-friendly description of the current system state.
    /// </summary>
    public string StatusDescription => State switch
    {
        SystemState.Healthy => "Operating normally",
        SystemState.Degraded => "Performance issues detected",
        SystemState.Critical => "Severe problems - high failure rate",
        SystemState.Failed => "System disabled due to failures",
        SystemState.Recovering => "Attempting to recover from failures",
        _ => "Unknown state"
    };
}


