using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Serilog.Debugging;
using Serilog.Sinks.File;

namespace ECMS.Host.Logging;

public sealed class LogArchiver : FileLifecycleHooks
{
    private static readonly Regex LogFilePattern = new(
        @"^ecms-(\d{8})(?:_\d+)?\.log$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Archives log files from dates earlier than the newly opened log file.
    /// </summary>
    /// <param name="path">The path of the newly opened log file.</param>
    /// <param name="underlyingStream">The stream opened by the file sink.</param>
    /// <param name="encoding">The encoding used to write to the log file.</param>
    /// <returns>The unchanged stream provided by the file sink.</returns>
    public override Stream OnFileOpened(
        string path,
        Stream underlyingStream,
        Encoding encoding)
    {
        // A new daily file has opened. Older dates can now
        // be archived; files from the current date stay active.
        try
        {
            ArchivePreviousDates(path);
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine(
                "Unable to archive previous log files: {0}",
                ex);
        }

        return underlyingStream;
    }

    /// <summary>
    /// Archives a log file Serilog is about to delete, unless its date is in the future.
    /// </summary>
    /// <param name="path">The path of the log file Serilog is about to delete.</param>
    public override void OnFileDeleting(string path)
    {
        // Preserve rolled files before Serilog removes them
        // due to its active-file retention limit.
        if (TryGetLogDate(path, out var fileDate) &&
            fileDate > DateOnly.FromDateTime(DateTime.Now))
        {
            SelfLog.WriteLine(
                "Skipping archive of future-dated log file {0}",
                path);
            return;
        }

        try
        {
            ArchiveFile(path);
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine(
                "Unable to archive log file {0}: {1}",
                path,
                ex);
        }
    }

    /// <summary>
    /// Archives matching log files older than the date encoded in the current log path.
    /// </summary>
    /// <param name="currentPath">The path of the newly opened log file whose date is the cutoff.</param>
    private static void ArchivePreviousDates(string currentPath)
    {
        if (!TryGetLogDate(
                currentPath,
                out var currentDate))
            return;

        if (!Directory.Exists(LoggingConfiguration.LogDirectory))
            return;

        foreach (var file in Directory.EnumerateFiles(
                     LoggingConfiguration.LogDirectory,
                     "*.log",
                     SearchOption.TopDirectoryOnly))
        {
            if (PathsEqual(
                    file,
                    currentPath))
                continue;

            if (TryGetLogDate(
                    file,
                    out var fileDate) &&
                fileDate < currentDate)
                ArchiveFile(file);
        }
    }

    /// <summary>
    /// Extracts a valid date from a log filename matching the configured naming pattern.
    /// </summary>
    /// <param name="path">The path whose filename should be checked.</param>
    /// <param name="date">When this method returns, contains the parsed date if successful; otherwise, the default date.</param>
    /// <returns><see langword="true"/> if the filename matches the pattern and contains a valid date; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetLogDate(
        string path,
        out DateOnly date)
    {
        date = default;

        var match = LogFilePattern.Match(
            Path.GetFileName(path));

        return match.Success &&
               DateOnly.TryParseExact(
                   match.Groups[1].Value,
                   "yyyyMMdd",
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.None,
                   out date);
    }

    /// <summary>
    /// Determines whether two paths refer to the same file using the platform's path casing rules.
    /// </summary>
    /// <param name="first">The first file path to compare.</param>
    /// <param name="second">The second file path to compare.</param>
    /// <returns><see langword="true"/> if the paths identify the same file; otherwise, <see langword="false"/>.</returns>
    private static bool PathsEqual(string first,
                                   string second)
    {
        return string.Equals(
            Path.GetFullPath(first),
            Path.GetFullPath(second),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);
    }

    /// <summary>
    /// Compresses a log file into the archive directory, removes the source, and enforces archive size limits.
    /// </summary>
    /// <param name="sourcePath">The path of the log file to archive.</param>
    private static void ArchiveFile(string sourcePath)
    {
        if (!File.Exists(sourcePath))
            return;

        Directory.CreateDirectory(
            LoggingConfiguration.ArchiveDirectory);

        var archivePath = Path.Combine(
            LoggingConfiguration.ArchiveDirectory,
            Path.GetFileName(sourcePath) + ".gz");

        // Avoid overwriting an existing archive.
        if (File.Exists(archivePath))
            archivePath = Path.Combine(
                LoggingConfiguration.ArchiveDirectory,
                $"{Path.GetFileName(sourcePath)}." +
                $"{DateTime.UtcNow:HHmmssfff}.gz");

        // Compress first. If compression fails, preserve the original.
        try
        {
            using (FileStream input = new(
                       sourcePath,
                       FileMode.Open,
                       FileAccess.Read,
                       FileShare.Read))
            {
                using (FileStream output = new(
                           archivePath,
                           FileMode.CreateNew,
                           FileAccess.Write,
                           FileShare.None))
                {
                    using (GZipStream gzip = new(
                               output,
                               CompressionLevel.Fastest))
                    {
                        input.CopyTo(gzip);
                    }
                }
            }
        }
        catch
        {
            // Keep the original file if archiving fails.
            // Remove any incomplete archive produced by this attempt.
            try
            {
                if (File.Exists(archivePath)) File.Delete(archivePath);
            }
            catch
            {
                // Leave any incomplete archive for later inspection.
            }

            throw;
        }

        // Compression succeeded. Remove the original afterward.
        File.Delete(sourcePath);

        // Retention errors must not delete the newly created archive.
        try
        {
            EnforceArchiveSizeLimit();
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine(
                "Unable to enforce archive size limit: {0}",
                ex);
        }
    }

    /// <summary>
    /// Deletes the oldest compressed archives until the configured total size limit is met.
    /// </summary>
    private static void EnforceArchiveSizeLimit()
    {
        var files = new DirectoryInfo(
                        LoggingConfiguration.ArchiveDirectory)
                   .GetFiles("*.gz")
                   .OrderBy(file => file.LastWriteTimeUtc)
                   .ToList();

        var totalSize = files.Sum(file => file.Length);

        foreach (var file in files)
        {
            if (totalSize <=
                LoggingConfiguration.MaxArchiveSizeBytes)
                break;

            var fileSize = file.Length;

            try
            {
                file.Delete();
                totalSize -= fileSize;
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine(
                    "Unable to remove old archive {0}: {1}",
                    file.FullName,
                    ex);
            }
        }
    }
}