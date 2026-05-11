using System.Diagnostics;
using EasyLog;

namespace EasySaveConsole
{
    public sealed class CopyEngine
    {
        private readonly Logger _logger;
        private readonly Settings _settings;
        private static readonly object _lockSync = new object(); // lock pour éviter les conflits d'accès
        private static readonly Dictionary<string, ReaderWriterLockSlim> _pathLocks = new();



        public CopyEngine(Settings settings)
        {
            _logger = new Logger(settings);
            _settings = settings;
        }

        public void Execute(
            CopyPlan plan,
            string jobName,
            bool type,
            Action<double>? OnProgressPercent = null,
            Action<int, int>? OnRemainingChanged = null,
            Action<FileEntry, string, double, double>? OnFileCopied = null,
            CancellationToken cancellationToken = default
        )
        {
            if (plan == null || jobName == null)
            {
                throw new ArgumentNullException(nameof(plan), LanguageService.T("error.copyengine.arguments.null"));
            }

            Directory.CreateDirectory(plan.TargetRoot);

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            string[] excludedProcesses = GetExcludedProcesses();
            Task? processMonitorTask = excludedProcesses.Length == 0
                ? null
                : CheckProcess(excludedProcesses, linkedCts);

            string[] encryptExtensions = GetEncryptExtensions();

            var createdDirectories = new HashSet<string>(GetPathComparer());
            try
            {
                int remainingBytes = plan.TotalBytes;
                int remainingFiles = plan.TotalFiles;

                foreach (var ent in plan)
                {
                    linkedCts.Token.ThrowIfCancellationRequested();

                    if (ent is DirectoryEntry dir)
                    {
                        string destDir = Path.Combine(plan.TargetRoot, dir.RelativePath);
                        var pathLock = GetPathLock(destDir);

                        pathLock.EnterWriteLock();
                        try
                        {
                            if (createdDirectories.Add(destDir) && !Directory.Exists(destDir))
                            {
                                Directory.CreateDirectory(destDir);
                                _logger.LogDirectoryCreation(jobName, destDir);
                            }
                            remainingFiles--;
                            OnRemainingChanged?.Invoke(remainingFiles, remainingBytes);
                        }
                        finally
                        {
                            pathLock.ExitWriteLock();
                        }
                    }
                    else if (ent is FileEntry file)
                    {
                        string destFile = Path.Combine(plan.TargetRoot, file.RelativePath);
                        string? destDir = Path.GetDirectoryName(destFile);

                        // Lock du répertoire
                        if (!string.IsNullOrEmpty(destDir))
                        {
                            var dirLock = GetPathLock(destDir);
                            dirLock.EnterWriteLock();
                            try
                            {
                                if (createdDirectories.Add(destDir) && !Directory.Exists(destDir))
                                {
                                    Directory.CreateDirectory(destDir);
                                    _logger.LogDirectoryCreation(jobName, destDir);
                                }
                            }
                            finally
                            {
                                dirLock.ExitWriteLock();
                            }
                        }

                        // Lock du fichier
                        var fileLock = GetPathLock(destFile);
                        fileLock.EnterWriteLock();
                        try
                        {
                            bool shouldEncrypt = IsExtensionToEncrypt(file.SourceFullPath, encryptExtensions);
                            var (transferMs, encryptMs) = CopyFileWithTiming(file.SourceFullPath, destFile, type, shouldEncrypt);

                            remainingBytes -= (int)file.LengthBytes;
                            remainingFiles--;

                            OnRemainingChanged?.Invoke(remainingFiles, remainingBytes);

                            if (plan.TotalBytes > 0)
                            {
                                double done = (double)(plan.TotalBytes - remainingBytes) / plan.TotalBytes;
                                OnProgressPercent?.Invoke(done * 100.0);
                            }

                            _logger.LogFileCopy(jobName, file.SourceFullPath, destFile, file.LengthBytes, transferMs, encryptMs);
                            OnFileCopied?.Invoke(file, destFile, transferMs, encryptMs);
                        }
                        finally
                        {
                            fileLock.ExitWriteLock();
                        }
                    }
                }

            }
            catch (OperationCanceledException) when (linkedCts.IsCancellationRequested)
            {
                throw new OperationCanceledException(LanguageService.T("settings.excludes.processes.exit"), linkedCts.Token);
            }
            finally
            {
                linkedCts.Cancel();

                if (processMonitorTask != null)
                {
                    try
                    {
                        processMonitorTask.GetAwaiter().GetResult();
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }
        }

        private static (double transferMs, double encryptMs) CopyFileWithTiming(string sourceFile, string destFile, bool type, bool encrypt)
        {
            var time = Stopwatch.StartNew();
            double elapsedTime = 0.0;
            double encryptTime = 0.0;

            if (encrypt)
            {
                encryptTime = CryptoSoftRunner.Encrypt(sourceFile, destFile);
                time.Stop();
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

                using var source = new FileStream(sourceFile, sourceOptions);
                using var destination = new FileStream(destFile, destOptions);

                source.CopyTo(destination, bufferSize);

                destination.Flush(true);
            }

            time.Stop();

            elapsedTime = time.Elapsed.TotalMilliseconds;

            return (elapsedTime, encryptTime);
        }

        private static ReaderWriterLockSlim GetPathLock(string path)
        {
            lock (_lockSync)
            {
                string normalized = Path.GetFullPath(path).ToLowerInvariant();
                if (!_pathLocks.TryGetValue(normalized, out var lockObj))
                {
                    lockObj = new ReaderWriterLockSlim();
                    _pathLocks[normalized] = lockObj;
                }
                return lockObj;
            }
        }

        public static IEqualityComparer<string> GetPathComparer()
        {
            var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            return comparer;
        }

        private string[] GetEncryptExtensions()
        {
            string? exts = _settings.EncryptExtensions;
            if (!string.IsNullOrWhiteSpace(exts))
            {
                return exts.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => e.Trim().ToLowerInvariant())
                            .ToArray();
            }
            return Array.Empty<string>();
        }

        private static bool IsExtensionToEncrypt(string filePath, string[] encryptExtensions)
        {
            if (encryptExtensions.Length == 0) return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            // Tolère si l'utilisateur a écrit "txt" sans le point "."
            return encryptExtensions.Any(ext => extension == ext || extension == "." + ext);
        }


        private string[] GetExcludedProcesses()
        {
            string excludedProcesses = _settings.ProcessExclusionSettings.ExcludedProcesses;

            if (string.IsNullOrWhiteSpace(excludedProcesses))
            {
                return [];
            }

            return excludedProcesses
                .Split(new[] { '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().ToLowerInvariant())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();
        }

        private static async Task CheckProcess(string[] excludedProcesses, CancellationTokenSource cancellationSource)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));

            while (!cancellationSource.IsCancellationRequested && await timer.WaitForNextTickAsync(cancellationSource.Token).ConfigureAwait(false))
            {
                if (IsExcludedProcessRunning(excludedProcesses))
                {
                    cancellationSource.Cancel();
                    return;
                }
            }
        }

        private static bool IsExcludedProcessRunning(IEnumerable<string> excludedProcesses)
        {
            HashSet<string> processNames = excludedProcesses.ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (processNames.Count == 0)
            {
                return false;
            }

            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    string processName = process.ProcessName.ToLowerInvariant();

                    if (processNames.Contains(processName))
                    {
                        return true;
                    }
                }
                catch
                {
                }
                finally
                {
                    process.Dispose();
                }
            }

            return false;
        }
    }
}
