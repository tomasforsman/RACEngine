namespace Rac.Core.Logger;

public class SerilogLogger : ILogger
{
    public void LogDebug(string message)
    {
        Console.WriteLine($"[DEBUG] {message}");
    }

    public void LogInfo(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public void LogWarning(string message)
    {
        Console.WriteLine($"[WARNING] {message}");
    }

    public void LogError(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }
}
