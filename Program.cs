using System;
using static LanguageService;

class Program
{
    static void Main(string[] args)
    {
        // language selection
        Console.WriteLine(T("lang.select"));
        string lang = Console.ReadLine()?.Trim().ToLower() ?? "en";
        if (lang != "fr" && lang != "en") lang = "en";
        LanguageService.Load(lang);

        // list of backup jobs
        JobList joblist = new JobList();

        bool exit = false;

        while (!exit)
        {
            DisplayMenu();

            ConsoleKeyInfo choice = Console.ReadKey();

            Console.Clear();

            switch (choice.KeyChar)
            {
                case '0':
                    exit = true;
                    Console.WriteLine(T("job.exit"));
                    break;

                case '1':
                    Console.WriteLine(T("choice.1"));
                    CreateJob(joblist);
                    break;

                case '2':
                    joblist.DisplayAllJob();
                    break;

                case '3':
                    Console.WriteLine(T("choice.3"));
                    break;

                case '4':
                    SearchJob(joblist);
                    break;

                default:
                    Console.WriteLine(T("job.invalid"));
                    break;
            }

            Console.WriteLine(T("job.continue"));
            Console.ReadKey();
            Console.Clear();
        }
    }

    static void DisplayMenu()
    {
        Console.WriteLine(T("menu.title"));
        Console.WriteLine(T("menu.prompt"));
        Console.WriteLine(T("menu.create"));
        Console.WriteLine(T("menu.display"));
        Console.WriteLine(T("menu.run"));
        Console.WriteLine(T("menu.search"));
        Console.WriteLine(T("menu.exit"));
        Console.WriteLine(T("menu.separator"));
        Console.Write(T("menu.choice"));
    }

    static void CreateJob(JobList joblist)
    {
        Console.WriteLine(T("create.name"));

        string Name = Console.ReadLine()!;

        Console.WriteLine(T("create.source"));

        string SourceDirectory = Console.ReadLine()!;

        Console.WriteLine(string.Format(T("create.source.confirm"), SourceDirectory));

        Console.WriteLine(T("create.target"));

        string TargetDirectory = Console.ReadLine()!;

        BackUpJob newJob = new BackUpJob(Name, SourceDirectory, TargetDirectory)
        {
            DateCreated = DateTime.Now // set the date of creation
        };

        joblist.AddJob(newJob);

        Console.WriteLine(string.Format(T("create.success"), Name));
    }

    static BackUpJob? SearchJob(JobList jobList)
    {
        Console.Write(T("search.prompt"));

        string name = Console.ReadLine()!;

        BackUpJob job = jobList.SearchJob(name);

        if (job != null)
        {
            Console.WriteLine(string.Format(T("run.found"), job.Name));
            return job;
        }
        else
        {
            Console.WriteLine(string.Format(T("search.notfound"), name));
            return null;
        }
    }

    static void RunJob(JobList joblist)
    {
        BackUpJob? job = SearchJob(joblist);

        if (job != null)
        {
            Console.WriteLine(string.Format(T("run.found"), job.Name));
            // Here you could call joblist.RunJob(job.Name) to execute
        }
    }
}