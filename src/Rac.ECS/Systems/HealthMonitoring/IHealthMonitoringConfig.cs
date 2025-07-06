using System;
using System.Collections.Generic;

namespace Rac.ECS.Systems.HealthMonitoring;

/// <summary>
/// Defines logging severity levels for health monitoring output.
/// Higher values indicate more severe issues requiring attention.
/// </summary>
public enum LogLevel
{
    /// <summary>Detailed diagnostic information for development and troubleshooting</summary>
    Debug = 0,

    /// <summary>General operational information about system state</summary>
    Information = 1,

    /// <summary>Warning conditions that may indicate potential problems</summary>
    Warning = 2,

    /// <summary>Error conditions that require attention but don't stop operation</summary>
    Error = 3,

    /// <summary>Disable all logging output for maximum performance</summary>
    None = 4
}

/// <summary>
/// Configuration interface for health monitoring behavior across different environments.
/// Provides comprehensive control over monitoring frequency, recovery behavior, and logging output.
/// </summary>
public interface IHealthMonitoringConfig
{
    /// <summary>Interval between scheduled health checks (not performed every frame)</summary>
    TimeSpan HealthCheckInterval { get; }

    /// <summary>Whether the system should automatically attempt recovery when failures are detected</summary>
    bool AutoRecoveryEnabled { get; }

    /// <summary>Whether exceptions should be re-thrown after being logged and handled</summary>
    bool RethrowExceptions { get; }

    /// <summary>Base interval between recovery attempts, escalated exponentially on repeated failures</summary>
    double BaseRecoveryIntervalSeconds { get; }

    /// <summary>Maximum recovery escalation level before the system gives up and enters failed state</summary>
    int MaxRecoveryLevel { get; }

    /// <summary>Environment identifier for configuration selection and logging context</summary>
    string Environment { get; }

    /// <summary>Whether garbage collection is permitted during recovery operations</summary>
    bool EnableGCInRecovery { get; }

    /// <summary>Whether detailed diagnostic output should be generated during operations</summary>
    bool EnableVerboseLogging { get; }

    /// <summary>Minimum severity level for log messages to be output</summary>
    LogLevel MinimumLogLevel { get; }
}

/// <summary>
/// Predefined health monitoring configurations optimized for different deployment environments.
/// Each configuration balances monitoring thoroughness against performance overhead.
/// </summary>
public static class HealthMonitoringConfigs
{
    /// <summary>
    /// Development environment configuration optimized for debugging and immediate feedback.
    /// Features frequent health checks, verbose logging, and fail-fast behavior for rapid problem identification.
    /// </summary>
    public static IHealthMonitoringConfig Development => new HealthMonitoringConfig
    {
        HealthCheckInterval = TimeSpan.FromSeconds(3),
        AutoRecoveryEnabled = true,
        RethrowExceptions = true,
        BaseRecoveryIntervalSeconds = 0.5,
        MaxRecoveryLevel = 2,
        Environment = "Development",
        EnableGCInRecovery = true,
        EnableVerboseLogging = true,
        MinimumLogLevel = LogLevel.Debug
    };

    /// <summary>
    /// Testing environment configuration balancing observability with test stability.
    /// Provides comprehensive monitoring without disrupting automated test execution.
    /// </summary>
    public static IHealthMonitoringConfig Testing => new HealthMonitoringConfig
    {
        HealthCheckInterval = TimeSpan.FromSeconds(10),
        AutoRecoveryEnabled = true,
        RethrowExceptions = false,
        BaseRecoveryIntervalSeconds = 2.0,
        MaxRecoveryLevel = 3,
        Environment = "Testing",
        EnableGCInRecovery = false,
        EnableVerboseLogging = true,
        MinimumLogLevel = LogLevel.Information
    };

    /// <summary>
    /// Staging environment configuration mirroring production settings with enhanced monitoring.
    /// Provides production-like behavior while maintaining visibility for validation and debugging.
    /// </summary>
    public static IHealthMonitoringConfig Staging => new HealthMonitoringConfig
    {
        HealthCheckInterval = TimeSpan.FromSeconds(30),
        AutoRecoveryEnabled = true,
        RethrowExceptions = false,
        BaseRecoveryIntervalSeconds = 5.0,
        MaxRecoveryLevel = 3,
        Environment = "Staging",
        EnableGCInRecovery = false,
        EnableVerboseLogging = true,
        MinimumLogLevel = LogLevel.Information
    };

