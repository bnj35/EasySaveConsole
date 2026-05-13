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
        _jobList.Save(Settings.JobsFilePath);
        _statusLogger.Update(_jobList.GetAllJobs());

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
    public void RunJob(ActiveJob active)
    {
        void OnFileCopied()
        {
            _statusLogger.Update(_jobList.GetAllJobs(), active);
            Console.WriteLine($"[Copie terminée] {Path.GetFileName(active.AddressesOfFiles?.LastOrDefault())}");
        }
        active.FileCopied += OnFileCopied;
        active.RunJob(_copyEngine, Settings.PriorityExtensions);
        active.FileCopied -= OnFileCopied;
        _statusLogger.Update(_jobList.GetAllJobs());
    }

    public void DeleteJob(int index)
    {
        _jobList.DeleteJob(index);
        _jobList.Save(Settings.JobsFilePath);
    }
}