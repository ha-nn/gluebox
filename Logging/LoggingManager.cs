using Serilog;
using Serilog.Events;

namespace HanneBogaerts.GlueBox.Logging;

public static class LoggingManager
{
    private static LogEventLevel _logEventLevel = LogEventLevel.Information;

    public static void SetLogLevel(string level)
    {
        Log.Debug("Setting log level to {LogLevel}", level);
        Log.CloseAndFlush();
        var logLevel = Enum.Parse<LogEventLevel>(level);
        _logEventLevel = logLevel;
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .WriteTo.Console()
            .CreateLogger();
        Log.Information("Log level set to {LogLevel}", logLevel.ToString());
    }

    public static void InitializeLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();
        Log.Debug("GlueBox created");
    }

    public static void ChangeLogOutput(string method)
    {
        const string path = "../../../log.txt";
        ChangeLogOutput(method, path);
    }


    public static void ChangeLogOutput(string method, string path)
    {
        Log.Debug("Changing log output to {Method} {Path}", method, path);
        Log.CloseAndFlush();
        if (method.Equals("file", StringComparison.CurrentCultureIgnoreCase))
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(_logEventLevel)
                .WriteTo.File(path)
                .CreateLogger();
            Log.Information("Log output changed to {Method} at {Path}", method, path);
            return;
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(_logEventLevel)
            .WriteTo.Console()
            .CreateLogger();
        Log.Information("Log output changed to {Method}", method);
    }
}