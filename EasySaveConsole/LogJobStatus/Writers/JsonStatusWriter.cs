using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySaveConsole;

public sealed class JsonStatusWriter : IStatusWriter
{
    private readonly string _filePath;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonStatusWriter(string filePath)
    {
        _filePath = filePath;
    }

    public void ResetStatusFile()
    {
        WriteAll(new Dictionary<string, object>());
    }

    public void UpdateEntry(LogEntryBackupJob entry)
    {
        var entries = ReadAll();
        entries[entry.Name] = entry;
        WriteAll(entries);
    }

    private Dictionary<string, object> ReadAll()
    {
        if (!File.Exists(_filePath))
            return new();

        var json = File.ReadAllText(_filePath);

        if (string.IsNullOrWhiteSpace(json))
            return new();

        return JsonSerializer.Deserialize<Dictionary<string, object>>(json, _jsonOptions) ?? new();
    }

    private void WriteAll(Dictionary<string, object> entries)
    {
        File.WriteAllText(_filePath, JsonSerializer.Serialize(entries, _jsonOptions));
    }
}