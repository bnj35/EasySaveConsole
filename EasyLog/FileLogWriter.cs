using System.Text.Json;

public class FileLogWriter : ILogWriter
{
    private readonly string logDirectory;

    public FileLogWriter(string logDirectory)
    {
        this.logDirectory = logDirectory;
        Directory.CreateDirectory(logDirectory);
    }

    /// <summary>
    /// Writes a log entry to the daily log file.
    /// Each entry is written on its own line (JSON Lines format).
    /// </summary>
    public void Write(LogEntry entry)
    {
        string fileName = $"{DateTime.Now:yyyy-MM-dd}.jsonl";
        string fullPath = Path.Combine(logDirectory, fileName);

        string json = JsonSerializer.Serialize(entry, entry.GetType());
        File.AppendAllText(fullPath, json + Environment.NewLine);
    }
}