using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;

namespace EasySaveConsole;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var desktop = ApplicationLifetime;
        if (desktop != null)
        {
            // Initialize language, settings, and view model
            LanguageService.Instance.Load("en");
            
            Settings settings = GetConfiguration();
            string logFileFormat = settings.DefaultFileFormat;
            
            Joblist joblist = new Joblist();
            MainViewModel viewModel = new MainViewModel(joblist, settings, logFileFormat);
            
            // Create and show main window
            var mainWindow = new MainWindow
            {
                DataContext = viewModel
            };
            
            // Set main window based on application lifetime type
            dynamic deskApp = desktop;
            try
            {
                deskApp.MainWindow = mainWindow;
            }
            catch { }
        }

        base.OnFrameworkInitializationCompleted();
    }

    static Settings GetConfiguration()
    {
        var settings = new Settings();
        try
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("./appsettings.json")
                .Build();
            config.Bind(settings);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine(LanguageService.T("error.configuration.notFound"), ex.Message);
        }
        return settings;
    }
}