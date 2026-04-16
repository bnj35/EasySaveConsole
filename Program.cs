using System;
class Program
{
    static void Main(string[] args)
    {
        //list of backup jobs 
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
                    Console.WriteLine("Thank you for using EasySave");
                    break;

                case '1':

                    Console.WriteLine("Choice 1 selected");
                    CreateJob(joblist);

                    break;

                case '2':

                    joblist.DisplayAllJob();
                    break;

                case '3':

                    Console.WriteLine("Choice 3 selected");
                    break;

                case '4':
                    SearchJob(joblist);

                    break;

                default:

                    Console.WriteLine("Invalid choice, please try again.");

                    break;

            }

            Console.WriteLine("\nPress any key to continue...");

            Console.ReadKey();

            Console.Clear();
        }

    }


    static void DisplayMenu()
    {
        Console.WriteLine("====== Welcome in EasySave ========");

        Console.WriteLine("What do you want to do ?");

        Console.WriteLine("1. Create a backup job");

        Console.WriteLine("2. Display all");

        Console.WriteLine("3. Run a backup job");

        Console.WriteLine("4. Search for a backup job");

        Console.WriteLine("0. Press 0 to exit");

        Console.WriteLine("===================================");

        Console.Write("Your choice:");
    }

    static void CreateJob(JobList joblist)
    {
        Console.WriteLine("Enter the name");

        string Name = Console.ReadLine()!;

        Console.WriteLine("Now enter the source directory");

        string SourceDirectory = Console.ReadLine()!;

        Console.WriteLine($"Your source directory is {SourceDirectory}");

        Console.WriteLine("Then enter the target directory");

        string TargetDirectory = Console.ReadLine()!;

        BackUpJob newJob = new BackUpJob(Name, SourceDirectory, TargetDirectory)
        {
            DateCreated = DateTime.Now// set the date of creation
        };

        joblist.AddJob(newJob);

        Console.WriteLine($"New backup job created {newJob}");
    }

    static BackUpJob? SearchJob(JobList jobList)

    {

        Console.Write("Enter the name of the job to search for:");

        string name = Console.ReadLine()!;

        BackUpJob job = jobList.SearchJob(name);

        if (job != null)

        {

            Console.WriteLine(job.Name);
            return job;

        }

        else

        {

            Console.WriteLine($"The book '{name}' was not found.");
            return null;

        }

    }

    static void RunJob(JobList joblist)
    {
        BackUpJob? job = SearchJob(joblist);

        if (job != null)
        {
            Console.WriteLine($"job {job.Name} finded ");
        }
        // SearchJob
    }


}