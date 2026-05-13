using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using EasyLog;

namespace EasySaveConsole;
public class StatusLogger {

    private readonly Settings _settings;
    private readonly Dictionary<string, LogEntryBackupJob> _jobs;
    private readonly JsonSerializerOptions _jsonOptions;

    public StatusLogger(Settings settings)
    {
        _settings = settings;
        _jobs = [];
        _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public void Update(IReadOnlyList<BackupJob> jobs, ActiveJob? runningJob = null)
    {
        foreach (var job in jobs)
        {
            if (runningJob != null && job.Name == runningJob.Name)
            {
                _jobs[job.Name] = new LogEntryActiveJob(runningJob, JobState.Active, _settings.DateFormat);
            }
            else
            {
                _jobs[job.Name] = new LogEntryBackupJob(job, JobState.Inactive, _settings.DateFormat);
            }
        }
        Save();
    }

    private void Save()
    {
        LogFileFormats format = _settings.LogFileFormat;
        string basePath = _settings.StatusFileSettings.FilePath;
        string wrongExtension = format == LogFileFormats.xml ? "json" : "xml";
        string wrongPath = $"{basePath}.{wrongExtension}";

        if (File.Exists(wrongPath)){
            File.Delete(wrongPath);
        }

        string path = $"{basePath}.{format}";
        if (format == LogFileFormats.xml) {
            WriteXml(path);
        }
        else {
            WriteJson(path);
        }
    }

    private void WriteJson(string path)
    {
        var asObjects = _jobs.ToDictionary(k => k.Key, v => (object)v.Value);
        File.WriteAllText(path, JsonSerializer.Serialize(asObjects, _jsonOptions));
    }

    private void WriteXml(string path)
    {
        var root = new XElement("Jobs");
        foreach (var entry in _jobs.Values)
        {
            var job = new XElement("Job",
                new XElement("Name", entry.Name),
                new XElement("State", entry.State),
                new XElement("LastActionTimestamp", entry.LastActionTimestamp),
                new XElement("SourceDir", entry.SourceDir),
                new XElement("TargetDir", entry.TargetDir),
                new XElement("DateCreated", entry.DateCreated)
            );
            if (entry is LogEntryActiveJob active)
            {
                job.Add(
                    new XElement("TotalFiles", active.TotalFiles),
                    new XElement("TotalBytes", active.TotalBytes),
                    new XElement("ProgressPercent", active.ProgressPercent),
                    new XElement("FilesRemaining", active.FilesRemaining),
                    new XElement("BytesRemaining", active.BytesRemaining),
                    new XElement("CurrentSourceFile", active.CurrentSourceFile),
                    new XElement("CurrentDestFile", active.CurrentDestFile)
                );
            }
            root.Add(job);
        }
        new XDocument(root).Save(path);
    }
}
