using EasyLog;
using Microsoft.Extensions.Configuration;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Choose a language / Choisis une langue: \n'en' for english\n'fr' pour français");

        string lang = (Console.ReadLine() ?? "en").Trim().ToLowerInvariant();

        if (lang != "fr" && lang != "en")
        {
            lang = "en";
        }

        LanguageService.Load(lang);
        
        Console.WriteLine(LanguageService.T("log.select"));

        string logFileFormat = (Console.ReadLine() ?? "json").Trim().ToLowerInvariant();

        Settings settings = GetConfiguration();

        if (logFileFormat != "xml" && logFileFormat != "json")
        {
            logFileFormat = settings.DefaultFileFormat;
            Console.WriteLine(LanguageService.T("log.invalid"));
        }

        Joblist joblist = new Joblist();

        MainViewModel vm = new MainViewModel(joblist, settings, logFileFormat);

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
                    Console.WriteLine(LanguageService.T("job.exit"));
                    break;

                case '1':
                    Console.WriteLine(LanguageService.T("choice.1"));
                    // if (vm.GetAllJobs().Count >= Joblist.MaxJobs)
                    // {
                    //     Console.WriteLine(string.Format(LanguageService.T("job.max.reached"), Joblist.MaxJobs));
                    //     break;
                    // }
                    CreateJob(vm);
                    break;

                case '2':
                    DisplayAllJobs(vm);
                    break;

                case '3':
                    SearchJob(vm);
                    break;
                
                case '4':
                    Console.WriteLine(LanguageService.T("choice.4"));
                    RunMultipleJob(vm);
                    break;

                default:
                    Console.WriteLine(LanguageService.T("job.invalid"));
                    break;
            }

            Console.WriteLine(LanguageService.T("job.continue"));
            Console.ReadKey();
            Console.Clear();
        }
    }

    static void DisplayMenu()
    {
        Console.WriteLine(LanguageService.T("menu.title"));
        Console.WriteLine(LanguageService.T("menu.prompt"));
        Console.WriteLine(LanguageService.T("menu.create"));
        Console.WriteLine(LanguageService.T("menu.display"));
        Console.WriteLine(LanguageService.T("menu.search"));
        Console.WriteLine(LanguageService.T("menu.runMultiple"));
        Console.WriteLine(LanguageService.T("menu.exit"));
        Console.WriteLine(LanguageService.T("menu.separator"));
        Console.Write(LanguageService.T("menu.choice"));
    }

    static void CreateJob(MainViewModel vm)
    {
        Console.WriteLine(LanguageService.T("create.name"));
        string name = Console.ReadLine() ?? "";

        Console.WriteLine(LanguageService.T("create.source"));
        string sourceDirectory = VerifyPath(Console.ReadLine() ?? "");

        Console.WriteLine(LanguageService.T("create.target"));
        string targetDirectory = VerifyPath(Console.ReadLine() ?? "");

        Console.WriteLine(LanguageService.T("create.type"));
        ConsoleKeyInfo typechoice = Console.ReadKey();
        Console.WriteLine();
        bool type = true;
        switch (typechoice.KeyChar)
        {
            case 'O':
            case 'o':
                type = false;
                break;

            case 'I':
            case 'i':
                type = true;
                break;

            default:
                Console.WriteLine(LanguageService.T("create.type.invalid"));
                break;
        }

        try
        {
            BackupJob job = vm.CreateJob(name, sourceDirectory, targetDirectory, type);
            Console.WriteLine(string.Format(LanguageService.T("add.success"), job.Name));
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static string VerifyPath(string path)
    {

        string current = path ?? "";

        while (true)
        {
            if (!string.IsNullOrEmpty(current) && Path.IsPathFullyQualified(current) && Path.IsPathRooted(current))
            {

                return Path.GetFullPath(current);
            }

            Console.WriteLine(LanguageService.T("validation.path.invalid"));
            current = Console.ReadLine() ?? "";
        }
    }

    static void DisplayAllJobs(MainViewModel vm)
    {
        var jobs = GetAllJobs(vm);

        Console.WriteLine(LanguageService.T("display.listTitle"));
        for (int i = 0; i < jobs.Count; i++)
        {
            int realIndex = i + 1;
            Console.WriteLine(string.Format(LanguageService.T("display.item"), realIndex, jobs[i].Name, jobs[i].DateCreated));
        }
        Console.WriteLine();
    }

    static BackupJob? SearchJob(MainViewModel vm)
    {
        Console.WriteLine(LanguageService.T("search.prompt"));
        string name = Console.ReadLine() ?? " ";

        BackupJob? job = vm.SearchJob(name);
        if(job != null)
        {
            Console.WriteLine(string.Format(LanguageService.T("run.found"), job.Name));
            return job;
        }
        else
        {
            Console.WriteLine(string.Format(LanguageService.T("search.notfound"), name));
            return null;
        }
    }

    static void RunMultipleJob(MainViewModel vm)
    {
        var jobs = GetAllJobs(vm);

        DisplayAllJobs(vm);

        string input = InputMultipleJob();

        if(!JobSelection(input,out List<int> indices,out string error))
        {
            Console.WriteLine(error);
            return;
        }

        foreach(int index in indices)
        {
            BackupJob? job = vm.GetJobByIndex(index);
            if(job == null)
            {
                Console.WriteLine(string.Format(LanguageService.T("run.index.notfound"), index));
                continue;
            }

            Console.WriteLine(string.Format(LanguageService.T("run.running.named"), job.Name));
            RunSingleJob(vm,job);
            Console.WriteLine(string.Format(LanguageService.T("run.finished.named"), job.Name));
        }
    }

    static void RunSingleJob(MainViewModel vm, BackupJob job)
    {
        try
        {
            ActiveJob active = vm.CreateActiveJob(job);
            AttachHandlers(active);
            vm.RunJob(active);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void AttachHandlers(ActiveJob active)
    {
        active.PropertyChanged +=(_,e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(ActiveJob.Progression):
                Console.WriteLine(string.Format(LanguageService.T("active.progress"), active.Progression));
                break;
                case nameof(ActiveJob.NumberFilesRemaining):
                Console.WriteLine(string.Format(LanguageService.T("active.remaining.files"), active.NumberFilesRemaining));
                break;
                case nameof(ActiveJob.LastFileCopied):
                Console.WriteLine(string.Format(LanguageService.T("active.last.file"), active.LastFileCopied ?? string.Empty));
                break;
            }
        };
    }

    static List<BackupJob> GetAllJobs(MainViewModel vm)
    {
        var jobs = vm.GetAllJobs().ToList();
        if ( jobs.Count == 0)
        {
            Console.WriteLine(LanguageService.T("display.empty"));
            return new List<BackupJob>();
        }
        else
        {
            return jobs;
        }
    }

    static string InputMultipleJob()
    {
        Console.WriteLine(LanguageService.T("run.multiple.input"));
        return Console.ReadLine() ?? "";
    }

    static bool JobSelection(string input, out List<int> indices, out string error)
    {
        indices = new List<int>();
        error = "";

        if (string.IsNullOrWhiteSpace(input))
        {
            error = LanguageService.T("selection.empty");
            return false;
        }

        string trimmed = input.Trim();

        if (trimmed.Contains('-'))
        {
            string[] parts = trimmed.Split('-',StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if(parts.Length != 2)
            {
                error = LanguageService.T("selection.range.format");
                return false;
            }

            if(!(int.TryParse(parts[0],out int start) && int.TryParse(parts[1], out int end)))
            {
                error = LanguageService.T("selection.range.number");
                return false;
            }

            if(start <= 0 || end <= 0 || end < start)
            {
                error = LanguageService.T("selection.range.order");
                return false;
        
            }

            for (int i = start; i <= end ; i++)
            {
                indices.Add(i);
            }

            return true;
        }

        if (trimmed.Contains(";"))
        {
            string[] parts = trimmed.Split(';',StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            foreach(string p in parts)
            {
                if(!int.TryParse(p,out int val) || val <= 0)
                {
                    error = LanguageService.T("selection.list.number");
                    return false;
                }
                indices.Add(val);
            }

            indices.Sort();
            for(int i = indices.Count - 1; i > 0; i--)
            {
                if(indices[i] == indices[i - 1])
                {
                    indices.RemoveAt(i);
                }
            }
            return true;
        }

        if(!int.TryParse(trimmed, out int single) || single <= 0)
        {
            error = LanguageService.T("selection.single.number");
            return false;
        }

        indices.Add(single);
        return true;
    }

    static Settings GetConfiguration()
    {
        var settings = new Settings();
        try
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("./appsettings.json")
                .Build();

            config.Bind(settings);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine(LanguageService.T("error.configuration.notFound"), ex.Message);
        }
        return settings;
    }
}