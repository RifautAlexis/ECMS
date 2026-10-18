namespace ECMS.Host.Logging;

public static class LoggingConfiguration
{
    public const string LogFileNamePrefix = "ecms-";

    public const long MaxLogFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public const int MaxActiveLogFiles = 10;

    public const long MaxArchiveSizeBytes = 1024L * 1024 * 1024; // 1 GB

    private const string LogDirectoryName = "Logs";

    private const string ArchiveDirectoryName = "Archive";

    public static string LogDirectory => Path.Combine(AppContext.BaseDirectory, LogDirectoryName);

    public static string ArchiveDirectory => Path.Combine(LogDirectory, ArchiveDirectoryName);
}