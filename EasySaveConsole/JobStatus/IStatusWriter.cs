public interface IStatusWriter
{
    public void UpdateJobStatus(BackupJobEntry entry);
    public void ResetJobStatus();
}