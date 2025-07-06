using System;
using Rac.ECS.Systems;

namespace Rac.ECS.Systems.HealthMonitoring;

/// <summary>
/// Extension methods to easily add health monitoring to any system.
/// Provides convenient APIs for wrapping existing systems with health monitoring capabilities.
/// </summary>
public static class SystemHealthExtensions
{
    /// <summary>
    /// Wraps any ISystem with health monitoring using predefined environment configuration.
    /// </summary>
    /// <param name="system">The system to monitor</param>
    /// <param name="environment">Environment name (Development, Testing, Staging, Production, PerformanceTesting, Disabled)</param>
    /// <returns>Health-monitored version of the system</returns>
    public static IHealthMonitoredSystem WithHealthMonitoring(this ISystem system, string environment = "Development")
    {
        var config = GetConfigForEnvironment(environment);
        return new HealthMonitoredSystemDecorator(system, config);
    }

    /// <summary>
    /// Wraps any ISystem with health monitoring using custom configuration.
    /// </summary>
    /// <param name="system">The system to monitor</param>
    /// <param name="config">Custom health monitoring configuration</param>
    /// <param name="healthCheckProvider">Optional custom health check provider</param>
    /// <returns>Health-monitored version of the system</returns>
    public static IHealthMonitoredSystem WithHealthMonitoring(
        this ISystem system,
        IHealthMonitoringConfig config,
        IHealthCheckProvider? healthCheckProvider = null)
    {
        return new HealthMonitoredSystemDecorator(system, config, healthCheckProvider);
    }

    /// <summary>
    /// Wraps any ISystem with health monitoring using a configuration builder.
    /// </summary>
    /// <param name="system">The system to monitor</param>
    /// <param name="configureHealth">Action to configure health monitoring</param>
    /// <returns>Health-monitored version of the system</returns>
    public static IHealthMonitoredSystem WithHealthMonitoring(
        this ISystem system,
        Action<HealthMonitoringConfigBuilder> configureHealth)
    {
        var builder = new HealthMonitoringConfigBuilder();
        configureHealth(builder);
        var config = builder.Build();

        return new HealthMonitoredSystemDecorator(system, config);
    }

    /// <summary>
    /// Maps environment names to their corresponding predefined configurations.
    /// </summary>
    private static IHealthMonitoringConfig GetConfigForEnvironment(string environment)
    {
        return environment.ToLowerInvariant() switch
        {
            "development" or "dev" => HealthMonitoringConfigs.Development,
            "testing" or "test" => HealthMonitoringConfigs.Testing,
            "staging" or "stage" => HealthMonitoringConfigs.Staging,
            "production" or "prod" => HealthMonitoringConfigs.Production,
            "performancetesting" or "perf" => HealthMonitoringConfigs.PerformanceTesting,
            "disabled" or "none" => HealthMonitoringConfigs.Disabled,
            _ => HealthMonitoringConfigs.Development
        };
    }
}

/// <summary>
/// Factory for creating health-monitored systems with common configurations.
/// Provides convenient methods for creating systems with predefined health monitoring setups.
/// </summary>
public static class HealthMonitoredSystemFactory
{
    /// <summary>
    /// Creates a health-monitored container system optimized for development.
    /// </summary>
    public static IHealthMonitoredSystem CreateDevelopmentContainerSystem()
    {
        var containerSystem = new ContainerSystem();
        return containerSystem.WithHealthMonitoring(HealthMonitoringConfigs.Development);
    }

    /// <summary>
    /// Creates a health-monitored container system optimized for production.
    /// </summary>
    public static IHealthMonitoredSystem CreateProductionContainerSystem()
    {
        var containerSystem = new ContainerSystem();
        return containerSystem.WithHealthMonitoring(HealthMonitoringConfigs.Production);
    }

    /// <summary>
    /// Creates a health-monitored container system optimized for staging environment.
    /// </summary>
    public static IHealthMonitoredSystem CreateStagingContainerSystem()
    {
        var containerSystem = new ContainerSystem();
        return containerSystem.WithHealthMonitoring(HealthMonitoringConfigs.Staging);
    }

    /// <summary>
    /// Creates a health-monitored system with custom configuration.
    /// </summary>
    public static IHealthMonitoredSystem CreateCustomMonitoredSystem<T>(
        Action<HealthMonitoringConfigBuilder>? configure = null)
        where T : ISystem, new()
    {
        var system = new T();

        if (configure == null)
        {
            return system.WithHealthMonitoring("Development");
        }

        return system.WithHealthMonitoring(configure);
    }
}

