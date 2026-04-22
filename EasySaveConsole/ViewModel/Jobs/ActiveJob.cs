using EasyLog;
using System;
using System.Collections.Generic;
using System.IO;

// ActiveJob is the runtime "executor" of a backup job
// It plans the copy, runs the engine, tracks progress, and notifies through properties
public class ActiveJob : BackUpJob
{
    private float _totalFileSize;
    // Total bytes computed before starting the transfer
    public float TotalFileSize
    {
        get => _totalFileSize;
        private set => SetProperty(ref _totalFileSize, value);
    }

    private float _numberFiles;
    // Total number of files computed before starting the transfer
    public float NumberFiles
    {
        get => _numberFiles;
        private set => SetProperty(ref _numberFiles, value);
    } // number of file in the Source Directory at the beginning

    private double _progression;
    // Progress percentage (0..100)
    public double Progression
    {
        get => _progression;
        private set => SetProperty(ref _progression, value);
    } // percentage

    private int _numberFileRemaining;
    // Remaining file count
    public int NumberFileRemaining
    {
        get => _numberFileRemaining;
        private set => SetProperty(ref _numberFileRemaining, value);
    }

    private float _sizeFileRemaining;
    // Remaining bytes
    public float SizeFileRemaining
    {
        get => _sizeFileRemaining;
        private set => SetProperty(ref _sizeFileRemaining, value);
    }

    private string? _lastCopiedFileName;
    public string? LastCopiedFileName
    {
        get => _lastCopiedFileName;
        private set => SetProperty(ref _lastCopiedFileName, value);
    }

    private long _lastCopiedBytes;
    public long LastCopiedBytes
    {
        get => _lastCopiedBytes;
        private set => SetProperty(ref _lastCopiedBytes, value);
    }

    private double _lastTransferMilliseconds;
    public double LastTransferMilliseconds
    {
        get => _lastTransferMilliseconds;
        private set => SetProperty(ref _lastTransferMilliseconds, value);
    }

    // Lists of copied paths (useful for reports/debugging)
    public List<string>? AdressesOfSaveFiles { get; set; }
    public List<string>? DestinationOfSaveFiles { get; set; }

    // Logger is used by the underlying copy engine
    private readonly EasyLogger Logger;

    public ActiveJob(string name, string sourceDirectory, string targetDirectory) : base(name, sourceDirectory, targetDirectory)

    {
        // Store base job data (duplicated assignments keep legacy style)
        Name = name;
        SourceDirectory = sourceDirectory;
        TargetDirectory = targetDirectory;

        // Initialize execution metrics
        TotalFileSize = 0;
        NumberFiles = 0;
        AdressesOfSaveFiles = [];
        DestinationOfSaveFiles = [];

        SizeFileRemaining = TotalFileSize;
        Progression = 0.0;
        Logger = EasyLogger.GetInstance();
    }

    public void runJob()
    {
        // Entry point: plan then execute copy
        Console.WriteLine("File copy will start...");

        // Safety: prevent infinite loops like source=/data target=/data/backup
        // Prevent infinite loops like: source=/data, target=/data/backup
        PathGuard.ThrowIfDestinationInsideSource(SourceDirectory, TargetDirectory);

        // Planning step: enumerate files/directories and compute totals
        CopyPlan plan = CopyPlanner.Build(SourceDirectory, TargetDirectory);

        // Initialize totals and remaining counters from the plan
        TotalFileSize = plan.TotalBytes;
        NumberFiles = plan.TotalFiles;
        NumberFileRemaining = (int)NumberFiles;
        SizeFileRemaining = TotalFileSize;

        Console.WriteLine($"Total size before transfer: {TotalFileSize / (1024 * 1024):F2} MB");
        Console.WriteLine($"Number of files: {NumberFiles}");
        Console.WriteLine();

        // Execution step: use CopyEngine to create directories and copy files
        var engine = new CopyEngine(Logger);

        // Run the copy according to the plan
        engine.Execute(
            plan,
            jobName: Name,
            onProgressPercent: percent =>
            {
                Progression = percent;
            },
            onRemainingChanged: (filesRemaining, bytesRemaining) =>
            {
                NumberFileRemaining = filesRemaining;
                SizeFileRemaining = bytesRemaining;
            },
            onFileCopied: (file, destinationPath, transferMs) =>
            {
                AdressesOfSaveFiles?.Add(file.SourceFullPath);
                DestinationOfSaveFiles?.Add(destinationPath);

                LastCopiedFileName = Path.GetFileName(destinationPath);
                LastCopiedBytes = file.LengthBytes;
                LastTransferMilliseconds = transferMs;
            });

        // Summary after completion
        Console.WriteLine();
        long totalCopied = plan.TotalBytes - (long)SizeFileRemaining;
        Console.WriteLine($"Total size after transfer: {totalCopied / (1024 * 1024):F2} MB");
    }
}
