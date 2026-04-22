using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO.Enumeration;
using System.Reflection.PortableExecutable;
using EasyLog;

public sealed class CopyEngine
{
    private readonly EasyLogger _logger;

    public CopyEngine(EasyLogger logger)
    {
        if(logger == null)
        {
            throw new ArgumentNullException(nameof(logger), LanguageService.T("error.copyengine.logger.null"));
        }
        else
        {
            _logger = logger;
        }
    }

    public void Execute(
        CopyPlan plan,
        string jobName,
        Action<double>? OnProgressPercent = null,
        Action<int, int>? OnRemainingChanged = null,
        Action<FileEntry,string,double>? OnFileCopied = null
    )
    {
        if (plan == null || jobName == null)
        {
            throw new ArgumentNullException(nameof(plan), LanguageService.T("error.copyengine.arguments.null"));
        }

        Directory.CreateDirectory(plan.TargetRoot);

        var createdDirectories = new HashSet<string>(GetPathComparer());

        foreach (DirectoryEntry dir in plan.Directories)
        {
            string destDir = Path.Combine(plan.TargetRoot, dir.RelativePath);

            if(createdDirectories.Add(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
                _logger.LogDirectoryCreation(jobName, destDir);
            }
        }

        int remainingBytes = plan.TotalBytes;
        int remainingFiles = plan.TotalFiles;

        foreach (FileEntry file in plan.Files)
        {
            string destFile = Path.Combine(plan.TargetRoot, file.RelativePath);
            string? destDir = Path.GetDirectoryName(destFile);

            if (!string.IsNullOrEmpty(destDir) && createdDirectories.Add(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
                _logger.LogDirectoryCreation(jobName, destDir);
            }

            double transferMs = CopyFileWithTiming(file.SourceFullPath,destFile);

            remainingBytes -= (int)file.LengthBytes;
            remainingFiles--;

            OnRemainingChanged?.Invoke(remainingFiles, remainingBytes);

            if(plan.TotalBytes > 0)
            {
                double done = (double)(plan.TotalBytes - remainingBytes) / plan.TotalBytes;
                OnProgressPercent?.Invoke(done * 100.0);
            }

            _logger.LogFileCopy(jobName, file.SourceFullPath, destFile, file.LengthBytes, transferMs);

            OnFileCopied?.Invoke(file,destFile,transferMs);
        }
    }

    private static double CopyFileWithTiming(string sourceFile, string destFile)
    {
        var time = Stopwatch.StartNew();
        double elapsedTime = 0.0;

        const int bufferSize = 1024 * 256;

        var sourceOptions = new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            Options = FileOptions.SequentialScan,
            BufferSize = bufferSize, 
        };

        var destOptions = new FileStreamOptions
        {
            Mode = FileMode.Create, // type decide of this
            Access = FileAccess.Write,
            Share = FileShare.None,
            Options = FileOptions.SequentialScan,
            BufferSize = bufferSize,
        };

        using var source = new FileStream(sourceFile,sourceOptions);
        using var destination = new FileStream(destFile, destOptions);

        source.CopyTo(destination,bufferSize);

        destination.Flush(true);

        time.Stop();

        elapsedTime = time.Elapsed.TotalMilliseconds;

        return elapsedTime;
    }

    public static IEqualityComparer<string> GetPathComparer()
    {
        var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        return comparer;
    }
}

