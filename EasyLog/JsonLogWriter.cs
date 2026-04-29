using System.Text.Json;

public class JsonLogWriter : ILogWriter
{
    private readonly string logDirectory;

    public JsonLogWriter(string logDirectory)
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
        string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";
        string fullPath = Path.Combine(logDirectory, fileName);
        List<JsonElement> entries = [];
        if (File.Exists(fullPath))
        {
            entries = JsonSerializer.Deserialize<List<JsonElement>>(File.ReadAllText(fullPath)) ?? [];
        }

        entries.Add(JsonSerializer.SerializeToElement(entry, entry.GetType()));

        string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(fullPath, json);
    }
}