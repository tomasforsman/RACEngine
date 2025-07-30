using Serilog;
using Serilog.Events;

namespace Rac.Core.Logger;

/// <summary>
/// Concrete implementation of <see cref="ILogger"/> using the Serilog logging framework.
/// Provides structured logging with contextual information for educational game engine debugging.
/// </summary>
/// <remarks>
/// This implementation leverages Serilog's powerful structured logging capabilities to provide:
/// - Contextual logging with system and operation names
/// - Hierarchical log levels (Debug, Info, Warning, Error)
/// - Structured data for easier log analysis and filtering
/// - Integration with external log sinks and dashboards
/// 
/// Educational Note: Serilog is chosen for its excellent structured logging support,
/// which is essential for debugging complex game engine systems where traditional
/// string-based logging can become overwhelming.
/// 
/// The logger automatically creates context-specific loggers using Serilog's ForContext
/// method, enabling fine-grained filtering and analysis of log data by system component.
/// </remarks>
/// <example>
/// <code>
/// var logger = new SerilogLogger();
/// 
/// // Basic logging
/// logger.LogInfo("Game engine initialized successfully");
/// logger.LogWarning("Low memory condition detected");
/// 
/// // Contextual logging for system-specific debugging
/// logger.LogInfo("PhysicsSystem", "CollisionDetection");
/// logger.LogError(exception, "RenderSystem", "ShaderCompilation");
/// </code>
/// </example>
public class SerilogLogger : ILogger
{
    /// <summary>
    /// The internal Serilog logger instance used for actual log writing.
    /// </summary>
    private readonly Serilog.ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerilogLogger"/> class.
    /// Creates a context-specific logger for the SerilogLogger type.
    /// </summary>
    /// <remarks>
    /// The logger is created with a context specific to the SerilogLogger class,
    /// which helps identify log entries originating from the logging infrastructure itself.
    /// This is useful for debugging logging configuration issues.
    /// </remarks>
    public SerilogLogger()
    {
        _logger = Log.ForContext<SerilogLogger>();
    }

    /// <summary>
    /// Logs a debug message for detailed diagnostic information.
    /// Debug messages are typically used for troubleshooting during development.
    /// </summary>
    /// <param name="message">The debug message to log. Should contain specific diagnostic information.</param>
    /// <remarks>
    /// Debug level logging is intended for detailed information that is typically only
    /// of interest when diagnosing problems. These messages are usually not shown in
    /// production environments due to their verbosity.
    /// </remarks>
    /// <example>
    /// <code>
    /// logger.LogDebug("Loading texture asset: sword_texture.png");
    /// logger.LogDebug($"Physics step completed in {elapsed}ms");
    /// </code>
    /// </example>
    public void LogDebug(string message)
    {
        _logger.Debug(message);
    }

    /// <summary>
    /// Logs an informational message about normal application flow.
    /// Information messages indicate that things are working as expected.
    /// </summary>
    /// <param name="message">The informational message to log. Should describe normal operational events.</param>
    /// <remarks>
    /// Information level logging is used to track the general flow of the application.
    /// These messages provide insight into what the application is doing and are
    /// typically shown in production environments.
    /// </remarks>
    /// <example>
    /// <code>
    /// logger.LogInfo("Game engine started successfully");
    /// logger.LogInfo("Level loaded: MainMenu");
    /// </code>
    /// </example>
    public void LogInfo(string message)
    {
        _logger.Information(message);
    }

    /// <summary>
    /// Logs an informational message with structured context for specific systems and operations.
    /// This overload provides enhanced logging for system-specific events with additional context.
    /// </summary>
    /// <param name="systemName">The name of the system generating the log entry (e.g., "AudioSystem", "PhysicsSystem").</param>
    /// <param name="contextName">Additional context information such as operation or subsystem name (e.g., "BufferUnderrun", "CollisionDetection").</param>
    /// <remarks>
    /// This method leverages Serilog's structured logging capabilities to add contextual
    /// properties to the log entry. The system and context names are stored as separate
    /// fields, enabling powerful filtering and analysis capabilities.
    /// 
    /// Educational Note: Structured logging is crucial for complex systems like game engines
    /// where traditional string-based logs can become overwhelming. By categorizing logs
    /// by system and context, developers can easily filter and analyze specific areas
    /// of the engine during debugging.
    /// </remarks>
    /// <example>
    /// <code>
    /// logger.LogInfo("AudioSystem", "SoundLoaded");
    /// logger.LogInfo("PhysicsSystem", "WorldStep");
    /// logger.LogInfo("RenderSystem", "FrameComplete");
    /// </code>
    /// </example>
    public void LogInfo(string systemName, string contextName)
    {
        _logger
            .ForContext("System", systemName)
            .ForContext("Context", contextName)
            .Information("Info: {System}/{Context}");
    }

