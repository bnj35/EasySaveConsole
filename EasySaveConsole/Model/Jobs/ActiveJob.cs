using EasyLog;
using System;
using System.Collections.Generic;
using System.Linq;

// ActiveJob is the runtime "executor" of a backup job.
// It plans the copy, runs the engine, tracks progress and forwards engine events.
public class ActiveJob : BackUpJob
{
    // Total bytes computed before starting the transfer
    public float TotalFileSize { get; set; }

    // Total number of files computed before starting the transfer
    public float NumberFiles { get; set; } // number of file in the Source Directory at the beginning

    // Progress percentage (0..100)
    public double Progression { get; set; } = 0.0; // percentage

    // Remaining file count
    public int NumberFileRemaining { get; set; }

    // Remaining bytes
    public float SizeFileRemaining { get; set; }

    // Lists of copied paths (useful for reports/debugging)
    public List<string>? AdressesOfSaveFiles { get; set; }
    public List<string>? DestinationOfSaveFiles { get; set; }

    // Logger is used by the underlying copy engine
    private readonly EasyLogger Logger;

    // Public events: the View can subscribe to update the UI
    public event EventHandler<CopyProgressChangedEventArgs>? ProgressChanged;
    public event EventHandler<CopyRemainingChangedEventArgs>? RemainingChanged;
    public event EventHandler<FileCopiedEventArgs>? FileCopied;

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

        // Forward engine events to observers (View/ViewModel)
        // Forward engine events to observers (typically the ViewModel)
        engine.ProgressChanged += (_, e) =>
        {
            Progression = e.ProgressPercent;
            ProgressChanged?.Invoke(this, e);
        };

        engine.RemainingChanged += (_, e) =>
        {
            NumberFileRemaining = e.FilesRemaining;
            SizeFileRemaining = e.BytesRemaining;
            RemainingChanged?.Invoke(this, e);
        };

        engine.FileCopied += (_, e) =>
        {
            // Track copied paths and forward the event
            AdressesOfSaveFiles?.Add(e.SourcePath);
            DestinationOfSaveFiles?.Add(e.DestinationPath);
            FileCopied?.Invoke(this, e);
        };

        // Run the copy according to the plan
        engine.Execute(
            plan,
            jobName: Name,
            onProgressPercent: null,
            onRemainingChanged: null,
            onFileCopied: null);

        // Summary after completion
        Console.WriteLine();
        long totalCopied = plan.TotalBytes - (long)SizeFileRemaining;
        Console.WriteLine($"Total size after transfer: {totalCopied / (1024 * 1024):F2} MB");
    }
}
