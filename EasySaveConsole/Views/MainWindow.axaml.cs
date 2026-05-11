using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Collections.ObjectModel;

namespace EasySaveConsole;

public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        _viewModel = DataContext as MainViewModel;
        if (_viewModel == null)
            return;

        SettingsButton.Click += SettingsButton_Click;
        CreateJobButton.Click += CreateJobButton_Click;
        RunSelectedButton.Click += RunSelectedButton_Click;
        DeleteJobButton.Click += DeleteJobButton_Click;

        RefreshJobList();
    }

    private void CreateJobButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
            return;

        string name = JobNameInput.Text ?? "";
        string source = SourceDirInput.Text ?? "";
        string target = TargetDirInput.Text ?? "";
        bool isFullBackup = BackupTypeCombo.SelectedIndex != 1;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
        {
            UpdateStatus(LanguageService.T("main.status.fill.fields"));
            return;
        }

        try
        {
            _viewModel.CreateJob(name, source, target, isFullBackup);
            UpdateStatus(string.Format(LanguageService.T("main.status.job.created"), name));
            RefreshJobList();

            JobNameInput.Text = "";
            SourceDirInput.Text = "";
            TargetDirInput.Text = "";
        }
        catch (Exception ex)
        {
            UpdateStatus(string.Format(LanguageService.T("main.status.error"), ex.Message));
        }
    }

    private void RunSelectedButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
            return;

        var selectedJobs = JobsList.SelectedItems?.OfType<BackupJob>().ToList() ?? [];


        if (selectedJobs.Count == 0)
        {
            UpdateStatus(LanguageService.T("main.status.select.run"));
            return;
        }

        var activeJobs = new ObservableCollection<ActiveJob>();
        ActiveJobsList.ItemsSource = activeJobs;

        UpdateStatus(selectedJobs.Count == 1
            ? string.Format(LanguageService.T("main.status.running.one"), selectedJobs[0].Name)
            : string.Format(LanguageService.T("main.status.running.multiple"), selectedJobs.Count));

    Task.Run(async () =>
    {
        var tasks = new List<Task>();
        bool allJobsSucceeded = true;
        //une task par job
        foreach (var job in selectedJobs)
        {
            var task = Task.Run(() =>
            {
                ActiveJob? activeJob = null;

                try
                {
                    activeJob = _viewModel.CreateActiveJob(job);

                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        activeJobs.Add(activeJob);
                    });

                    _viewModel.RunJob(activeJob);

                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        activeJobs.Remove(activeJob);
                    });
                }
                catch (Exception ex)
                {
                    allJobsSucceeded = false;

                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (activeJob != null)
                            activeJobs.Remove(activeJob);
                        UpdateStatus(string.Format(LanguageService.T("main.status.error.run"), job.Name, ex.Message));
                    });
                }
            });

            tasks.Add(task);
        }

        // attend que tout les jobs termine
        await Task.WhenAll(tasks);

            if (allJobsSucceeded)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UpdateStatus(LanguageService.T("main.status.completed"));
                    RefreshJobList();
                });
            }
            else
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    RefreshJobList();
                });
            }
        });
    }

    private void DeleteJobButton_Click(object? sender, RoutedEventArgs e)
    {
        if (JobsList.SelectedItem is BackupJob job)
        {
            int index = _viewModel!.GetAllJobs().ToList().IndexOf(job);
            _viewModel.DeleteJob(index + 1);
            UpdateStatus(string.Format(LanguageService.T("main.status.job.deleted"), job.Name));
            RefreshJobList();
        }
        else
        {
            UpdateStatus(LanguageService.T("main.status.select.delete"));
        }
    }

    private async void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
            return;

        var settingsWindow = new SettingsWindow(_viewModel.Settings, _viewModel.Language);
        var saved = await settingsWindow.ShowDialog<bool?>(this);

        if (saved == true)
        {
            _viewModel.Language = settingsWindow.CurrentLanguage;
            UpdateStatus(LanguageService.T("main.status.settings.saved"));
        }
    }

    private void RefreshJobList()
    {
        if (_viewModel == null)
            return;

        JobsList.ItemsSource = new ObservableCollection<BackupJob>(_viewModel.GetAllJobs());
    }

    private void UpdateStatus(string message) => StatusMessage.Text = message;
}