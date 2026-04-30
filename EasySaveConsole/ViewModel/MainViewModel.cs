using System.ComponentModel;

public sealed class MainViewModel
{
    private readonly Joblist _jobList;

    private StatusLogger _statusWriter;

    private Settings _settings;

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
        _settings = settings;
        _statusWriter = new StatusLogger(settings.StatusFileSettings.FilePath, settings.StatusFileSettings.FileFormat);
    }

    public BackupJob CreateJob(string name, string source_dir, string target_dir, bool type)
    {
        BackupJob newjob = new BackupJob(name, source_dir, target_dir, type, DateTime.Now, _statusWriter);

        _jobList.AddJob(newjob);

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
        return new ActiveJob(job.Name,job.SourceDir,job.TargetDir,job.Type,job.DateCreated, _statusWriter, _settings.EasyLogSettings);
    }


}