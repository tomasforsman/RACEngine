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
    /// Logs a warning message. Indicates potentially harmful situations that don't prevent operation.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    void LogWarning(string message);

    /// <summary>
    /// Logs an error message. Indicates error conditions that may prevent normal operation.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    void LogError(string message);
}
