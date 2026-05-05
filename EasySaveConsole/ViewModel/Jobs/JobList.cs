namespace EasySaveConsole;
public class Joblist
{
    private readonly List<BackupJob> jobs = new ();

    public void AddJob(BackupJob job)
    {
        if (job == null)
        {
            throw new ArgumentNullException(nameof(job), LanguageService.T("error.joblist.job.null"));
        }

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

    public BackupJob DeleteJob(int index)
    {
        if (index <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), LanguageService.T("error.joblist.index.invalid"));
        }
        else
        {
            int realIndex = index - 1;
            if (realIndex >= jobs.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), LanguageService.T("error.joblist.index.invalid"));
            }
            else
            {
                BackupJob jobToDelete = jobs[realIndex];
                jobs.RemoveAt(realIndex);
                return jobToDelete;
            }
        }
    }
}