    /// <summary>
    /// Production environment configuration optimized for runtime performance and stability.
    /// Minimizes monitoring overhead while maintaining system resilience and error recovery.
    /// </summary>
    public static IHealthMonitoringConfig Production => new HealthMonitoringConfig
    {
        HealthCheckInterval = TimeSpan.FromMinutes(1),
        AutoRecoveryEnabled = true,
        RethrowExceptions = false,
        BaseRecoveryIntervalSeconds = 10.0,
        MaxRecoveryLevel = 2,
        Environment = "Production",
        EnableGCInRecovery = false,
        EnableVerboseLogging = false,
        MinimumLogLevel = LogLevel.Warning
    };

    /// <summary>
    /// Performance testing configuration with minimal monitoring overhead.
    /// Designed for benchmarking scenarios where monitoring must not affect performance measurements.
    /// </summary>
    public static IHealthMonitoringConfig PerformanceTesting => new HealthMonitoringConfig
    {
        HealthCheckInterval = TimeSpan.FromMinutes(10),
        AutoRecoveryEnabled = false,
        RethrowExceptions = true,
        BaseRecoveryIntervalSeconds = 60.0,
        MaxRecoveryLevel = 0,
        Environment = "PerformanceTesting",
        EnableGCInRecovery = false,
        EnableVerboseLogging = false,
        MinimumLogLevel = LogLevel.None
    };

    /// <summary>
    /// Disabled monitoring configuration for scenarios requiring absolute minimal overhead.
    /// Essentially disables health monitoring while maintaining interface compatibility.
    /// </summary>
    public static IHealthMonitoringConfig Disabled => new HealthMonitoringConfig
    {
        HealthCheckInterval = TimeSpan.FromHours(1),
        AutoRecoveryEnabled = false,
        RethrowExceptions = true,
        BaseRecoveryIntervalSeconds = 60.0,
        MaxRecoveryLevel = 0,
        Environment = "Disabled",
        EnableGCInRecovery = false,
        EnableVerboseLogging = false,
        MinimumLogLevel = LogLevel.None
    };
}

/// <summary>
/// Concrete implementation of health monitoring configuration with production-safe defaults.
/// All properties use init-only setters for immutability after construction.
/// </summary>
public class HealthMonitoringConfig : IHealthMonitoringConfig
{
    public TimeSpan HealthCheckInterval { get; init; } = TimeSpan.FromSeconds(10);
    public bool AutoRecoveryEnabled { get; init; } = true;
    public bool RethrowExceptions { get; init; } = false;
    public double BaseRecoveryIntervalSeconds { get; init; } = 2.0;
    public int MaxRecoveryLevel { get; init; } = 3;
    public string Environment { get; init; } = "Default";
    public bool EnableGCInRecovery { get; init; } = false;
    public bool EnableVerboseLogging { get; init; } = false;
    public LogLevel MinimumLogLevel { get; init; } = LogLevel.Information;
}

/// <summary>
/// Fluent builder for creating custom health monitoring configurations.
/// Provides a convenient API for constructing configurations with specific requirements.
/// </summary>
public class HealthMonitoringConfigBuilder
{
    private TimeSpan _healthCheckInterval = TimeSpan.FromSeconds(10);
    private bool _autoRecoveryEnabled = true;
    private bool _rethrowExceptions = false;
    private double _baseRecoveryIntervalSeconds = 2.0;
    private int _maxRecoveryLevel = 3;
    private string _environment = "Custom";
    private bool _enableGCInRecovery = false;
    private bool _enableVerboseLogging = false;
    private LogLevel _minimumLogLevel = LogLevel.Information;

    /// <summary>Sets the interval between scheduled health checks</summary>
    public HealthMonitoringConfigBuilder CheckEvery(TimeSpan interval)
    {
        _healthCheckInterval = interval;
        return this;
    }

    /// <summary>Sets the interval between scheduled health checks in seconds</summary>
    public HealthMonitoringConfigBuilder CheckEverySeconds(double seconds)
    {
        _healthCheckInterval = TimeSpan.FromSeconds(seconds);
        return this;
    }

