using EasyLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

// ######### Important ################
// I've used Filestream / Stopwatch API 
// ######### Important ################


public sealed class CopyEngine
{
    private readonly EasyLogger _logger;

    public CopyEngine(EasyLogger logger)
    {
        // The engine doesn't own the logger lifecycle; it just uses the provided instance
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Execute(
        CopyPlan plan,
        string jobName,
        // check the Delegates part of : https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions for why using action
        Action<double>? onProgressPercent = null,
        Action<int, long>? onRemainingChanged = null,
        Action<FileEntry, string, double>? onFileCopied = null)
    {
        // Validate inputs and plan consistency before doing any filesystem work
        if (plan is null) throw new ArgumentNullException(nameof(plan));
        if (jobName is null) throw new ArgumentNullException(nameof(jobName));
        plan.Validate();

        // Ensure the target root exists
        Directory.CreateDirectory(plan.TargetRoot);

        // Track created directories to avoid redundant Directory.Exists/CreateDirectory calls
        // We use a path comparer that matches the OS filesystem casing behavior to be sure it work on every OS
        var createdDirectories = new HashSet<string>(GetPathComparer());

        // Create directories first (keeps empty directories)
        foreach (DirectoryEntry dir in plan.Directories)
        {
            string destDir = Path.Combine(plan.TargetRoot, dir.RelativePath);

            // Create each directory once max
            if (createdDirectories.Add(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
                _logger.LogDirectoryCreation(jobName, destDir);
            }
        }

        // Track remaining work so we can raise progress and remaining events
        long remainingBytes = plan.TotalBytes;
        int remainingFiles = plan.TotalFiles;

        // Copy files
        foreach (FileEntry file in plan.Files)
        {
            string destFile = Path.Combine(plan.TargetRoot, file.RelativePath);
            string? destDir = Path.GetDirectoryName(destFile);

            // Make sure the parent directory exists (some directories may only appear because of files).
            if (!string.IsNullOrEmpty(destDir) && createdDirectories.Add(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
                _logger.LogDirectoryCreation(jobName, destDir);
            }

            // Copy file using streaming in/out (chunks by chunks) and measure transfer time.
            double transferMs = CopyFileWithTiming(file.SourceFullPath, destFile);

            // Update remaining counts
            remainingBytes -= file.LengthBytes; // remove and reassign
            remainingFiles--; // remove one

            // Notify remaining work.
            onRemainingChanged?.Invoke(remainingFiles, remainingBytes);

            // Notify progress as a percentage (guard against division by zero)
            if (plan.TotalBytes > 0)
            {
                double done = (double)(plan.TotalBytes - remainingBytes) / plan.TotalBytes;
                onProgressPercent?.Invoke(done * 100.0);
            }

            // Log the file transfer.
            _logger.LogFileCopy(jobName, file.SourceFullPath, destFile, file.LengthBytes, transferMs);

            // Notify file copied.
            onFileCopied?.Invoke(file, destFile, transferMs);
        }
    }

    private static double CopyFileWithTiming(string sourceFile, string destFile)
    {
        // Simple stopwatch timing for logging/telemetry 
        var stopwatch = Stopwatch.StartNew();

        // Buffer size is a throughput vs memory tradeoff 
        const int bufferSize = 1024 * 256; // 256KB

        // Use sequential scan hints for better in/out behavior on large copies (proposed by AI)
        var sourceOptions = new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            Options = FileOptions.SequentialScan,
            BufferSize = bufferSize,
        };

        // Create/overwrite destination file
        var destOptions = new FileStreamOptions
        {
            Mode = FileMode.Create,
            Access = FileAccess.Write,
            Share = FileShare.None,
            Options = FileOptions.SequentialScan,
            BufferSize = bufferSize,
        };

        // Stream copy to avoid loading the whole file into memory
        using var source = new FileStream(sourceFile, sourceOptions);
        using var destination = new FileStream(destFile, destOptions);

        // Copy with the same buffer size for predictable behavior.
        source.CopyTo(destination, bufferSize);

        // Force flushing to disk (best-effort durability after each file).
        destination.Flush(true);

        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private static IEqualityComparer<string> GetPathComparer()
    {
        // Windows paths are typically case-insensitive; Unix-like systems are typically case-sensitive.
        // Using the right comparer prevents treating the same directory as different keys.
        return OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    }
}
