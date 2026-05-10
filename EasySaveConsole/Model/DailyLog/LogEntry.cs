
public record LogEntry(
    string Action,
    string Name,
    string Time
);

public record FileLogEntry(
    string Action, string Name, string Time,
    string FileSource,
    string FileTarget,
    long FileSize,
    double FileTransfertTime,
    double EncryptionTime
) : LogEntry(Action, Name, Time);

public record DirectoryLogEntry(
    string Action, string Name, string Time,
    string Target
) : LogEntry(Action, Name, Time);