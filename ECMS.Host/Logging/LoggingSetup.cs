using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace ECMS.Host.Logging;

public static class LoggingSetup
{
    private const string OutputTemplate =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    // The logger is created before the host is built, so read the host's environment variables directly.
    private static bool IsDevelopment()
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                          ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        return string.Equals(
            environment,
            Environments.Development,
            StringComparison.OrdinalIgnoreCase);
    }

    public static ILogger CreateLogger()
    {
        Directory.CreateDirectory(LoggingConfiguration.LogDirectory);

        Directory.CreateDirectory(LoggingConfiguration.ArchiveDirectory);

        var configuration = new LoggerConfiguration()
              .MinimumLevel.Information()
              .MinimumLevel.Override(
                   "Microsoft",
                   LogEventLevel.Warning)
              .WriteTo.File(
                   path: Path.Combine(LoggingConfiguration.LogDirectory, $"{LoggingConfiguration.LogFileNamePrefix}.log"),
                   rollingInterval: RollingInterval.Day,
                   rollOnFileSizeLimit: true,
                   fileSizeLimitBytes: LoggingConfiguration.MaxLogFileSizeBytes,
                   retainedFileCountLimit: LoggingConfiguration.MaxActiveLogFiles,
                   hooks: new LogArchiver(),
                   outputTemplate: OutputTemplate
                );

        // Mirror the file output on the console in Development.
        if (IsDevelopment())
            configuration.WriteTo.Console(outputTemplate: OutputTemplate);

        return configuration.CreateLogger();
    }
}