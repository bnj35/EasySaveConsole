public class ActiveJob : BackUpJob
{

    //Active
    public float TotalFileSize {get; set;}

    public float Progression {get; set;}

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
}