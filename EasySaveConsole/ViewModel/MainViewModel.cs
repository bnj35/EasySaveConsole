using System;
using System.Collections.Generic;

// ViewModel: exposes app operations to the View (console now, GUI later)
// It keeps UI concerns out of the Model and centralizes orchestration
public sealed class MainViewModel
{
    private readonly JobList _jobList;

    public MainViewModel(JobList jobList)
    {
        // The ViewModel needs a JobList to store/manage jobs
        _jobList = jobList ?? throw new ArgumentNullException(nameof(jobList)); // ?? if not null return it if null return the right value
    }

    public BackUpJob CreateJob(string name, string sourceDirectory, string targetDirectory)
    {
        // Create a job with user-provided properties, and stamp the creation date
        var newJob = new BackUpJob(name, sourceDirectory, targetDirectory)
        {
            DateCreated = DateTime.Now,
        };

        // Persist in memory via the JobList
        _jobList.AddJob(newJob);
        return newJob;
    }

    // Return all jobs as read-only data for the View
    public IReadOnlyList<BackUpJob> GetAllJobs() => _jobList.GetAllJobs();

    // Search by name and return null if not found
    public BackUpJob? SearchJob(string name) => _jobList.SearchJob(name);

    // Get a job by 1-based index (1 = first job). Returns null if out of range.
    public BackUpJob? GetJobByIndex(int index1Based) => _jobList.GetByIndex(index1Based);

    public ActiveJob CreateActiveJob(BackUpJob job)
    {
        // ActiveJob is the runtime object that performs the copy and emits events
        if (job is null) throw new ArgumentNullException(nameof(job));
        return new ActiveJob(job.Name, job.SourceDirectory, job.TargetDirectory);
    }
}
