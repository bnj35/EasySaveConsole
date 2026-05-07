using System;
using System.Linq;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace EasySaveConsole;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            Console.WriteLine("running");
            var settings = App.GetConfiguration();
            string format = settings.DefaultFileFormat;
            string statusPath = $"{settings.StatusFileSettings.FilePath}.{format}";

            Joblist joblist = Joblist.LoadFromStatusFile(statusPath);
            MainViewModel viewModel = new MainViewModel(joblist, settings);

            var indices = ParseIndices(args[0]);
            var tasks = new List<Task>();

            foreach (var idx in indices)
            {
                var job = viewModel.GetJobByIndex(idx);
                Console.WriteLine($"Running job at index {idx}: {(job != null ? job.Name : "Not found")}");
                if (job == null)
                {
                    Console.WriteLine($"No job at index {idx}");
                    continue;
                }

                var task = Task.Run(() =>
                    {
                        try
                        {
                            var active = viewModel.CreateActiveJob(job);
                            Console.WriteLine($"Starting job: {active.Name}");
                            viewModel.RunJob(active);//ça plante la dedans pour le moment
                            Console.WriteLine($"Finished job: {active.Name}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error running job {idx}: {ex.Message}");
                        }
                    });

                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("All jobs completed");
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
    static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();

    static List<int> ParseIndices(string input)
    {
        var indices = new List<int>();
        if (string.IsNullOrWhiteSpace(input)) return indices;

        var segments = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            var token = segment.Trim();
            if (token.Contains('-'))
            {
                var bounds = token.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (bounds.Length == 2
                    && int.TryParse(bounds[0], out int rangeStart)
                    && int.TryParse(bounds[1], out int rangeEnd))
                {
                    if (rangeStart <= rangeEnd)
                    {
                        for (int i = rangeStart; i <= rangeEnd; i++)
                            indices.Add(i);
                    }
                    else
                    {
                        for (int i = rangeStart; i >= rangeEnd; i--)
                            indices.Add(i);
                    }
                }
            }
            else if (int.TryParse(token, out int singleIndex))
            {
                indices.Add(singleIndex);
            }
        }

        return indices.Distinct().OrderBy(x => x).ToList();
    }
}