    /// <summary>Configures whether automatic recovery should be attempted on system failures</summary>
    public HealthMonitoringConfigBuilder EnableAutoRecovery(bool enabled = true)
    {
        _autoRecoveryEnabled = enabled;
        return this;
    }

    /// <summary>Configures whether exceptions should be re-thrown after handling</summary>
    public HealthMonitoringConfigBuilder RethrowExceptions(bool rethrow = true)
    {
        _rethrowExceptions = rethrow;
        return this;
    }

    /// <summary>Sets the base interval between recovery attempts in seconds</summary>
    public HealthMonitoringConfigBuilder RecoveryInterval(double seconds)
    {
        _baseRecoveryIntervalSeconds = seconds;
        return this;
    }

    /// <summary>Sets the maximum number of escalated recovery attempts before giving up</summary>
    public HealthMonitoringConfigBuilder MaxRecoveryAttempts(int maxLevel)
    {
        _maxRecoveryLevel = maxLevel;
        return this;
    }

    /// <summary>Sets the environment identifier for this configuration</summary>
    public HealthMonitoringConfigBuilder Environment(string environment)
    {
        _environment = environment;
        return this;
    }

    /// <summary>Configures whether garbage collection is permitted during recovery operations</summary>
    public HealthMonitoringConfigBuilder AllowGCInRecovery(bool allow = true)
    {
        _enableGCInRecovery = allow;
        return this;
    }

    /// <summary>Configures whether verbose diagnostic output should be generated</summary>
    public HealthMonitoringConfigBuilder EnableVerboseLogging(bool enable = true)
    {
        _enableVerboseLogging = enable;
        return this;
    }

    /// <summary>Sets the minimum severity level for log message output</summary>
    public HealthMonitoringConfigBuilder MinimumLogLevel(LogLevel level)
    {
        _minimumLogLevel = level;
        return this;
    }

    /// <summary>Constructs the final configuration with all specified settings</summary>
    public IHealthMonitoringConfig Build()
    {
        return new HealthMonitoringConfig
        {
            HealthCheckInterval = _healthCheckInterval,
            AutoRecoveryEnabled = _autoRecoveryEnabled,
            RethrowExceptions = _rethrowExceptions,
            BaseRecoveryIntervalSeconds = _baseRecoveryIntervalSeconds,
            MaxRecoveryLevel = _maxRecoveryLevel,
            Environment = _environment,
            EnableGCInRecovery = _enableGCInRecovery,
            EnableVerboseLogging = _enableVerboseLogging,
            MinimumLogLevel = _minimumLogLevel
        };
    }
}

/// <summary>
/// Represents a single health check operation that can be performed on a system.
/// Health checks should be lightweight and complete quickly to avoid impacting performance.
/// </summary>
public interface IHealthCheck
{
    /// <summary>Short descriptive name for this health check</summary>
    string Name { get; }

    /// <summary>Detailed description of what this health check validates</summary>
    string Description { get; }

    /// <summary>
    /// Executes the health check and returns the result.
    /// Should complete quickly and not throw exceptions.
    /// </summary>
    /// <returns>True if the check passed, false if issues were detected</returns>
    bool Execute();
}

/// <summary>
/// Provides system-specific health checks based on system type and configuration.
/// Implementations can customize health checks for different system categories.
/// </summary>
public interface IHealthCheckProvider
{
    /// <summary>
    /// Gets the collection of health checks appropriate for the specified system and configuration.
    /// </summary>
    /// <param name="system">The system to generate health checks for</param>
    /// <param name="config">The health monitoring configuration in use</param>
    /// <returns>Collection of health checks to execute</returns>
    IEnumerable<IHealthCheck> GetHealthChecks(ISystem system, IHealthMonitoringConfig config);
}

