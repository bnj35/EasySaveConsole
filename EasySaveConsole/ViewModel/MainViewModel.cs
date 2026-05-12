using System.ComponentModel;

namespace EasySaveConsole;

public sealed class MainViewModel
{
    private readonly Joblist _jobList;

    private readonly StatusLogger _statusLogger;

    private readonly CopyEngine _copyEngine;

    public string StatusMessage { get; set; } = "Ready";
    public Settings Settings { get; }
    public string Language { get; set; } = "en";

    public MainViewModel(Joblist jobList, Settings settings)
    {
        if (jobList == null)
        {
        throw new ArgumentNullException(nameof(jobList), LanguageService.T("error.viewmodel.joblist.null"));
        }
        else
        {
            _jobList = jobList;
        }
        Settings = settings;
        _statusLogger = new StatusLogger(settings);
        _copyEngine = new CopyEngine(settings);
    }

    public BackupJob CreateJob(string name, string source_dir, string target_dir, bool type)
    {
        BackupJob newjob = new BackupJob(name, source_dir, target_dir, type, DateTime.Now);

        _jobList.AddJob(newjob);

        _statusLogger.UpdateInactiveJob(newjob, false);

        return newjob;
    }

    public IReadOnlyList<BackupJob> GetAllJobs()
    {
        return _jobList.GetAllJobs();
    }
    public BackupJob? SearchJob(string name)
    {
        return _jobList.SearchJob(name);
    }
    public BackupJob? GetJobByIndex(int index)
    {
        return _jobList.GetByIndex(index);
    }
    public ActiveJob CreateActiveJob(BackupJob job)
    {
        if(job == null)
        {
            throw new ArgumentNullException(nameof(job), LanguageService.T("error.viewmodel.job.null"));
        }
        return new ActiveJob(job.Name, job.SourceDir, job.TargetDir, job.Type, job.DateCreated);
    }
    public bool CanExecuteJob(BackupJob job)
    {
        var priorityExtensions = CopyPlanner.ParsePriorityExtensions(Settings.PriorityExtensions);
        return PriorityFileManager.CanExecuteNonPriorityJob(job, _jobList, priorityExtensions);
    }
    public string GetBlockingReason(BackupJob job)
    {
        if (CanExecuteJob(job))
            return string.Empty;

        var priorityExtensions = CopyPlanner.ParsePriorityExtensions(Settings.PriorityExtensions);
        var blockingJobs = PriorityFileManager.GetJobsWithPriorityFiles(_jobList, priorityExtensions);
        
        if (blockingJobs.Count == 0)
            return "Extensions prioritaires en attente.";
        
        var jobNames = string.Join(", ", blockingJobs.Select(j => $"'{j.Name}'"));
        return $"Fichiers prioritaires en attente sur : {jobNames}";
    }
    public void RunJob(ActiveJob active)
    {
        var priorityExtensions = CopyPlanner.ParsePriorityExtensions(Settings.PriorityExtensions);
        
        if (!PriorityFileManager.CanExecuteNonPriorityJob(active, _jobList, priorityExtensions))
        {
            var blockingReason = GetBlockingReason(active);
            throw new InvalidOperationException($"Impossible d'exécuter '{active.Name}'. {blockingReason}");
        }

        void OnFileCopied(string source, string dest) => _statusLogger.UpdateActiveJob(active, source, dest);
        active.FileCopied += OnFileCopied;
        
        active.IsRunning = true;
        
        try
        {
            active.RunJobWithPriority(_copyEngine, priorityExtensions);
        }
        finally
        {
            active.IsRunning = false;
            active.FileCopied -= OnFileCopied;
            _statusLogger.UpdateInactiveJob(active, true);
        }
    }

    public void DeleteJob(int index)
    {
        BackupJob? deletedJob = _jobList.DeleteJob(index);
    }
}