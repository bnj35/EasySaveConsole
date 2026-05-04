public class LogEntryBackupJob
{
    public string Name { get; set; } = "";
    public JobState State { get; set; }
    public string LastActionTimestamp { get; set; } = "";
    public string SourceDir { get; set; } = "";
    public string TargetDir { get; set; } = "";
    public string DateCreated { get; set; } = "";

    public LogEntryBackupJob() { }

    public LogEntryBackupJob(BackupJob job, JobState state, string DateFormat)
    {
        Name = job.Name;
        State = state;
        SourceDir = job.SourceDir;
        TargetDir = job.TargetDir;
        DateCreated = job.DateCreated.ToString(DateFormat);
        LastActionTimestamp = DateTime.Now.ToString(DateFormat);
    }
}