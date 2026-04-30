public class StatusLogger
{
    private IStatusWriter _writer;
    public StatusLogger(string filePath, string fileFormat)
    {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));

        string completeFilePath = String.Concat(filePath, fileFormat);
        
        switch (fileFormat)
        {
            case ".xml":
                _writer = new XmlStatusWriter(completeFilePath);
                break;

            default:
                _writer = new JsonStatusWriter(completeFilePath);
                break;
        }
        _writer.ResetJobStatus();
    }

    public void UpdateStatus(BackupJobEntry backupJob)
    {
        _writer.UpdateJobStatus(backupJob);
    }
}