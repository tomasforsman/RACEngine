using Serilog;
using Serilog.Events;

namespace Rac.Core.Logger;

public class SerilogLogger : ILogger
{
    private readonly Serilog.ILogger _logger;

    public SerilogLogger()
    {
        _logger = Log.ForContext<SerilogLogger>();
    }

    public void LogDebug(string message)
    {
        _logger.Debug(message);
    }

    public void LogInfo(string message)
    {
        _logger.Information(message);
    }

    public void LogInfo(string systemName, string contextName)
    {
        _logger
            .ForContext("System", systemName)
            .ForContext("Context", contextName)
            .Information("Info: {System}/{Context}");
    }

    public void LogWarning(string message)
    {
        _logger.Warning(message);
    }

    public void LogWarning(Exception exception, string systemName, string contextName)
    {
        _logger
            .ForContext("System", systemName)
            .ForContext("Context", contextName)
            .Warning(exception, "Warning in {System}/{Context}");
    }

    public void LogError(string message)
    {
        _logger.Error(message);
    }

    public void LogError(Exception exception, string systemName, string contextName)
    {
        _logger
            .ForContext("System", systemName)
            .ForContext("Context", contextName)
            .Error(exception, "Error in {System}/{Context}");
    }
}
