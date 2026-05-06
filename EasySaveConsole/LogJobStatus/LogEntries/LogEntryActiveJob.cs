using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EasySaveConsole;

public sealed class LogEntryActiveJob : LogEntryBackupJob
{
    public int TotalFiles { get; set; }
    public float TotalBytes { get; set; }
    public double ProgressPercent { get; set; }
    public int FilesRemaining { get; set; }
    public float BytesRemaining { get; set; }
    public string CurrentSourceFile { get; set; } = "";
    public string CurrentDestFile { get; set; } = "";

    public LogEntryActiveJob() { }

    public LogEntryActiveJob(ActiveJob job, JobState state, string dateFormat, string currentSource, string currentDest) : base(job, state, dateFormat)
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