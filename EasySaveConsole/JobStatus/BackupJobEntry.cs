public class BackupJobEntry
{
    public string Name { get; set; } = "";
    public JobState State { get; set; }
    public string LastActionTimestamp { get; set; } = "";
    public string SourceDir { get; set; } = "";
    public string TargetDir { get; set; } = "";
    public string DateCreated { get; set; } = "";

    public BackupJobEntry() { }

    public BackupJobEntry(BackupJob job, JobState State)
    {
        Name = job.Name;
        this.State = State;
        SourceDir = job.SourceDir;
        TargetDir = job.TargetDir;
        DateCreated = job.DateCreated.ToString("dd/MM/yyyy HH:mm:ss");
        LastActionTimestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    }
}