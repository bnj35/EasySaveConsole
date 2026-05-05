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
            string existingJson = File.ReadAllText(fullPath);

            if (!string.IsNullOrWhiteSpace(existingJson))
            {
                try
                {
                    entries = JsonSerializer.Deserialize<List<JsonElement>>(existingJson) ?? [];
                }
                catch (JsonException)
                {
                    entries = [];
                }
            }
        }

        entries.Add(JsonSerializer.SerializeToElement(entry, entry.GetType()));

        string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
        string tempPath = Path.Combine(logDirectory, $".{fileName}.{Guid.NewGuid():N}.tmp");
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, fullPath, true);
    }
}