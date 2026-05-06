# EasySave - Technical Note

## Version
V2.0.0


## Overview
EasySave is a C# .NET 10 application for managing and executing directory backup jobs. It is engineered with a strict **MVVM (Model-View-ViewModel)** architectural pattern to ensure a clean separation of concerns, high performance for file operations, and a flexible, multilingual user interface built with **Avalonia**.


## Architecture (MVVM)
The codebase is strictly organized to separate concerns:

1. **View (`MainWindow.axaml` / `SettingsWindow.axaml`)**:
   - **Responsibility**: User interaction only. Renders the job list, progress bars, and settings form.
   - **Implementation**: Avalonia XAML views bound to `LanguageService.Instance` for live translation. Code-behind handles button events and forwards actions to the ViewModel. Subscribes to `PropertyChanged` events from `ActiveJob` to display real-time progress without containing any business logic.

2. **ViewModel (`MainViewModel.cs`)**:
   - **Responsibility**: Acts as the bridge between the View and the Model. It prepares data for display and orchestrates business operations.
   - **Implementation**: Exposes clean, high-level methods like `CreateJob()`, `CreateActiveJob()`, and `RunJob()` that the View can call. It does not expose the Model's internal data structures directly.

3. **Model**:
   - **Responsibility**: Represents the application's data and business logic. It is completely UI-agnostic.
   - **Components**:
     - `JobList`: Manages the collection of `BackupJob` instances.
     - `BackupJob`: A data object representing the static configuration of a backup (Name, Source, Target, Type, Encrypt flag).
     - `ActiveJob`: Represents a running backup execution. Extends `BackupJob` with live progress properties (`Progression`, `NumberFilesRemaining`, etc.) and raises `PropertyChanged` events consumed by the View.
     - `CopyEngine`: Executes the copy plan, enforces process exclusion, and delegates encryption to `CryptoSoftRunner`.
     - `CopyPlanner`: Scans the source directory and builds a `CopyPlan` (file list, total size) before execution starts.

**Strict Separation Rules:**
- The **View** contains NO business logic. It only knows how to display data and forward user actions to the ViewModel.
- The **Model** contains NO UI code. It has no knowledge of Avalonia or any other presentation framework.
- The **ViewModel** contains NO direct references to UI elements.


## Key Components
- **CopyEngine**: Designed for high performance and low memory footprint. Uses `FileStream` with `FileOptions.SequentialScan` for large file transfers. Implements an event-driven approach (via `Action` delegates) to report progress, remaining files, and bytes in real-time. Checks for excluded business processes before copying each file, and pauses execution if one is detected.
- **CryptoSoftRunner**: Runs the external CryptoSoft executable for the file encryption. Triggered by file when the job's `Encrypt` flag is set and the file extension matches the configured list.
- **LanguageService**: A dynamic, dictionary-based translation system. Loads flattened JSON files (`LanguageEN.json`, `LanguageFR.json`). Emit an event on language switch, which causes all Avalonia bindings bound to `LanguageService.Instance` to update without any manual refresh.

## Logging & State Management
The application features two distinct logging mechanisms:

1. **Execution Logging (EasyLog)**: Records every file transfer with source path, destination path, file size, and transfer duration. A new file is created each day. Supports both **JSON** and **XML** formats.

2. **State Logging**: A single file (JSON or XML) tracks the current state of each job (progress, files remaining, status). Updated in real-time during execution.


## Security & Business Software Blocking
- **Encryption**: Jobs can be configured to encrypt files on copy using CryptoSoft. The encrypted extensions are configurable in Settings (e.g., `.txt;.pdf`).
- **Process Exclusion**: A list of business software process names can be configured in Settings. Before copying each file, `CopyEngine` checks if any of these processes are running. If one is detected, the copy is blocked until the process exits.


## Coding Conventions
### Naming (C# Standard)
- Public classes, methods, and properties follow `PascalCase` (e.g., `BackUpJob`, `ExecuteJob()`).
- Local variables and parameters follow `camelCase` (e.g., `jobName`, `sourcePath`).
- Private fields use `_camelCase` with an underscore (e.g., `_translations`, `_currentJob`).

### Language Management
All strings displayed to the user MUST be routed through `LanguageService`:

```csharp
// Forbidden
Console.WriteLine("Welcome");

// Mandatory
LanguageService.T("menu.welcome")
```
Any new string must be added to both translation files using a flat JSON syntax:
```json
{
  "menu.welcome": "Welcome",
  "create.error": "Error during creation"
}
```


## Acceptance Tests
- **Test 1:** Create a job with a valid path.
    Expected result: Job appears in the list (Status: OK).
- **Test 2:** Create a job with an invalid or missing path.
    Expected result: Error message displayed in the status bar (Status: OK).
- **Test 3:** Display job list when no jobs are created.
    Expected result: Empty list displayed (Status: OK).
- **Test 4:** Change language in Settings.
    Expected result: All UI text updates immediately without restarting (Status: OK).
- **Test 5:** Run a job with encryption enabled.
    Expected result: Files at destination are encrypted by CryptoSoft (Status: OK).
- **Test 6:** Run a job while an excluded business process is running.
    Expected result: Execution is blocked until the process is closed (Status: OK).


## Prerequisites
- .NET 10.0 SDK
- Git


## Installation & Run
```bash
git clone <your-repo-url>
cd EasySaveConsole
dotnet build
dotnet run
```


# EasySave - User Manual

## Introduction
Welcome to EasySave! This application allows you to configure, manage, and execute backup jobs for your directories through a graphical interface.

## Getting Started
1. Launch the application (`dotnet run` or the compiled executable).
2. The main window opens directly. No startup prompts — language and settings are managed through the **Settings** button.

## Main Window

### Creating a Backup Job
Fill in the form on the left panel:
- **Name**: A unique name to identify the job (e.g., "My Documents Backup").
- **Source Directory**: The full path of the folder to back up.
- **Target Directory**: The full path where the backup will be stored.
- **Type**: Choose between **Full** (copies all files) or **Incremental** (copies only new/modified files).
- **Encrypt**: Check this box to encrypt files on copy using CryptoSoft.

Click **Create Job** to add it to the list.

### Running Jobs
Select one or more jobs in the list, then click **Run Selected**. Active jobs appear in the progress panel at the bottom with a live progress bar and remaining file count.

### Deleting a Job
Select a job in the list and click **Delete Job**.

### Settings
Click the **Settings** button (top right) to configure:
- **Log format**: JSON or XML for execution logs.
- **Log directory**: Where daily log files are stored.
- **Status file path**: Location of the real-time state file.
- **Encrypted extensions**: Semicolon-separated list of extensions to encrypt (e.g., `.txt;.pdf`).
- **Excluded processes**: Semicolon-separated list of business software process names that block job execution when running (e.g., `notepad;calc`).
- **Language**: Switch between English and French. The interface updates immediately.

## Step-by-Step Example
1. Launch the application.
2. Fill in the job form: name `MyWorkBackup`, source `C:\Work`, target `D:\Backup`, type Full.
3. Click **Create Job**. The job appears in the list.
4. Select it and click **Run Selected**.
5. Watch the progress bar until completion.


## Authors
AUGER Benjamin
DOUBLET Amaury
ROURE Antoine
NGOUONPE-FEZEU-TAMEU Jeffrey
