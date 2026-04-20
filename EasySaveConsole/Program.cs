using System;
using System.IO;
using static LanguageService;

// Amaury or Jeffrey check if my comments are good pls <= and remove this later

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
                    CreateJob(vm);
                    break;

                case '2':
                    DisplayAllJobs(vm);
                    break;

                case '3':
                    DisplayAllJobs(vm);
                    Console.WriteLine(T("choice.3"));
                    RunJob(vm);
                    break;

                case '4':
                    SearchJob(vm);
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
        Console.WriteLine(T("menu.run"));
        Console.WriteLine(T("menu.search"));
        Console.WriteLine(T("menu.exit"));
        Console.WriteLine(T("menu.separator"));
        Console.Write(T("menu.choice"));
    }

    static void CreateJob(MainViewModel vm)
    {
        // Collect job properties from the user, validate paths, then create via the ViewModel.
        Console.WriteLine(T("create.name"));
        string name = Console.ReadLine() ?? "";

        Console.WriteLine(T("create.source"));
        string sourceDirectory = VerifyPath(Console.ReadLine() ?? "");

        Console.WriteLine(T("create.target"));
        string targetDirectory = VerifyPath(Console.ReadLine() ?? "");

        // Job creation is in the ViewModel (so the View stays thin).
        BackUpJob job = vm.CreateJob(name, sourceDirectory, targetDirectory);
        Console.WriteLine(string.Format(T("add.success"), job.Name));
        Console.WriteLine(string.Format(T("create.success"), job.Name));
    }

    static void DisplayAllJobs(MainViewModel vm)
    {
        // Read jobs from the ViewModel and display them.
        var jobs = vm.GetAllJobs();
        if (jobs.Count == 0)
        {
            Console.WriteLine(T("display.empty"));
            return;
        }

        Console.WriteLine(T("display.listTitle"));
        foreach (var job in jobs)
        {
            Console.WriteLine($"{job.Name} - Created on {job.DateCreated}");
        }
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

    static void RunJob(MainViewModel vm)
    {
        // Select a job first; if not found, we can't run anything.
        BackUpJob? job = SearchJob(vm);
        if (job is null) return;

        // Keep the historical behavior: show the "found" message again before running.
        // Preserve previous behavior: runJob printed the "found" message again
        Console.WriteLine(string.Format(T("run.found"), job.Name));

        try
        {
            // Create an ActiveJob (runtime object) and subscribe to its events for UI updates.
            ActiveJob active = vm.CreateActiveJob(job);

            // Event: percentage progress changed.
            // `+=` means we add a handler to the event
            // `(_, e) =>` means when the event happens, run this code
            // `_` is the first parameter we do not use (the sender)
            // `e` is the event data (EventArgs)
            active.ProgressChanged += (_, e) =>
            {
                Console.WriteLine($"Progress: {e.ProgressPercent:0.0}%");// e will be bewteen 0 and 100 
            };

            // Event: remaining work changed (files and bytes).
            // Same syntax: we ignore the sender (`_`) and read values from `e`
            active.RemainingChanged += (_, e) =>
            {
                Console.WriteLine($"Remaining: {e.FilesRemaining} files, {e.BytesRemaining / (1024.0 * 1024.0):0.00} MB");
            };

            // Event: a file was copied (includes size and timing).
            // `{ ... }` is the body of the handler
            active.FileCopied += (_, e) =>
            {
                Console.WriteLine($"Copied: {Path.GetFileName(e.SourcePath)} ({e.BytesCopied / (1024.0 * 1024.0):0.00} MB)");
            };

            // Run the copy.
            active.runJob();
        }
        catch (Exception ex)
        {
            // Show the error (invalid paths or guard triggered)
            Console.WriteLine(ex.Message);
        }
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