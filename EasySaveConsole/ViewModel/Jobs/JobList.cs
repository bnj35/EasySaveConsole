using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySaveConsole;
public class Joblist
{
    private readonly List<BackupJob> jobs = new ();

    private record JobEntry(string Name, string SourceDir, string TargetDir, bool Type, string DateCreated);

    public void AddJob(BackupJob job)
    {
        if (job == null) {
            throw new ArgumentNullException(nameof(job), LanguageService.T("error.joblist.job.null"));
        }
        jobs.Add(job);
    }

    public IReadOnlyList<BackupJob> GetAllJobs() => jobs;

    public BackupJob? GetByIndex(int index)
    {
        if (index <= 0) return null;
        int realIndex = index - 1;
        return realIndex >= jobs.Count ? null : jobs[realIndex];
    }

    public BackupJob? SearchJob(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        return jobs.FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public BackupJob DeleteJob(int index)
    {
        if (index <= 0) throw new ArgumentOutOfRangeException(nameof(index), LanguageService.T("error.joblist.index.invalid"));
        int realIndex = index - 1;
        if (realIndex >= jobs.Count) throw new ArgumentOutOfRangeException(nameof(index), LanguageService.T("error.joblist.index.invalid"));
        var job = jobs[realIndex];
        jobs.RemoveAt(realIndex);
        return job;
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static Joblist Load(string filePath)
    {
        var list = new Joblist();
        try
        {
            if (!File.Exists(filePath)) {
                return list;
            }
            var json = File.ReadAllText(filePath);

            var entries = JsonSerializer.Deserialize<List<JobEntry>>(json, _jsonOptions);
            if (entries == null) {
                return list;
            }

            foreach (var entry in entries)
            {
                DateTime dateCreated;
                try 
                { 
                    dateCreated = DateTime.Parse(entry.DateCreated); 
                }
                catch 
                { 
                    dateCreated = DateTime.Now;
                }
                list.AddJob(new BackupJob(entry.Name, entry.SourceDir, entry.TargetDir, entry.Type, dateCreated));
            }
        }
        catch {}
        return list;
    }

    public void Save(string filePath)
    {
        var entries = new List<JobEntry>();

        foreach (var job in jobs){
            entries.Add(new JobEntry(job.Name, job.SourceDir, job.TargetDir, job.Type, job.DateCreated.ToString("O")));
        }

        File.WriteAllText(filePath, JsonSerializer.Serialize(entries, _jsonOptions));
    }
}