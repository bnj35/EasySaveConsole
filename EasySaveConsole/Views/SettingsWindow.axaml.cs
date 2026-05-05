using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Text.Json;

namespace EasySaveConsole;

public partial class SettingsWindow : Window
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly Settings _settings;
    public string CurrentLanguage { get; private set; }

    public SettingsWindow(Settings settings, string currentLanguage)
    {
        InitializeComponent();
        _settings = settings;
        CurrentLanguage = currentLanguage;

        LanguageCombo.SelectedIndex = CurrentLanguage == "fr" ? 1 : 0;
        LogFormatCombo.SelectedIndex = _settings.DefaultFileFormat == "xml" ? 1 : 0;
        LogDirInput.Text = _settings.EasyLogSettings.DirectoryPath;
        StatusPathInput.Text = _settings.StatusFileSettings.FilePath;

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

        _settings.DefaultFileFormat = LogFormatCombo.SelectedIndex == 1 ? "xml" : "json";
        _settings.EasyLogSettings.DirectoryPath = LogDirInput.Text ?? _settings.EasyLogSettings.DirectoryPath;
        _settings.StatusFileSettings.FilePath = StatusPathInput.Text ?? _settings.StatusFileSettings.FilePath;

        PersistSettings();
        Close(true);
    }

    private void PersistSettings()
    {
        try
        {
            var data = new
            {
                defaultFileFormat = _settings.DefaultFileFormat,
                dateFormat = _settings.DateFormat,
                statusFileSettings = new { filePath = _settings.StatusFileSettings.FilePath },
                easyLogSettings = new { directoryPath = _settings.EasyLogSettings.DirectoryPath }
            };
            File.WriteAllText("./appsettings.json", JsonSerializer.Serialize(data, _jsonOptions));
        }
        catch { }
    }
}
