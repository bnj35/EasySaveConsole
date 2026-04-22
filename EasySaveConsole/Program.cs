using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using static LanguageService;

// Console "View" (temporary): handles user input/output.
// The View delegates business actions to the ViewModel and subscribes to job events.
class Program
{
    static void Main(string[] args)
    {
        // Language selection happens in the View (console).
        // `LanguageService.Load` fills the translation dictionary used by `T(key)`
        Console.WriteLine("Choose a language / Choisis une langue: \n'en' for english\n'fr' pour français");
        string lang = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "en";
        if (lang != "fr" && lang != "en") lang = "en";
        LanguageService.Load(lang);

        // Model + ViewModel: the View owns them for the lifetime of the app
        var jobList = new JobList();
        var vm = new MainViewModel(jobList);

        // Main loop: keep showing the menu until user chooses exit
        bool exit = false;

        while (!exit)
        {
            // Render the menu, then read one key choice.
            DisplayMenu();

            ConsoleKeyInfo choice = Console.ReadKey();
            Console.Clear();

            // Dispatch user choice to an action.
            switch (choice.KeyChar)
            {
                case '0':
                    exit = true;
                    Console.WriteLine(T("job.exit"));
                    break;

                case '1':
                    Console.WriteLine(T("choice.1"));
                    if (vm.GetAllJobs().Count >= JobList.MaxJobs)
                    {
                        Console.WriteLine($"Maximum number of jobs reached ({JobList.MaxJobs}).");
                        break;
                    }
                    CreateJob(vm);
                    break;

                case '2':
                    DisplayAllJobs(vm);
                    break;

                case '3':
                    SearchJob(vm);
                    break;
                
                case '4':
                    Console.WriteLine(T("choice.5"));
                    RunMultipleJob(vm);
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
        // UI text is driven by translation keys.
        Console.WriteLine(T("menu.title"));
        Console.WriteLine(T("menu.prompt"));
        Console.WriteLine(T("menu.create"));
        Console.WriteLine(T("menu.display"));
        Console.WriteLine(T("menu.search"));
        // Option 5: run multiple jobs by index selection (e.g. "1-3" or "1;3")
        Console.WriteLine(T("menu.runMultiple"));
        Console.WriteLine(T("menu.exit"));
        Console.WriteLine(T("menu.separator"));
        Console.Write(T("menu.choice"));
    }

    static void CreateJob(MainViewModel vm)
    {
        if (vm.GetAllJobs().Count >= JobList.MaxJobs)
        {
            Console.WriteLine($"Maximum number of jobs reached ({JobList.MaxJobs}).");
            return;
        }

        // Collect job properties from the user, validate paths, then create via the ViewModel.
        Console.WriteLine(T("create.name"));
        string name = Console.ReadLine() ?? "";

        Console.WriteLine(T("create.source"));
        string sourceDirectory = VerifyPath(Console.ReadLine() ?? "");

        Console.WriteLine(T("create.target"));
        string targetDirectory = VerifyPath(Console.ReadLine() ?? "");

        // Job creation is in the ViewModel (so the View stays thin).
        try
        {
            BackUpJob job = vm.CreateJob(name, sourceDirectory, targetDirectory);
            Console.WriteLine(string.Format(T("add.success"), job.Name));
            Console.WriteLine(string.Format(T("create.success"), job.Name));
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void DisplayAllJobs(MainViewModel vm)
    {
        // Read jobs from the ViewModel and display them (always with 1-based indices).
        var jobs = vm.GetAllJobs();
        DisplayAllJobs(jobs);
    }

    static void DisplayAllJobs(System.Collections.Generic.IReadOnlyList<BackUpJob> jobs)
    {
        // Display all jobs with their 1-based index so the user can refer to them.
        if (jobs.Count == 0)
        {
            Console.WriteLine(T("display.empty"));
            return;
        }

        Console.WriteLine(T("display.listTitle"));
        for (int i = 0; i < jobs.Count; i++)
        {
            int index1Based = i + 1;
            Console.WriteLine($"{index1Based}. {jobs[i].Name} - Created on {jobs[i].DateCreated}");
        }
        Console.WriteLine();
    }

    static BackUpJob? SearchJob(MainViewModel vm)
    {
        // Search by name (case-insensitive), returns null if not found.
        Console.Write(T("search.prompt"));
        string name = Console.ReadLine() ?? "";

        BackUpJob? job = vm.SearchJob(name);
        if (job != null)
        {
            Console.WriteLine(string.Format(T("run.found"), job.Name));
            return job;
        }

        Console.WriteLine(string.Format(T("search.notfound"), name));
        return null;
    }

    static void RunMultipleJob(MainViewModel vm)
    {
        // Multi-run entry point.
        // This method is intentionally small: it shows the jobs, asks the user for a selection,
        // then runs each selected job sequentially
        //
        // Supported inputs:
        // - Range: "1-3" runs jobs 1 through 3
        // - List:  "1;3" runs jobs 1 and 3
        // - Single index: "2" runs job 2
        var jobs = vm.GetAllJobs();
        if (jobs.Count == 0)
        {
            Console.WriteLine(T("display.empty"));
            return;
        }

        // Reuse the standard job listing output.
        DisplayAllJobs(jobs);

        string input = PromptMultipleJobSelection();
        if (!TryParseJobSelection(input, out List<int> indices, out string error))
        {
            Console.WriteLine(error);
            return;
        }

        foreach (int index1Based in indices)
        {
            // Indices are 1-based to match what is displayed to the user
            BackUpJob? job = vm.GetJobByIndex(index1Based);
            if (job is null)
            {
                Console.WriteLine($"Job #{index1Based} not found.");
                continue;
            }

            Console.WriteLine(string.Format(T("run.found"), job.Name));
            RunSingleJob(vm, job);
            Console.WriteLine();
        }
    }

    static void RunSingleJob(MainViewModel vm, BackUpJob job)
    {
        // Shared execution path for running exactly one job
        // Used by multi-run after user selection
        try
        {
            ActiveJob active = vm.CreateActiveJob(job);
            AttachConsoleHandlers(active);
            active.runJob();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void AttachConsoleHandlers(ActiveJob active)
    {
        // Subscribe to property changes so the console prints progress updates
        active.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(ActiveJob.Progression):
                    Console.WriteLine($"Progress: {active.Progression:0.0}%");
                    break;

                case nameof(ActiveJob.NumberFileRemaining):
                    Console.WriteLine($"Remaining: {active.NumberFileRemaining} files, {active.SizeFileRemaining / (1024.0 * 1024.0):0.00} MB");
                    break;

                case nameof(ActiveJob.LastCopiedFileName):
                    Console.WriteLine($"Copied: {active.LastCopiedFileName} ({active.LastCopiedBytes / (1024.0 * 1024.0):0.00} MB)");
                    break;
            }
        };
    }

    static string PromptMultipleJobSelection()
    {
        // Ask the user which jobs to run.
        // Examples: "1-3" (range), "1;3" (list), "2" (single)
        Console.WriteLine("Enter the jobs to run (example: 1-3 or 1;3):");
        return Console.ReadLine() ?? "";
    }

    static bool TryParseJobSelection(string input, out List<int> indices, out string error)
    {
        // Parse the user selection into a list of 1-based indices
        //
        // Accepted formats:
        // - "1-3"  => [1, 2, 3]
        // - "1;3"  => [1, 3]
        // - "2"    => [2]
        //
        // Notes:
        // - list format is normalized (sorted + duplicates removed)
        // - range format keeps the natural ascending order
        indices = new List<int>();
        error = "";

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "No selection provided.";
            return false;
        }

        string trimmed = input.Trim();

        // Range format: "1-3"
        if (trimmed.Contains('-'))
        {
            string[] parts = trimmed.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                error = "Invalid range format. Use: 1-3";
                return false;
            }

            if (!int.TryParse(parts[0], out int start) || !int.TryParse(parts[1], out int end))
            {
                error = "Invalid range numbers. Use: 1-3";
                return false;
            }

            if (start <= 0 || end <= 0 || end < start)
            {
                error = "Invalid range. Indices must be positive and end must be >= start.";
                return false;
            }

            for (int i = start; i <= end; i++)
            {
                indices.Add(i);
            }

            return true;
        }

        // List format: "1;3"
        if (trimmed.Contains(';'))
        {
            string[] parts = trimmed.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            foreach (string p in parts)
            {
                if (!int.TryParse(p, out int val) || val <= 0)
                {
                    error = "Invalid list format. Use: 1;3";
                    return false;
                }
                indices.Add(val);
            }

            // Remove duplicates and run in ascending order (predictable execution)
            indices.Sort();
            for (int i = indices.Count - 1; i > 0; i--)
            {
                if (indices[i] == indices[i - 1]) indices.RemoveAt(i);
            }

            return true;
        }

        // Single index format: "2"
        if (!int.TryParse(trimmed, out int single) || single <= 0)
        {
            error = "Invalid selection. Use: 1-3 or 1;3";
            return false;
        }

        indices.Add(single);
        return true;
    }

    static string VerifyPath(string path)
    {
        // Loop until the user provides a fully-qualified rooted path
        string current = path ?? "";

        while (true)
        {
            if (!string.IsNullOrEmpty(current) && Path.IsPathFullyQualified(current) && Path.IsPathRooted(current))
            {
                // Normalize: ensures a consistent absolute path string
                return Path.GetFullPath(current);
            }

            // Ask again if the input path is invalid
            Console.WriteLine("The path isn't valid, try again :");
            current = Console.ReadLine() ?? "";
        }
    }
}