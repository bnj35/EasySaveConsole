using System;
using System.Collections.Generic;
using System.Linq;

public class JobList
{
    public const int MaxJobs = 5;

    // In-memory list of jobs created/loaded during this app session.
    private readonly List<BackUpJob> jobs = new();

    public void AddJob(BackUpJob job)
    {
        // Guard against null jobs.
        if (job is null) throw new ArgumentNullException(nameof(job));

        if (jobs.Count >= MaxJobs)
        {
            throw new InvalidOperationException($"Maximum number of jobs reached ({MaxJobs}).");
        }

        jobs.Add(job);
    }

    public void AddJobs(IEnumerable<BackUpJob> jobsToAdd)
    {
        // Convenience method to bulk-add jobs (after loading from storage)
        if (jobsToAdd is null) throw new ArgumentNullException(nameof(jobsToAdd));
        foreach (BackUpJob job in jobsToAdd)
        {
            AddJob(job);
        }
    }

    public IReadOnlyList<BackUpJob> GetAllJobs() => jobs;

    public BackUpJob? GetByIndex(int index1Based)
    {
        // 1-based indexing (1 = first job). for 1-3 or 1;3 later
        if (index1Based <= 0) return null;
        int index0 = index1Based - 1;
        if ((uint)index0 >= (uint)jobs.Count) return null;
        return jobs[index0];
    }

    public BackUpJob? SearchJob(string name)
    {
        // Simple linear search by name (case-insensitive).
        if (string.IsNullOrWhiteSpace(name)) return null;
        return jobs.FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
