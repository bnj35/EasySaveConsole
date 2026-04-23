using System.Runtime.CompilerServices;
using EasyLog;

public class ActiveJob : BackupJob
{
    private float _totalFileSize;

    public float TotalFileSize
    {
        
        get => _totalFileSize;
        private set => SetProperty(ref _totalFileSize, value);
    }

    private int _numberFiles;

    public int NumberFiles
    {
        get => _numberFiles;
        private set => SetProperty(ref _numberFiles, value);
    }

    private int _numberFilesRemaining;

    public int NumberFilesRemaining
    {
        get => _numberFilesRemaining;
        set => SetProperty(ref _numberFilesRemaining, value);
    }

    private double _progression;

    public double Progression
    {
        get => _progression;
        private set => SetProperty(ref _progression, value);
    }

    private float _sizeFileRemaining;

    public float SizeFileRemaining
    {
        get => _sizeFileRemaining;
        private set => SetProperty(ref _sizeFileRemaining, value);
    }

    private string? _lastFileCopied;

    public string? LastFileCopied
    {
        get => _lastFileCopied;
        private set => SetProperty(ref _lastFileCopied, value);
    }

    private int _lastCopiedBytes;

    public int LastCopiedBytes
    {
        get => _lastCopiedBytes;
        private set => SetProperty(ref _lastCopiedBytes, value);
    }

    private double _lastTransferMs;

    public double LastTransferMs
    {
        get => _lastTransferMs;
        private set => SetProperty(ref _lastTransferMs, value);
    }

    public List<string>? AddressesOfFiles{get; set; }

    public List<string>? DestinationOfFiles {get; set; }

    private readonly EasyLogger Logger;

    public ActiveJob(string name, string source_dir, string target_dir, bool type, DateTime date) : base(name, source_dir, target_dir, type, date)
    {
        TotalFileSize = 0;
        NumberFiles = 0;
        AddressesOfFiles = [];
        DestinationOfFiles = [];

        SizeFileRemaining = TotalFileSize;
        Progression = 0.0;
        
        Logger = EasyLogger.GetInstance();
    }

    public void RunJob()
    {
        Console.WriteLine(string.Format(LanguageService.T("run.running.named"), Name));

        PathGuard.IsLooping(SourceDir, TargetDir);

        CopyPlan plan = CopyPlanner.Build(SourceDir, TargetDir);

        TotalFileSize = plan.TotalBytes;
        NumberFiles = plan.TotalFiles;
        NumberFilesRemaining = NumberFiles;
        SizeFileRemaining = TotalFileSize;

    Console.WriteLine(string.Format(LanguageService.T("active.total.size"), TotalFileSize));
    Console.WriteLine(string.Format(LanguageService.T("active.total.files"), NumberFiles));
    Console.WriteLine();

    CopyEngine engine = new CopyEngine(Logger);

    engine.Execute(
        plan,
        Name,
        Type,
        OnProgressPercent : percent =>
        {
            Progression = percent;
        },
        OnRemainingChanged : (filesRemaining, bytesRemaining) =>
        {
            NumberFilesRemaining = filesRemaining;
            SizeFileRemaining = (float)bytesRemaining;
        },
        OnFileCopied: (file, destinationPath, transferMs) =>
        {
            AddressesOfFiles?.Add(file.SourceFullPath);
            DestinationOfFiles?.Add(destinationPath);

            LastFileCopied = Path.GetFileName(destinationPath);
            LastCopiedBytes = (int)file.LengthBytes;
            LastTransferMs = transferMs;
        }
    );

    Console.WriteLine();
    float totalCopied = plan.TotalBytes - SizeFileRemaining;
    Console.WriteLine(string.Format(LanguageService.T("active.total.copied"), totalCopied));

    }

}