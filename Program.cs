using System;
using static LanguageService;

class Program
{
    static void Main(string[] args)
    {
        // language selection
        Console.WriteLine("Choose a language / Choisis une langue: \n'en' for english\n'fr' pour français");
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
                    createJob(joblist);

                    break;

                case '2':

                    joblist.displayAllJob();
                    break;

                case '3':

                    Console.WriteLine(T("choice.3"));
                    runJob(joblist);
                    break;

                case '4':
                    searchJob(joblist);

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

    static void createJob(JobList joblist)
    {
        Console.WriteLine(T("create.name"));

        string Name = Console.ReadLine()!;

        Console.WriteLine(T("create.source"));

      string SourceDirectory = VerifyPath(Console.ReadLine()!);

        Console.WriteLine(T("create.target"));

        string TargetDirectory = VerifyPath(Console.ReadLine()!);

        BackUpJob newJob = new BackUpJob(Name, SourceDirectory, TargetDirectory)
        {
            DateCreated = DateTime.Now // set the date of creation
        };

        joblist.AddJob(newJob);

        Console.WriteLine(string.Format(T("create.success"), Name));
    }

    static BackUpJob? searchJob(JobList jobList)

    {
        Console.Write(T("search.prompt"));

        string name = Console.ReadLine()!;

        BackUpJob job = jobList.searchJob(name);

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

    static void runJob(JobList joblist)
    {
        BackUpJob? job = searchJob(joblist);

        if (job != null)
        {
            Console.WriteLine(string.Format(T("run.found"), job.Name));
            ActiveJob jobActive = new ActiveJob(job.Name, job.SourceDirectory, job.TargetDirectory);
            jobActive.runJob();

        }
        // searchJob
    }

    // verify if a path is correct
    static string VerifyPath(string path)
    {
        string newPath = path ?? "";

        while (true)
        {
            if (!string.IsNullOrEmpty(newPath) && Path.IsPathFullyQualified(newPath) && Path.IsPathRooted(newPath))
            {
                return Path.GetFullPath(newPath);
            }
            else
            {
                Console.WriteLine("The path isn't valid, try again :");
                newPath = Console.ReadLine() ?? "";
            }
        }
    }


}

// I follow these steps
/*
- Does the source exist and is valid ?
- Will I overwrite? => type gestion
- Should I log the operation ? => antoine 
- Do I need async?
- Do I need to show progress? => yes
- What if the copy fails? => restart 3 times max => Should I implement retries?
- Are permissions OK?
- Do I need temp-file atomic writes?
- Local vs Network drive?
- Small, medium, or huge files?
*/
