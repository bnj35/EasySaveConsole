using System.ComponentModel;

public sealed class MainViewModel
{
    private readonly Joblist _jobList;

    public MainViewModel(Joblist jobList)
    {
        if (jobList == null)
        {
        throw new ArgumentNullException(nameof(jobList), LanguageService.T("error.viewmodel.joblist.null"));
        }
        else
        {
            _jobList = jobList;
        }
    }

    public BackupJob CreateJob(string name, string source_dir, string target_dir, bool type)
    {
        BackupJob newjob = new BackupJob(name, source_dir, target_dir, type, DateTime.Now);

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
        return new ActiveJob(job.Name,job.SourceDir,job.TargetDir,job.Type,job.DateCreated);
    }


}