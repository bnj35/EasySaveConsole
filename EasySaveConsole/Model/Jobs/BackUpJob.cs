using System;

// BackUpJob is the persistent model (data) of a backup configuration
// ActiveJob inherits from it to reuse the source/target/name fields while running
public class BackUpJob : ObservableObject
{
    // Display name of the job
    public string Name { get; set; }

    // Source and target directories for the copy
    public string SourceDirectory { get; set; }
    public string TargetDirectory { get; set; }

    // Runtime flags / options (will be expanded later)
    public bool IsRunning { get; set; } = false;
    public bool Type { get; set; } = true; //true = differential backup / false = full backup

    // Dates used for history / UI
    public DateTime DateCreated {get; set;}

    public DateTime? DateStart { get; set; } // start of the job can be null

    public DateTime? DateEnd { get; set; } // end of the job can be null


    public BackUpJob(string name, string sourceDirectory, string targetDirectory)

    {
        // Minimal constructor: store the core job settings
        Name = name;
        SourceDirectory = sourceDirectory;
        TargetDirectory = targetDirectory;

    }

    public override string ToString()
    {
        // Useful for debugging / quick console display
        return $"Name: {Name}\n- Source Directory: {SourceDirectory} \n- Target Directory: {TargetDirectory} \n- Date Created: {DateCreated}";
    }

}
