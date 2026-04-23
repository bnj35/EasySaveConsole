public class BackupJob : ObservableObject
{
    public string Name {get; set;}

    public string SourceDir {get; set;}

    public string TargetDir {get; set;}

    public bool IsRunning {get; set; } = true;

    public bool Type {get; set;} = true;

    public DateTime DateCreated {get; set;}
    public DateTime? DateStart {get; set;}
    public DateTime? DateEnd {get; set;}

    public BackupJob (string name, string source_dir, string target_dir, bool type,DateTime date_created)
    {
        Name = name;
        SourceDir = source_dir;
        TargetDir = target_dir;
        Type = type;
        DateCreated = date_created;
    }
}