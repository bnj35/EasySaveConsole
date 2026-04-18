using EasyLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
public class ActiveJob : BackUpJob
{

    //Active
    public float TotalFileSize { get; set; }

    public float NumberFiles { get; set; } // number of file in the Source Directory at the beginning

    public double Progression { get; set; } = 0.0; // percentage

    public int NumberFileRemaining { get; set; }

    public float SizeFileRemaining { get; set; }

    public List<string>? AdressesOfSaveFiles { get; set; }
    public List<string>? DestinationOfSaveFiles { get; set; }

    private readonly EasyLogger Logger;

    public ActiveJob(string name, string sourceDirectory, string targetDirectory) : base(name, sourceDirectory, targetDirectory)

    {

        Name = name;
        SourceDirectory = sourceDirectory;
        TargetDirectory = targetDirectory;

        TotalFileSize = 0;
        NumberFiles = 0;
        AdressesOfSaveFiles = [];
        DestinationOfSaveFiles = [];

        SizeFileRemaining = TotalFileSize;
        Progression = 0.0;
        Logger = EasyLogger.GetInstance();
    }

    // methods 

    //method1 set the address of save files by concatening a directory and all the files in it 
    // (input : string directory / output List<string>)

    //method2 get the number of files in the directory ( input : string directory )


    //method3 get the total size of files in the directory  ( input : string directory )

    //

    public void runJob()
    {
        Console.WriteLine("File copy will start...");
        // Calculate total size and count files
        CalculateDirectoryStats(SourceDirectory);
        NumberFileRemaining = (int)NumberFiles;
        
        Console.WriteLine($"Total size before transfer: {TotalFileSize / (1024 * 1024):F2} MB");
        Console.WriteLine($"Number of files: {NumberFiles}");
        Console.WriteLine();
        
        CopyDirectory(SourceDirectory, TargetDirectory);
        
        Console.WriteLine();
        long totalCopied = (long)TotalFileSize - (long)SizeFileRemaining;
        Console.WriteLine($"Total size after transfer: {totalCopied / (1024 * 1024):F2} MB");
    }

    private void CalculateDirectoryStats(string sourceDir)
    {
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            TotalFileSize += new FileInfo(file).Length;
            NumberFiles++;
        }

        foreach (string folder in Directory.GetDirectories(sourceDir))
        {
            CalculateDirectoryStats(folder);
        }
    }

//Copy job with buffer better for large sizes files
    public void CopyJob(string sourceFile, string destFile)
    {
        
        long totalBytes = new FileInfo(sourceFile).Length;
        long totalRead = 0;
        byte[] buffer = new byte[81920];

        // start a timer for the copying duration
        Stopwatch stopwatch = Stopwatch.StartNew();

        using var source = File.OpenRead(sourceFile);
        using var destination = File.Create(destFile);

        int read;
        while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
        {
            destination.Write(buffer, 0, read);
            totalRead += read;

            Progression = (double)(TotalFileSize - SizeFileRemaining + totalRead) / TotalFileSize * 100;
            Console.WriteLine($"Progress: {Progression:0.0}%");
        }
        // Stop the timer and get the duration in milliseconds
        stopwatch.Stop();
        double transferTime = stopwatch.Elapsed.TotalMilliseconds;

        AdressesOfSaveFiles?.Add(sourceFile);
        DestinationOfSaveFiles?.Add(destFile);
        SizeFileRemaining -= totalBytes;
        NumberFileRemaining--;
        Logger.LogFileCopy(Name, sourceFile, destFile, totalBytes, transferTime);
    }

// copy a whole directory
    public void CopyDirectory(string sourceDir, string targetDir)
    {
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
            Logger.LogDirectoryCreation(Name, targetDir);
        }

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(targetDir, Path.GetFileName(file));
            CopyJob(file, destFile);
        }

        foreach (string folder in Directory.GetDirectories(sourceDir))
        {
            string destFolder = Path.Combine(targetDir, Path.GetFileName(folder));
            CopyDirectory(folder, destFolder);
        }
    }
}