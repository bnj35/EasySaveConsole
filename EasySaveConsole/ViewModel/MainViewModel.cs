using System.ComponentModel;

public sealed class MainViewModel
{
    private readonly Joblist _jobList;

    private StatusFileWriter statusWriter;

    private Settings Settings;

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
        string statusFileName = settings.StatusFileSettings.Name + settings.StatusFileSettings.Format;
        statusWriter = new StatusFileWriter(statusFileName);
    }

    public BackupJob CreateJob(string name, string source_dir, string target_dir, bool type)
    {
        BackupJob newjob = new BackupJob(name, source_dir, target_dir, type, DateTime.Now, statusWriter);

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
        return new ActiveJob(job.Name,job.SourceDir,job.TargetDir,job.Type,job.DateCreated, statusWriter, Settings.EasyLogSettings);
    }


}