    /// <summary>
    /// Logs a warning message indicating a potentially harmful situation that doesn't prevent operation.
    /// Warning messages highlight issues that should be investigated but don't stop execution.
    /// </summary>
    /// <param name="message">The warning message to log. Should describe the potentially problematic condition.</param>
    /// <remarks>
    /// Warning level logging is used for situations that are unexpected but not necessarily
    /// errors. The application can continue to operate, but the situation may need attention
    /// to prevent future problems.
    /// </remarks>
    /// <example>
    /// <code>
    /// logger.LogWarning("Memory usage is above 80% threshold");
    /// logger.LogWarning("Deprecated API called: Use UpdateTransform instead");
    /// </code>
    /// </example>
    public void LogWarning(string message)
    {
        _logger.Warning(message);
    }

    /// <summary>
    /// Logs a warning message with an associated exception and structured context information.
    /// This overload captures both the exception details and system-specific context for comprehensive debugging.
    /// </summary>
    /// <param name="exception">The exception that triggered the warning. Contains detailed diagnostic information.</param>
    /// <param name="systemName">The name of the system where the warning occurred (e.g., "AudioSystem", "NetworkSystem").</param>
    /// <param name="contextName">Additional context such as operation or subsystem name (e.g., "BufferRecovery", "ConnectionRetry").</param>
    /// <remarks>
    /// This method combines exception logging with structured context, providing both the
    /// technical details from the exception and the business context of where it occurred.
    /// The exception stack trace and message are preserved while additional context
    /// properties are added for filtering and analysis.
    /// 
    /// Educational Note: Exception context logging is essential in game engines where
    /// failures in one system shouldn't crash the entire engine. By capturing both
    /// the exception and the system context, developers can implement graceful
    /// degradation and recovery strategies.
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     audioSystem.LoadSound("missing_file.wav");
    /// }
    /// catch (FileNotFoundException ex)
    /// {
    ///     logger.LogWarning(ex, "AudioSystem", "SoundLoading");
    ///     // Continue with default sound or silence
    /// }
    /// </code>
    /// </example>
    public void LogWarning(Exception exception, string systemName, string contextName)
    {
        _logger
            .ForContext("System", systemName)
            .ForContext("Context", contextName)
            .Warning(exception, "Warning in {System}/{Context}");
    }

    /// <summary>
    /// Logs an error message indicating a serious problem that may prevent normal operation.
    /// Error messages represent failures that need immediate attention.
    /// </summary>
    /// <param name="message">The error message to log. Should describe the failure condition clearly.</param>
    /// <remarks>
    /// Error level logging is used for serious problems that prevent normal operation
    /// of a feature or system. These messages always require attention and often
    /// indicate bugs or environmental issues that need to be resolved.
    /// </remarks>
    /// <example>
    /// <code>
    /// logger.LogError("Failed to initialize graphics device");
    /// logger.LogError("Critical game save corruption detected");
    /// </code>
    /// </example>
    public void LogError(string message)
    {
        _logger.Error(message);
    }

    /// <summary>
    /// Logs an error message with an associated exception and structured context information.
    /// This overload provides comprehensive error tracking with both exception details and system context.
    /// </summary>
    /// <param name="exception">The exception that caused the error. Contains detailed diagnostic information including stack trace.</param>
    /// <param name="systemName">The name of the system where the error occurred (e.g., "RenderSystem", "PhysicsSystem").</param>
    /// <param name="contextName">Additional context such as operation or subsystem name (e.g., "ShaderCompilation", "CollisionDetection").</param>
    /// <remarks>
    /// This method provides the most comprehensive error logging by combining exception
    /// details with structured context information. The full exception including stack
    /// trace is preserved while system and context properties enable precise filtering
    /// and analysis of error patterns.
    /// 
    /// Educational Note: Comprehensive error logging is critical for game engine reliability.
    /// By capturing both technical exception details and business context, developers can:
    /// - Quickly identify which systems are failing most frequently
    /// - Understand the operational context when errors occur
    /// - Implement targeted fixes and monitoring
    /// - Build resilient systems that can recover from component failures
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     renderSystem.CompileShader(shaderSource);
    /// }
    /// catch (ShaderCompilationException ex)
    /// {
    ///     logger.LogError(ex, "RenderSystem", "ShaderCompilation");
    ///     // Fall back to default shader or disable advanced rendering
    /// }
    /// </code>
    /// </example>
    public void LogError(Exception exception, string systemName, string contextName)
    {
        _logger
            .ForContext("System", systemName)
            .ForContext("Context", contextName)
            .Error(exception, "Error in {System}/{Context}");
    }
}
