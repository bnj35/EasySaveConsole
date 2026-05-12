using Avalonia.Controls;
using Avalonia.Interactivity;
using EasyLog;
using System.Text.Json;

namespace EasySaveConsole;

public partial class SettingsWindow : Window
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly Settings _settings;
    public string CurrentLanguage { get; private set; }

    // Required for Avalonia for runtime loader
    public SettingsWindow()
    {
        InitializeComponent();
        _settings = new Settings();
        CurrentLanguage = "en";
    }

    public SettingsWindow(Settings settings, string currentLanguage)
    {
        InitializeComponent();
        _settings = settings;
        CurrentLanguage = currentLanguage;

        switch (_settings.EasyLogSettings.LogStorage)
        {
            case LogStorage.Local:
                LogStorageCombo.SelectedIndex = 0;
                break;
            case LogStorage.Remote:
                LogStorageCombo.SelectedIndex = 1;
                break;
            case LogStorage.Both:
                LogStorageCombo.SelectedIndex = 2;
                break;
        };

        LanguageCombo.SelectedIndex = CurrentLanguage == "fr" ? 1 : 0;
        LogFormatCombo.SelectedIndex = _settings.DefaultFileFormat == "xml" ? 1 : 0;
        ExcludeProcessesInput.Text = _settings.ProcessExclusionSettings.ExcludedProcesses;
        EncryptExtensionsInput.Text = _settings.EncryptExtensions;
        PriorityExtensionsInput.Text = _settings.PriorityExtensions;
        BigFileSizeInput.Value = _settings.BigFileSize <= 0 ? 10 : _settings.BigFileSize;

        SaveButton.Click += SaveButton_Click;
        CancelButton.Click += (_, _) => Close(false);
    }

    private void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        string newLang = LanguageCombo.SelectedIndex == 1 ? "fr" : "en";
        if (newLang != CurrentLanguage)
        {
            LanguageService.Instance.Load(newLang);
            CurrentLanguage = newLang;
        }

        switch (LogStorageCombo.SelectedIndex)
        {
            case 0:
                _settings.EasyLogSettings.LogStorage = LogStorage.Local;
                break;
            case 1:
                _settings.EasyLogSettings.LogStorage = LogStorage.Remote;
                break;
            case 2:
                _settings.EasyLogSettings.LogStorage = LogStorage.Both;
                break;
        };

        _settings.DefaultFileFormat = LogFormatCombo.SelectedIndex == 1 ? "xml" : "json";
        _settings.ProcessExclusionSettings.ExcludedProcesses = ExcludeProcessesInput.Text ?? "";
        _settings.EncryptExtensions = EncryptExtensionsInput.Text ?? "";
        _settings.PriorityExtensions = PriorityExtensionsInput.Text ?? "";
        _settings.BigFileSize = (int)(BigFileSizeInput.Value ?? 10);

        PersistSettings();
        Close(true);
    }

    private void PersistSettings()
    {
        var data = new
        {
            defaultFileFormat = _settings.DefaultFileFormat,
            dateFormat = _settings.DateFormat,
            statusFileSettings = new { filePath = _settings.StatusFileSettings.FilePath },
            easyLogSettings = new { directoryPath = _settings.EasyLogSettings.DirectoryPath, logStorage = _settings.EasyLogSettings.LogStorage },
            processExclusionSettings = new { excludedProcesses = _settings.ProcessExclusionSettings.ExcludedProcesses },
            encryptExtensions = _settings.EncryptExtensions,
            priorityExtensions = _settings.PriorityExtensions,
            bigFileSize = _settings.BigFileSize
        };
        File.WriteAllText("./appsettings.json", JsonSerializer.Serialize(data, _jsonOptions));
    }
}
