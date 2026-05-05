using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySaveConsole;
public class Joblist
{
    private readonly List<BackupJob> jobs = new ();

    public void AddJob(BackupJob job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job), LanguageService.T("error.joblist.job.null"));
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
        Converters = { new JsonStringEnumConverter() }
    };

    public static Joblist LoadFromStatusFile(string filePathWithExtension)
    {
        var list = new Joblist();
        try
        {
            if (!File.Exists(filePathWithExtension)) return list;
            var json = File.ReadAllText(filePathWithExtension);
            if (string.IsNullOrWhiteSpace(json)) return list;

            var dict = JsonSerializer.Deserialize<Dictionary<string, LogEntryBackupJob>>(json, _jsonOptions);
            if (dict == null) return list;

            foreach (var keyValue in dict)
            {
                var entry = keyValue.Value;
                if (string.IsNullOrWhiteSpace(entry.Name)) entry.Name = keyValue.Key;
                DateTime dateCreated = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(entry.DateCreated) && DateTime.TryParse(entry.DateCreated, out var parsed))
                    dateCreated = parsed;

                var job = new BackupJob(entry.Name, entry.SourceDir ?? "", entry.TargetDir ?? "", true, false, dateCreated);
                list.AddJob(job);
            }
        }
        catch { }
        return list;
    }
}