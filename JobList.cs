public class JobList
{

    private List<BackUpJob> jobs = new List<BackUpJob>();

    public void AddJob(BackUpJob job)

    {

        jobs.Add(job);

        Console.WriteLine($"The job '{job.Name}' has been sucessfuly created.");

    }

    public void DisplayAllJob()
    {
        if (jobs.Count > 0)

        {

            Console.WriteLine("Backup jobs created since the start:");

            foreach (var job in jobs)

            {

                Console.WriteLine($"{job.Name} - Created on {job.DateCreated}");

            }

        }

        else

        {

            Console.WriteLine("The list is empty.");

        }
    }
}

