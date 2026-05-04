namespace EasySaveConsole;
public class Joblist
{
    // public const int MaxJobs = 5;
    
    private readonly List<BackupJob> jobs = new ();

    public void AddJob(BackupJob job)
    {
        if (job == null)
        {
            throw new ArgumentNullException(nameof(job), LanguageService.T("error.joblist.job.null"));
        }
        // if(jobs.Count >= MaxJobs)
        // {
        //     throw new InvalidOperationException(string.Format(LanguageService.T("error.joblist.max.reached"), MaxJobs));
        // }

        jobs.Add(job);
    }

    public IReadOnlyList<BackupJob> GetAllJobs()
    {
        return jobs;
    }

    public BackupJob? GetByIndex(int index)
    {
        if (index <= 0)
        {
            return null;
        }
        else
        {
            int realIndex = index - 1;
            if (realIndex >= jobs.Count)
            {
                return null;
            }
            else
            {
                return jobs[realIndex];
            }
        }
    }

    public BackupJob? SearchJob(string name)
    {
        bool nameClean = string.IsNullOrWhiteSpace(name);
        if (nameClean)
        {
            return null;
        }
        else
        {
            return jobs.FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}