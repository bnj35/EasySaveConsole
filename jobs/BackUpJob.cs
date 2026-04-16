using System;

public class BackUpJob
{

    public string Name { get; set; }

    public string SourceDirectory { get; set; }
    public string TargetDirectory { get; set; }

    public bool IsRunning { get; set; } = false;
    public bool Type { get; set; } = true; //true = differential backup / false = full backup

    public DateTime DateCreated {get; set;}

    public DateTime? DateStart { get; set; } // start of the job can be null 

    public DateTime? DateEnd { get; set; } // end of the job can be null 


    public BackUpJob(string name, string sourceDirectory, string targetDirectory)

    {

        Name = name;
        SourceDirectory = sourceDirectory;
        TargetDirectory = targetDirectory;

    }

    // methods 
}