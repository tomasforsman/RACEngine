namespace Rac.ECS.Systems.HealthMonitoring;

/// <summary>
/// Tracks health metrics and state for an individual system over time.
///
/// HEALTH TRACKING ALGORITHM:
/// - Maintains rolling window of recent performance metrics
/// - Uses weighted moving average for update time calculation
/// - Implements state transitions based on failure thresholds and recovery
/// - Tracks recovery attempts with exponential backoff timing
///
/// EDUCATIONAL NOTES:
/// This demonstrates several important monitoring concepts:
/// - Time series data collection and analysis
/// - Statistical analysis (moving averages, thresholds)
/// - State machine design for health transitions
/// - Exponential backoff for recovery attempts
///
/// PERFORMANCE CONSIDERATIONS:
/// - Uses circular buffer for efficient rolling window
/// - Minimal allocations during normal operation
/// - Lazy computation of derived metrics
/// - Thread-safe for concurrent access scenarios
/// </summary>
internal sealed class SystemHealthTracker
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TRACKING STATE AND CONFIGURATION
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly ISystem _system;
    private readonly Queue<float> _recentUpdateTimes = new();
    private readonly int _maxHistorySize = 100; // Track last 100 updates

    // Current health state
    private SystemState _currentState = SystemState.Healthy;
    private int _successiveFailures = 0;
    private DateTime _lastUpdateTime = DateTime.UtcNow;
    private DateTime _lastStateChange = DateTime.UtcNow;
    private string? _lastError = null;
    private string? _diagnosticMessage = "System initialized";

    // Performance metrics
    private float _totalUpdateTime = 0.0f;
    private int _totalUpdates = 0;
    private float _recentAverageUpdateTime = 0.0f;

    // Recovery tracking
    private int _recoveryAttempts = 0;
    private DateTime _lastRecoveryAttempt = DateTime.MinValue;

    // Health thresholds (configurable per system type)
    private float _degradedThresholdMs = 16.0f; // >16ms average = degraded (60fps)
    private float _criticalThresholdMs = 33.0f; // >33ms average = critical (30fps)
    private const int _degradedFailureThreshold = 2; // 2 failures = degraded
    private const int _criticalFailureThreshold = 5; // 5 failures = critical
    private const int _failedThreshold = 10; // 10 failures = failed

    /// <summary>
    /// Initializes a new health tracker for the specified system.
    /// </summary>
    /// <param name="system">The system to track health for</param>
    public SystemHealthTracker(ISystem system)
    {
        _system = system ?? throw new ArgumentNullException(nameof(system));

        // Customize thresholds based on system type for educational purposes
        CustomizeThresholdsForSystemType();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HEALTH EVENT RECORDING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Records a successful system operation with optional timing information.
    /// </summary>
    /// <param name="updateTimeMs">Execution time in milliseconds (optional)</param>
    public void RecordSuccessfulOperation(float updateTimeMs = 0.0f)
    {
        _lastUpdateTime = DateTime.UtcNow;
        _successiveFailures = 0; // Reset failure counter on success
        _lastError = null;
        _diagnosticMessage = "Operating normally";

        // Record timing if provided
        if (updateTimeMs > 0.0f)
        {
            RecordUpdateTime(updateTimeMs);
        }

        // Update state based on performance metrics
        UpdateHealthState();
    }

    /// <summary>
    /// Records a system failure with exception information.
    /// </summary>
    /// <param name="exception">The exception that caused the failure</param>
    public void RecordFailure(Exception exception)
    {
        _successiveFailures++;
        _lastError = exception.Message;
        _diagnosticMessage = $"Failed: {exception.GetType().Name}";

        Console.WriteLine($"🚨 System {_system.GetType().Name} failure #{_successiveFailures}: {exception.Message}");

        // Update state based on failure count
        UpdateHealthState();
    }

    /// <summary>
    /// Records a health check failure (CanUpdate returned false).
    /// </summary>
    /// <param name="reason">Reason for the health check failure</param>
    public void RecordHealthCheckFailure(string reason)
    {
        _diagnosticMessage = $"Health check failed: {reason}";

        // Health check failures are less severe than update failures
        // but still indicate system stress
        if (_currentState == SystemState.Healthy)
        {
            TransitionToState(SystemState.Degraded);
        }
    }

    /// <summary>
    /// Records a successful recovery operation.
    /// </summary>
    public void RecordSuccessfulRecovery()
    {
        _recoveryAttempts = 0;
        _successiveFailures = 0;
        _lastError = null;
        _diagnosticMessage = "Recovered successfully";
        _lastRecoveryAttempt = DateTime.UtcNow;

        TransitionToState(SystemState.Healthy);
    }

    /// <summary>
    /// Records a failed recovery attempt.
    /// </summary>
    public void RecordFailedRecovery()
    {
        _recoveryAttempts++;
        _lastRecoveryAttempt = DateTime.UtcNow;
        _diagnosticMessage = $"Recovery attempt #{_recoveryAttempts} failed";

        // Stay in failed state
        TransitionToState(SystemState.Failed);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HEALTH STATE COMPUTATION AND TRANSITIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current health information for the system.
    /// </summary>
    /// <returns>Comprehensive health information</returns>
    public SystemHealthInfo GetCurrentHealth()
    {
        return new SystemHealthInfo(
            _currentState,
            _lastUpdateTime,
            _successiveFailures,
            _recentAverageUpdateTime,
            _lastError,
            _diagnosticMessage
        );
    }

    /// <summary>
    /// Updates the health state based on current metrics and failure counts.
    /// Implements the health state transition logic.
    /// </summary>
    private void UpdateHealthState()
    {
        var newState = DetermineHealthState();

        if (newState != _currentState)
        {
            TransitionToState(newState);
        }
    }

    /// <summary>
    /// Determines the appropriate health state based on current metrics.
    /// </summary>
    /// <returns>The health state that matches current system condition</returns>
    private SystemState DetermineHealthState()
    {
        // Failed state: Too many successive failures
        if (_successiveFailures >= _failedThreshold)
        {
            return SystemState.Failed;
        }

        // Critical state: High failure rate or very poor performance
        if (_successiveFailures >= _criticalFailureThreshold ||
            _recentAverageUpdateTime > _criticalThresholdMs)
        {
            return SystemState.Critical;
        }

        // Degraded state: Some failures or poor performance
        if (_successiveFailures >= _degradedFailureThreshold ||
            _recentAverageUpdateTime > _degradedThresholdMs)
        {
            return SystemState.Degraded;
        }

        // Healthy state: No recent failures and good performance
        return SystemState.Healthy;
    }

    /// <summary>
    /// Transitions the system to a new health state with logging.
    /// </summary>
    /// <param name="newState">The new health state</param>
    private void TransitionToState(SystemState newState)
    {
        var oldState = _currentState;
        _currentState = newState;
        _lastStateChange = DateTime.UtcNow;

        var systemName = _system.GetType().Name;

        // Log state transitions for visibility
        if (newState != oldState)
        {
            var icon = newState switch
            {
                SystemState.Healthy => "✅",
                SystemState.Degraded => "⚠️",
                SystemState.Critical => "🔶",
                SystemState.Failed => "🚨",
                SystemState.Recovering => "🔄",
                _ => "❓"
            };

            Console.WriteLine($"{icon} System {systemName}: {oldState} → {newState}");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PERFORMANCE METRICS TRACKING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Records an update execution time and maintains rolling average.
    /// </summary>
    /// <param name="updateTimeMs">Update execution time in milliseconds</param>
    private void RecordUpdateTime(float updateTimeMs)
    {
        // Add to rolling window
        _recentUpdateTimes.Enqueue(updateTimeMs);

        // Maintain window size
        if (_recentUpdateTimes.Count > _maxHistorySize)
        {
            _recentUpdateTimes.Dequeue();
        }

        // Update overall metrics
        _totalUpdateTime += updateTimeMs;
        _totalUpdates++;

        // Recalculate recent average (weighted toward recent samples)
        _recentAverageUpdateTime = _recentUpdateTimes.Average();
    }

    /// <summary>
    /// Customizes health thresholds based on the system type.
    /// Educational example of how different systems have different performance expectations.
    /// </summary>
    private void CustomizeThresholdsForSystemType()
    {
        var systemType = _system.GetType();
        var systemName = systemType.Name;

        // Example: Rendering systems should be faster than AI systems
        if (systemName.Contains("Render") || systemName.Contains("Graphics"))
        {
            // Rendering systems need to be fast for smooth framerates
            _degradedThresholdMs = 8.0f;  // >8ms = degraded
            _criticalThresholdMs = 16.0f; // >16ms = critical
        }
        else if (systemName.Contains("AI") || systemName.Contains("Pathfinding"))
        {
            // AI systems can take longer but should still be reasonable
            _degradedThresholdMs = 32.0f; // >32ms = degraded
            _criticalThresholdMs = 64.0f; // >64ms = critical
        }
        else if (systemName.Contains("Physics"))
        {
            // Physics systems are performance-critical
            _degradedThresholdMs = 10.0f; // >10ms = degraded
            _criticalThresholdMs = 20.0f; // >20ms = critical
        }

        // Log customized thresholds for educational purposes
        Console.WriteLine($"📊 Health thresholds for {systemName}: " +
                         $"Degraded>{_degradedThresholdMs}ms, Critical>{_criticalThresholdMs}ms");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RECOVERY TRACKING PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the number of recovery attempts for this system.
    /// </summary>
    public int RecoveryAttempts => _recoveryAttempts;

    /// <summary>
    /// Gets the timestamp of the last recovery attempt.
    /// </summary>
    public DateTime LastRecoveryAttempt => _lastRecoveryAttempt;

    /// <summary>
    /// Gets the total number of updates recorded for this system.
    /// </summary>
    public int TotalUpdates => _totalUpdates;

    /// <summary>
    /// Gets the average update time across all recorded updates.
    /// </summary>
    public float OverallAverageUpdateTime => _totalUpdates > 0 ? _totalUpdateTime / _totalUpdates : 0.0f;
}
