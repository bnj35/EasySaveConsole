namespace EasySaveConsole;
public enum JobState
{
    Inactive,
    Active
}
public class StatusLogger
{
    private IStatusWriter _writer;
    private Settings _settings;
    public StatusLogger(Settings settings, string fileFormat)
    {
        _settings = settings;
        string completeFilePath = $"{settings.StatusFileSettings.FilePath}.{fileFormat}";

        switch (fileFormat)
        {
            case "xml":
                _writer = new XmlStatusWriter(completeFilePath);
                break;

            default:
                _writer = new JsonStatusWriter(completeFilePath);
                break;
        }
        if (!File.Exists(completeFilePath))
        {
            _writer.ResetStatusFile();
        }
    }
    public void UpdateInactiveJob(BackupJob job)
    {
        _writer.UpdateEntry(new LogEntryBackupJob(job, JobState.Inactive, _settings.DateFormat));
    }

    public void UpdateActiveJob(ActiveJob job, string sourcePath, string destinationPath)
    {
        _writer.UpdateEntry(new LogEntryActiveJob(job, JobState.Active, _settings.DateFormat, sourcePath, destinationPath));
    }
}