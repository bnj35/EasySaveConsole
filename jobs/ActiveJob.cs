using System;
using System.Collections.Generic;
using System.Linq;
public class ActiveJob : BackUpJob
{

    //Active
    public float TotalFileSize {get; set;}

    public float NumberFiles {get; set;} // number of file in the Source Directory at the beginning

    public float Progression {get; set;} = 0; // percentage

    public int NumberFileRemaining {get; set;}

    public float SizeFileRemaining {get; set;}

    public List<string>? AdressesOfSaveFiles  {get; set;}
    public List<string>? DestinationOfSaveFiles  {get; set;}


    public ActiveJob(string name, string sourceDirectory, string targetDirectory) : base(name, sourceDirectory, targetDirectory)

    {

        Name = name;
        SourceDirectory = sourceDirectory;
        TargetDirectory = targetDirectory;
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

        File.Copy(SourceDirectory,TargetDirectory,Type);

        // if Source path exist && files in Source path > 0 then test if target exist if not create the folder(s) needed 
        // copy and update the size file remaining and number file remaining by displaying this and progression in %
        
    }
}