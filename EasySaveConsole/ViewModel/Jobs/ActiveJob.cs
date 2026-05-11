using EasyLog;

namespace EasySaveConsole;

public class ActiveJob : BackupJob
{
    private float _totalFileSize;
    private bool _isPlanning;
    private string _phaseMessage = string.Empty;
    private int _planningItemsScanned;

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

    private double _lastEncryptionMs;

    public double LastEncryptionMs
    {
        get => _lastEncryptionMs;
        private set => SetProperty(ref _lastEncryptionMs, value);
    }

// to have a better display of the progression of the job, we need to know if it's in the planning phase or in the copying phase, and to have some details about the planning phase (like how many items have been scanned)
    public bool IsPlanning
    {
        get => _isPlanning;
        private set => SetProperty(ref _isPlanning, value);
    }

    public string PhaseMessage
    {
        get => _phaseMessage;
        private set => SetProperty(ref _phaseMessage, value);
    }

    public int PlanningItemsScanned
    {
        get => _planningItemsScanned;
        private set => SetProperty(ref _planningItemsScanned, value);
    }

    public List<string>? AddressesOfFiles { get; set; }

    public List<string>? DestinationOfFiles { get; set; }

    public event Action<string, string>? FileCopied;


    public ActiveJob(string name, string source_dir, string target_dir, bool type, DateTime date) : base(name, source_dir, target_dir, type, date)
    {
        TotalFileSize = 0;
        NumberFiles = 0;
        AddressesOfFiles = [];
        DestinationOfFiles = [];

        SizeFileRemaining = TotalFileSize;
        Progression = 0.0;
        IsPlanning = false;
        PhaseMessage = string.Empty;
        PlanningItemsScanned = 0;
    }

    public void RunJob(CopyEngine engine)
    {
        PathGuard.IsLooping(SourceDir, TargetDir);
        IsPlanning = true;
        PhaseMessage = LanguageService.T("run.phase.planning");
        PlanningItemsScanned = 0;

        Console.WriteLine("aj1");

        CopyPlan plan = CopyPlanner.Build(
            SourceDir,
            TargetDir,
            (itemsScanned, currentPath) =>
            {
                PlanningItemsScanned = itemsScanned;
            }
        );// error
        
        Console.WriteLine("aj2");
        IsPlanning = false;
        PhaseMessage = LanguageService.T("run.phase.copying");

        TotalFileSize = plan.TotalBytes;
        NumberFiles = plan.TotalFiles;
        NumberFilesRemaining = NumberFiles;
        SizeFileRemaining = TotalFileSize;

        engine.Execute(
            plan,
            Name,
            Type,
            OnProgressPercent: percent =>
            {
                Progression = percent;
            },
            OnRemainingChanged: (filesRemaining, bytesRemaining) =>
            {
                NumberFilesRemaining = filesRemaining;
                SizeFileRemaining = (float)bytesRemaining;
            },
            OnFileCopied: (file, destinationPath, transferMs, encryptMs) =>
            {
                AddressesOfFiles?.Add(file.SourceFullPath);
                DestinationOfFiles?.Add(destinationPath);

                LastFileCopied = Path.GetFileName(destinationPath);
                LastCopiedBytes = (int)file.LengthBytes;
                LastTransferMs = transferMs;
                LastEncryptionMs = encryptMs;

                FileCopied?.Invoke(file.SourceFullPath, destinationPath);
            }
        );
                Console.WriteLine("aj3");

        PhaseMessage = LanguageService.T("run.phase.completed");
        Console.WriteLine();
        float totalCopied = plan.TotalBytes - SizeFileRemaining;

    }

}