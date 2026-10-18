using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace ECMS.Host.Logging;

public static class LoggingSetup
{
    public static ILogger CreateLogger()
    {
        Directory.CreateDirectory(LoggingConfiguration.LogDirectory);

        Directory.CreateDirectory(LoggingConfiguration.ArchiveDirectory);

        return new LoggerConfiguration()
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
                   shared: true,
                   hooks: new LogArchiver(),
                   outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
              .CreateLogger();
    }
}