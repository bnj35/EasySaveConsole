using System;
using System.Collections.Generic;
using System.Linq;
using static LanguageService;

public class JobList
{

    private List<BackUpJob> jobs = new List<BackUpJob>();

    public void AddJob(BackUpJob job)

    {

        jobs.Add(job);

        Console.WriteLine(string.Format(T("add.success"), job.Name));

    }

    public void displayAllJob()
    {
        if (jobs.Count > 0)

        {

            Console.WriteLine(T("display.listTitle"));

            foreach (var job in jobs)

            {

                Console.WriteLine($"{job.Name} - Created on {job.DateCreated}");

            }

        }

        else

        {

            Console.WriteLine(T("display.empty"));

        }
    }

    public BackUpJob searchJob(string name)
    {
        return jobs.FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase))!;
    }

    public void RunJob(string name)
    {
        BackUpJob job = searchJob(name);

        if (job != null)
        {
            // jobs.Run(job);
            Console.WriteLine(T("run.running"));
        }
        else
        {
            Console.WriteLine(T("run.notfound"));
        }
    }
}

