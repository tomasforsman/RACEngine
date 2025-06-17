namespace Rac.Core.Logger;

/// <summary>
/// Implementation of ILogger that provides structured logging capabilities.
/// This logger minimizes console spam while preserving debug information.
/// </summary>
public class SerilogLogger : ILogger
{
    /// <summary>
    /// Logs debug messages. Only outputs in debug builds to prevent console spam.
    /// </summary>
    /// <param name="message">The debug message to log.</param>
    public void LogDebug(string message)
    {
#if DEBUG
        // Only show debug messages in debug builds
        System.Diagnostics.Debug.WriteLine($"[DEBUG] {message}");
#endif
    }

    /// <summary>
    /// Logs informational messages to debug output instead of console.
    /// </summary>
    /// <param name="message">The informational message to log.</param>
    public void LogInfo(string message)
    {
        // Use Debug.WriteLine to avoid console spam during tests
        System.Diagnostics.Debug.WriteLine($"[INFO] {message}");
    }

    /// <summary>
    /// Logs warning messages. These are important enough to show in console.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    public void LogWarning(string message)
    {
        Console.WriteLine($"[WARNING] {message}");
    }

    /// <summary>
    /// Logs error messages. These are critical and should always be visible.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    public void LogError(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }
}
