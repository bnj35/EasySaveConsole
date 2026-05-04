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

        string name = nameInput?.Text ?? "";
        string source = sourceInput?.Text ?? "";
        string target = targetInput?.Text ?? "";
        bool isFullBackup = typeCombo?.SelectedIndex != 1;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
        {
            UpdateStatus("Please fill all fields");
            return;
        }

        try
        {
            _viewModel.CreateJob(name, source, target, isFullBackup);
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
        if (jobsList?.SelectedItem is not BackupJob job)
        {
            UpdateStatus("Please select a job to run");
            return;
        }

        try
        {
            UpdateStatus($"Running job '{job.Name}'...");

            // Run async to avoid freezing UI
            Task.Run(() =>
            {
                try
                {
                    ActiveJob activeJob = _viewModel.CreateActiveJob(job);
                    _viewModel.RunJob(activeJob);

                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        UpdateStatus($"Job '{job.Name}' completed");
                        RefreshJobList();
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                        UpdateStatus($"Error running job: {ex.Message}")
                    );
                }
            });
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error: {ex.Message}");
        }
    }

    private void DeleteJobButton_Click(object? sender, RoutedEventArgs e)
    {
        var jobsList = this.FindControl<ListBox>("JobsList");
        if (jobsList?.SelectedItem is BackupJob job)
        {
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