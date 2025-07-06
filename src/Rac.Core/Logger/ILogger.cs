namespace Rac.Core.Logger;

/// <summary>
/// Defines the contract for logging services within the RACEngine.
/// Provides structured logging capabilities with different severity levels.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs a debug message. Typically used for detailed diagnostic information.
    /// </summary>
    /// <param name="message">The debug message to log.</param>
    void LogDebug(string message);

    /// <summary>
    /// Logs an informational message. Used for general application flow information.
    /// </summary>
    /// <param name="message">The informational message to log.</param>
    void LogInfo(string message);


    /// <summary>
    /// Logs an informational message with additional context, such as the name of the system for which medium-level recovery is being attempted and an additional identifier.
    /// This overload is intended for scenarios where more detailed context is required for troubleshooting or log filtering.
    /// </summary>
    /// <param name="systemName">The name of the system where medium-level recovery is being attempted. This provides context for the log entry.</param>
    /// <param name="contextName">An additional identifier or name relevant to the informational context, such as a subsystem, resource, or operation name.</param>
    /// <example>
    /// <code>
    /// logger.LogInfo("AudioSystem", "BufferUnderrunRecovery");
    /// </code>
    /// </example>
    void LogInfo(string systemName, string contextName);



    /// <summary>
    /// Logs a warning message. Indicates potentially harmful situations that don't prevent operation.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    void LogWarning(string message);


    /// <summary>
    /// Logs a warning message with an associated <see cref="Exception"/> and additional context information.
    /// This overload is intended for scenarios where a warning is related to an exception and includes
    /// system-specific context, such as the name of the system for which recovery failed and an additional identifier.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/> instance representing the warning condition. This should provide detailed diagnostic information about the failure.</param>
    /// <param name="systemName">The name of the system where the warning occurred. This provides context for troubleshooting and log filtering.</param>
    /// <param name="contextName">An additional identifier or name relevant to the warning context, such as a subsystem, resource, or operation name.</param>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     // Attempt recovery for the "PhysicsSystem"
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogWarning(ex, "PhysicsSystem", "CollisionResolution");
    /// }
    /// </code>
    /// </example>
    void LogWarning(Exception exception, string systemName, string contextName);


    /// <summary>
    /// Logs an error message. Indicates error conditions that may prevent normal operation.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    void LogError(string message);


    /// <summary>
    /// Logs an error message associated with a specific <see cref="Exception"/> and provides additional context information.
    /// This overload is intended for scenarios where an error is directly related to an exception and requires detailed context,
    /// such as the name of the system where the error occurred and an additional identifier for precise troubleshooting.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/> instance representing the error condition. This should contain detailed diagnostic information about the failure.</param>
    /// <param name="systemName">The name of the system where the error occurred. This provides context for filtering and analyzing log entries.</param>
    /// <param name="contextName">An additional identifier or name relevant to the error context, such as a subsystem, resource, or operation name.</param>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     // Attempt to load a resource in the "AssetSystem"
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError(ex, "AssetSystem", "ResourceLoading");
    /// }
    /// </code>
    /// </example>
    void LogError(Exception exception, string systemName, string contextName);

}
