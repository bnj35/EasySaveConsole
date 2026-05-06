namespace EasySaveConsole;
public interface IStatusWriter
{
    public void UpdateEntry(LogEntryBackupJob entry);
    public void ResetStatusFile();
}