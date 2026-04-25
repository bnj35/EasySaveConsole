using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public sealed class ActiveJobEntry : BackupJobEntry
{
    public int TotalFiles { get; set; }
    public float TotalBytes { get; set; }
    public double ProgressPercent { get; set; }
    public int FilesRemaining { get; set; }
    public float BytesRemaining { get; set; }
    public string CurrentSourceFile { get; set; } = "";
    public string CurrentDestFile { get; set; } = "";

    public ActiveJobEntry() { }

    public ActiveJobEntry(ActiveJob job, JobState state, string currentSource, string currentDest) : base(job, state)
    {
        TotalFiles = job.NumberFiles;
        TotalBytes = job.TotalFileSize;
        ProgressPercent = job.Progression;
        FilesRemaining = job.NumberFilesRemaining;
        BytesRemaining = job.SizeFileRemaining;
        CurrentSourceFile = currentSource;
        CurrentDestFile = currentDest;
    }
}