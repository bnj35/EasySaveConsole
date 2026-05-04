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

        // Populate initial job list
        RefreshJobList();

        // Wire button events
        if (this.FindControl<Button>("CreateJobButton") is Button createBtn)
            createBtn.Click += CreateJobButton_Click;

        if (this.FindControl<Button>("RunSelectedButton") is Button runBtn)
            runBtn.Click += RunSelectedButton_Click;

        if (this.FindControl<Button>("DeleteJobButton") is Button delBtn)
            delBtn.Click += DeleteJobButton_Click;
    }

    private void CreateJobButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
            return;

        var nameInput = this.FindControl<TextBox>("JobNameInput");
        var sourceInput = this.FindControl<TextBox>("SourceDirInput");
        var targetInput = this.FindControl<TextBox>("TargetDirInput");
        var typeCombo = this.FindControl<ComboBox>("BackupTypeCombo");
        var encryptCheck = this.FindControl<CheckBox>("EncryptCheck");

        string name = nameInput?.Text ?? "";
        string source = sourceInput?.Text ?? "";
        string target = targetInput?.Text ?? "";
        bool isFullBackup = typeCombo?.SelectedIndex != 1;
        bool encrypt = encryptCheck?.IsChecked ?? false;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
        {
            UpdateStatus("Please fill all fields");
            return;
        }

        try
        {
            _viewModel.CreateJob(name, source, target, isFullBackup, encrypt);
            UpdateStatus($"Job '{name}' created successfully");
            RefreshJobList();

            // Clear inputs
            if (nameInput != null) nameInput.Text = "";
            if (sourceInput != null) sourceInput.Text = "";
            if (targetInput != null) targetInput.Text = "";
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error: {ex.Message}");
        }
    }

    private void RunSelectedButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
            return;

        var jobsList = this.FindControl<ListBox>("JobsList");
        var activeJobsList = this.FindControl<ListBox>("ActiveJobsList");
        var selectedJobs = jobsList?.SelectedItems?.OfType<BackupJob>().ToList() ?? [];

        if (selectedJobs.Count == 0)
        {
            UpdateStatus("Please select one or more jobs to run");
            return;
        }

        var activeJobs = new ObservableCollection<ActiveJob>();

        if (activeJobsList != null)
            activeJobsList.ItemsSource = activeJobs;

        UpdateStatus(selectedJobs.Count == 1
            ? $"Running job '{selectedJobs[0].Name}'..."
            : $"Running {selectedJobs.Count} jobs...");

        Task.Run(() =>
        {
            foreach (var job in selectedJobs)
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
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (activeJob != null)
                            activeJobs.Remove(activeJob);

                        UpdateStatus($"Error running job '{job.Name}': {ex.Message}");
                    });
                }
            }

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                UpdateStatus("Selected jobs completed");
                RefreshJobList();
            });
        });
    }

    private void DeleteJobButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
            return;
            
        var jobsList = this.FindControl<ListBox>("JobsList");
        if (jobsList?.SelectedItem is BackupJob job)
        {
            int index = _viewModel.GetAllJobs().ToList().IndexOf(job);
            _viewModel.DeleteJob(index + 1);
            UpdateStatus($"Job '{job.Name}' deleted");
            RefreshJobList();
        }
        else
        {
            UpdateStatus("Please select a job to delete");
        }
    }

    private void RefreshJobList()
    {
        if (_viewModel == null)
            return;

        var jobsList = this.FindControl<ListBox>("JobsList");
        if (jobsList != null)
        {
            var jobs = _viewModel.GetAllJobs().ToList();
            jobsList.ItemsSource = new ObservableCollection<BackupJob>(jobs);
        }
    }

    private void UpdateStatus(string message)
    {
        var statusLabel = this.FindControl<TextBlock>("StatusMessage");
        if (statusLabel != null)
            statusLabel.Text = message;
    }
}