/// <summary>
/// Comprehensive examples demonstrating health monitoring usage patterns.
/// Shows recommended approaches for different environments and use cases.
/// </summary>
public static class HealthMonitoringExamples
{
    /// <summary>
    /// Demonstrates various health monitoring configuration patterns.
    /// </summary>
    public static void ShowUsageExamples()
    {
        // Simple usage with predefined environment configurations
        var developmentSystem = new ContainerSystem().WithHealthMonitoring("Development");
        var testingSystem = new ContainerSystem().WithHealthMonitoring("Testing");
        var stagingSystem = new ContainerSystem().WithHealthMonitoring("Staging");
        var productionSystem = new ContainerSystem().WithHealthMonitoring("Production");
        var performanceTestSystem = new ContainerSystem().WithHealthMonitoring("PerformanceTesting");

        // Custom configuration using enhanced builder with new properties
        var customSystem = new ContainerSystem().WithHealthMonitoring(config =>
        {
            config.CheckEverySeconds(15)
                  .EnableAutoRecovery()
                  .MaxRecoveryAttempts(2)
                  .RethrowExceptions(false)
                  .AllowGCInRecovery(false)                    // New property
                  .EnableVerboseLogging(true)                  // New property
                  .MinimumLogLevel(LogLevel.Information)       // New property
                  .Environment("CustomProduction");
        });

        // Production-optimized custom configuration
        var productionOptimizedSystem = new ContainerSystem().WithHealthMonitoring(config =>
        {
            config.CheckEvery(TimeSpan.FromMinutes(2))
                  .EnableAutoRecovery(true)
                  .MaxRecoveryAttempts(1)
                  .RethrowExceptions(false)
                  .AllowGCInRecovery(false)                    // Never allow GC in production
                  .EnableVerboseLogging(false)                 // Minimal logging
                  .MinimumLogLevel(LogLevel.Error)             // Only errors
                  .Environment("OptimizedProduction");
        });

        // Development configuration with comprehensive monitoring
        var developmentOptimizedSystem = new ContainerSystem().WithHealthMonitoring(config =>
        {
            config.CheckEverySeconds(2)
                  .EnableAutoRecovery(true)
                  .MaxRecoveryAttempts(3)
                  .RethrowExceptions(true)                     // Fail fast for debugging
                  .AllowGCInRecovery(true)                     // OK in development
                  .EnableVerboseLogging(true)                  // Full diagnostics
                  .MinimumLogLevel(LogLevel.Debug)             // Everything
                  .Environment("VerboseDevelopment");
        });

        // Factory usage examples
        var devContainerSystem = HealthMonitoredSystemFactory.CreateDevelopmentContainerSystem();
        var prodContainerSystem = HealthMonitoredSystemFactory.CreateProductionContainerSystem();
        var stagingContainerSystem = HealthMonitoredSystemFactory.CreateStagingContainerSystem();

        // Custom system type with specific configuration
        var customMonitoredSystem = HealthMonitoredSystemFactory.CreateCustomMonitoredSystem<ContainerSystem>(config =>
        {
            config.CheckEvery(TimeSpan.FromMinutes(1))
                  .EnableAutoRecovery(false)
                  .AllowGCInRecovery(false)
                  .MinimumLogLevel(LogLevel.Warning);
        });

        Console.WriteLine("Health monitoring examples created successfully!");
        Console.WriteLine("Configurations demonstrate production-ready patterns with enhanced control.");
    }

    /// <summary>
    /// Demonstrates environment-specific configuration selection.
    /// </summary>
    public static void ShowEnvironmentExamples()
    {
        // Environment-based configuration selection
        var environments = new[] { "Development", "Testing", "Staging", "Production", "PerformanceTesting", "Disabled" };

        foreach (var environment in environments)
        {
            var system = new ContainerSystem().WithHealthMonitoring(environment);
            var healthInfo = system.GetHealthInfo();

            Console.WriteLine($"Environment: {environment}");
            Console.WriteLine($"  Status: {healthInfo.StatusDescription}");
            Console.WriteLine($"  Can Update: {system.CanUpdate()}");
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Demonstrates advanced configuration patterns for specialized scenarios.
    /// </summary>
    public static void ShowAdvancedPatterns()
    {
        // High-performance gaming scenario
        var gameSystem = new ContainerSystem().WithHealthMonitoring(config =>
        {
            config.CheckEvery(TimeSpan.FromMinutes(5))          // Very infrequent
                  .EnableAutoRecovery(false)                     // No recovery during gameplay
                  .RethrowExceptions(false)                      // Never crash the game
                  .AllowGCInRecovery(false)                      // Never cause frame drops
                  .EnableVerboseLogging(false)                   // No logging overhead
                  .MinimumLogLevel(LogLevel.None)                // Complete silence
                  .Environment("HighPerformanceGame");
        });

        // Development debugging scenario
        var debugSystem = new ContainerSystem().WithHealthMonitoring(config =>
        {
            config.CheckEverySeconds(1)                         // Very frequent
                  .EnableAutoRecovery(true)                      // Try to recover
                  .MaxRecoveryAttempts(5)                        // Many attempts
                  .RethrowExceptions(true)                       // Fail fast
                  .AllowGCInRecovery(true)                       // OK to impact performance
                  .EnableVerboseLogging(true)                    // Full diagnostics
                  .MinimumLogLevel(LogLevel.Debug)               // Everything
                  .Environment("IntensiveDebugging");
        });

        // Server/service scenario
        var serverSystem = new ContainerSystem().WithHealthMonitoring(config =>
        {
            config.CheckEvery(TimeSpan.FromSeconds(30))         // Regular monitoring
                  .EnableAutoRecovery(true)                      // Auto-heal
                  .MaxRecoveryAttempts(3)                        // Reasonable attempts
                  .RethrowExceptions(false)                      // Keep service running
                  .AllowGCInRecovery(false)                      // Avoid service disruption
                  .EnableVerboseLogging(false)                   // Minimal overhead
                  .MinimumLogLevel(LogLevel.Warning)             // Important issues only
                  .Environment("ServerProduction");
        });

        Console.WriteLine("Advanced configuration patterns demonstrate specialized use cases.");
    }
}
