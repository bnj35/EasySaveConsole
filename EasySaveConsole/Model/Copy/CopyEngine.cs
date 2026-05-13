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
        private static readonly SemaphoreSlim _bigFileSemaphore = new SemaphoreSlim(2);
        private long BigFileThresholdBytes => (long)Math.Max(1, _settings.BigFileSize) * 1024 * 1024;


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
            ManualResetEventSlim? pauseEvent = null,
            CancellationToken cancellationToken = default
        )
        {
            if (plan == null || jobName == null)
            {
                throw new ArgumentNullException(nameof(plan), LanguageService.T("error.copyengine.arguments.null"));
            }

            if (IsUnixPath(plan.TargetRoot))
            {

                DriveInfo drive = new DriveInfo(plan.TargetRoot);

                long freeSpace = drive.AvailableFreeSpace;
                long requiredSpace = plan.TotalBytes;

                if (freeSpace < requiredSpace)
                {
                    throw new InvalidOperationException(LanguageService.T("error.copyengine.space"));
                }
            }
            Directory.CreateDirectory(plan.TargetRoot);

            string[] excludedProcesses = GetExcludedProcesses();
            using var processPauseEvent = new ManualResetEventSlim(true);
            using var monitorStopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Task? processMonitorTask = excludedProcesses.Length == 0
                ? null
                : MonitorProcesses(excludedProcesses, processPauseEvent, monitorStopCts.Token);

            string[] encryptExtensions = GetEncryptExtensions();

            var createdDirectories = new HashSet<string>(GetPathComparer());
            try
            {
                int remainingBytes = plan.TotalBytes;
                int remainingFiles = plan.TotalFiles;

                foreach (var ent in plan)
                {
                    pauseEvent?.Wait(cancellationToken);
                    processPauseEvent.Wait(cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

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
                            bool isBigFile = file.LengthBytes >= BigFileThresholdBytes;

                            if (isBigFile)
                            {
                                // pour SemaphoreSlim : _bigFileSemaphore.Wait(linkedCts.Token);
                                // pour Semaphore : _bigFileSemaphore.WaitOne();
                                _bigFileSemaphore.Wait(cancellationToken);

                            }

                            try
                            {
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
                                if (isBigFile)
                                {
                                    _bigFileSemaphore.Release();
                                }
                            }
                        }
                        finally
                        {
                            fileLock.ExitWriteLock();
                        }
                    }
                }
            }
            finally
            {
                monitorStopCts.Cancel();

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

        private static async Task MonitorProcesses(string[] excludedProcesses, ManualResetEventSlim processPauseEvent, CancellationToken cancellationToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));

            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                if (IsExcludedProcessRunning(excludedProcesses))
                {
                    processPauseEvent.Reset();
                }
                else
                {
                    processPauseEvent.Set();
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
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex}");
                }
                finally
                {
                    process.Dispose();
                }
            }

            return false;
        }

        private static bool IsUnixPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            // Chemin Unix/Linux/macOS commence par /
            if (path.StartsWith("/"))
            {
                if (path.StartsWith("/Volumes"))
                {
                    return true;
                }
                return false;
            }

            // Chemin Windows: C:\ ou \\server\share
            if (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':')
                return true;

            if (path.StartsWith("\\\\"))
                return true;

            // Par défaut Unix si pas de caractéristiques Windows
            return !path.Contains("\\");
        }
    }
}