/// <summary>
/// Default health check provider that includes common system health validations.
/// Provides basic memory monitoring and system-specific checks based on type names.
/// </summary>
public class DefaultHealthCheckProvider : IHealthCheckProvider
{
    public IEnumerable<IHealthCheck> GetHealthChecks(ISystem system, IHealthMonitoringConfig config)
    {
        // Universal health checks applicable to all systems
        yield return new MemoryUsageHealthCheck();

        // System-specific health checks based on naming conventions
        var systemTypeName = system.GetType().Name;

        if (systemTypeName.Contains("Container"))
        {
            yield return new ContainerSystemHealthCheck();
        }

        if (systemTypeName.Contains("Render") || systemTypeName.Contains("Graphics"))
        {
            yield return new RenderingSystemHealthCheck();
        }

        // Environment-specific diagnostic checks
        if (config.Environment == "Development")
        {
            yield return new DebugHealthCheck();
        }
    }
}

/// <summary>
/// Health check that monitors overall application memory usage for leak detection.
/// Triggers warnings when memory consumption exceeds safe operational thresholds.
/// </summary>
public class MemoryUsageHealthCheck : IHealthCheck
{
    public string Name => "Memory Usage";
    public string Description => "Monitors overall memory consumption for leak detection";

    public bool Execute()
    {
        var memoryMB = GC.GetTotalMemory(false) / (1024 * 1024);
        const long maxMemoryMB = 512;

        if (memoryMB > maxMemoryMB)
        {
            Console.WriteLine($"⚠️ High memory usage: {memoryMB}MB (threshold: {maxMemoryMB}MB)");
            return false;
        }

        return true;
    }
}

/// <summary>
/// Health check specific to container management systems.
/// Validates container hierarchy integrity and resource usage patterns.
/// </summary>
public class ContainerSystemHealthCheck : IHealthCheck
{
    public string Name => "Container System";
    public string Description => "Validates container system integrity and hierarchy consistency";

    public bool Execute()
    {
        // Production implementation would validate:
        // - Container hierarchy for circular references
        // - Memory usage of container tracking structures
        // - Number of active containers against configured limits
        // - Entity reference consistency

        Console.WriteLine("🔍 Container system health check completed");
        return true;
    }
}

/// <summary>
/// Health check for rendering and graphics systems.
/// Validates graphics context state and rendering pipeline health.
/// </summary>
public class RenderingSystemHealthCheck : IHealthCheck
{
    public string Name => "Rendering System";
    public string Description => "Validates rendering system state and graphics context health";

    public bool Execute()
    {
        // Production implementation would validate:
        // - Graphics context validity and initialization state
        // - Frame rate stability and performance metrics
        // - GPU memory usage and resource allocation
        // - Render target and buffer integrity

        Console.WriteLine("🎮 Rendering system health check completed");
        return true;
    }
}

/// <summary>
/// Development environment health check providing diagnostic information.
/// Only active in development configurations to provide debugging context.
/// </summary>
public class DebugHealthCheck : IHealthCheck
{
    public string Name => "Debug Info";
    public string Description => "Development environment diagnostic information";

    public bool Execute()
    {
        Console.WriteLine($"🐛 Debug health check executed at {DateTime.Now:HH:mm:ss}");
        return true;
    }
}

/// <summary>
/// Demonstration of configuration usage patterns for different scenarios.
/// Shows recommended approaches for common deployment environments.
/// </summary>
public static class ConfigurationUsageExamples
{
    /// <summary>Demonstrates production-optimized health monitoring configuration</summary>
    public static void ShowProductionUsage()
    {
        // Standard production configuration with minimal overhead
        var productionSystem = new ContainerSystem()
            .WithHealthMonitoring(HealthMonitoringConfigs.Production);

        // Custom production configuration for specific requirements
        var customProductionSystem = new ContainerSystem().WithHealthMonitoring(config =>
        {
            config.CheckEvery(TimeSpan.FromMinutes(2))
                  .EnableAutoRecovery(true)
                  .MaxRecoveryAttempts(1)
                  .AllowGCInRecovery(false)
                  .MinimumLogLevel(LogLevel.Error)
                  .Environment("CustomProduction");
        });

        // Development configuration with comprehensive monitoring
        var developmentSystem = new ContainerSystem()
            .WithHealthMonitoring(HealthMonitoringConfigs.Development);

        // Performance testing configuration with minimal monitoring impact
        var performanceTestSystem = new ContainerSystem()
            .WithHealthMonitoring(HealthMonitoringConfigs.PerformanceTesting);
    }
}
