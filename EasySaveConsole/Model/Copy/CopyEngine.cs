using System.Diagnostics;
using EasyLog;
using EasySaveConsole;
public sealed class CopyEngine
{
    private readonly EasyLogger _logger;

    public CopyEngine(Settings settings)
    {
        _logger = EasyLogger.GetInstance(settings.EasyLogSettings.DirectoryPath, settings.DateFormat, settings.DefaultFileFormat);
    }

    public void Execute(
        CopyPlan plan,
        string jobName,
        bool type,
        bool encrypt,
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

            double transferMs = CopyFileWithTiming(file.SourceFullPath,destFile, type, encrypt);

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

    private static double CopyFileWithTiming(string sourceFile, string destFile, bool type, bool encrypt)
    {
        var time = Stopwatch.StartNew();
        double elapsedTime = 0.0;

        if (encrypt)
        {
            CryptoSoftRunner.Encrypt(sourceFile, destFile);
        }
        else
        {
            const int bufferSize = 1024 * 256;

        var sourceOptions = new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            Options = FileOptions.SequentialScan,
            BufferSize = bufferSize, 
        };

        bool destinationExists = File.Exists(destFile);

        FileMode destinationMode = type
            ? (destinationExists ? FileMode.Create : FileMode.CreateNew)
            : FileMode.Create;

        var destOptions = new FileStreamOptions
        {
            Mode = destinationMode,
            Access = FileAccess.Write,
            Share = FileShare.None,
            Options = FileOptions.SequentialScan,
            BufferSize = bufferSize,
        };

        using var source = new FileStream(sourceFile,sourceOptions);
        using var destination = new FileStream(destFile, destOptions);

        source.CopyTo(destination,bufferSize);

        destination.Flush(true);
        }

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

