using System.Text.Json.Serialization;

public abstract record LogEntry(
    [property: JsonPropertyOrder(0)] string Action,
    [property: JsonPropertyOrder(1)] string Name,
    [property: JsonPropertyOrder(2)] string Time
);

public record FileLogEntry(
    string Action, string Name, string Time,
    [property: JsonPropertyOrder(3)] string FileSource,
    [property: JsonPropertyOrder(4)] string FileTarget,
    [property: JsonPropertyOrder(5)] long FileSize,
    [property: JsonPropertyOrder(6)] double FileTransfertTime,
    [property: JsonPropertyOrder(7)] double EncryptionTime
) : LogEntry(Action, Name, Time);

public record DirectoryLogEntry(
    string Action, string Name, string Time,
    [property: JsonPropertyOrder(3)] string Target
) : LogEntry(Action, Name, Time);