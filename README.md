# EasySave - Technical Note

## Version
V1.1.0


## Overview
EasySave is a C# .NET 10 console application for managing and executing directory backup jobs. It is engineered with a strict **MVVM (Model-View-ViewModel)** architectural pattern to ensure a clean separation of concerns, high performance for file operations, and a flexible, multilingual user interface.


## Architecture (MVVM)
The codebase is strictly organized to separate concerns:

1. **View (`Program.cs`)**: 
   - **Responsibility**: User interaction only. Renders menus and progress updates in the console.
   - **Implementation**: Captures user input (`Console.ReadKey`, `Console.ReadLine`) and displays localized strings via the `LanguageService`. It subscribes to `PropertyChanged` events from the ViewModel's `ActiveJob` to display real-time progress without containing any business logic.

2. **ViewModel (`MainViewModel.cs`)**: 
   - **Responsibility**: Acts as the bridge between the View and the Model. It prepares data for display and orchestrates business operations.
   - **Implementation**: Instantiated by the View, it holds a reference to the `JobList` model. It exposes clean, high-level methods like `CreateJob()`, `SearchJob()`, and `CreateActiveJob()` that the View can call. It does not expose the Model's internal data structures directly.

3. **Model**: 
   - **Responsibility**: Represents the application's data and business logic. It is completely UI-agnostic.
   - **Components**:
     - `JobList`: Manages the collection of `BackUpJob` instances, enforcing business rules like the maximum job limit.
     - `BackUpJob`: A data object representing the static configuration of a backup (Name, Source, Target, etc.).
     - `ActiveJob`: Represents a running backup execution. It encapsulates the `CopyEngine`, tracks file iteration, and raises `PropertyChanged` events for progress, which the ViewModel forwards to the View.

**Strict Separation Rules:**
- The **View** contains NO business logic. It only knows how to display data and forward user actions to the ViewModel.
- The **Model** contains NO UI code. It has no knowledge of `Console` or any other presentation framework.
- The **ViewModel** contains NO direct references to UI elements (e.g., `Console.WriteLine`).


## Key Components
- **CopyEngine**: Designed for high performance and low memory footprint. It utilizes `FileStream` combined with `FileOptions.SequentialScan` hints to optimize large file transfers. It implements an event-driven approach (using `Action` delegates) to report progress, remaining files, and bytes in real-time.
   > **Note**: Progress is currently reported after each file is copied. For very large files, the UI will update only upon that file's completion. See Roadmap for planned enhancements.
- **LanguageService**: A dynamic, dictionary-based translation system. It loads flattened JSON files (`LanguageEN.json`, `LanguageFR.json`) to provide multilingual support dynamically without needing a compilation step.
- **Job Execution Parser**: Supports executing multiple backup jobs sequentially. The custom parser safely interprets user strings to extract indices, supporting range selections (`1-3`) and specific lists (`1;3`), while filtering out duplicates.


## Logging & State Management
The application features two distinct logging mechanisms:

1.  **Execution Logging (EasyLog)**: The application integrates the external **EasyLog** library to record detailed telemetry for every job execution.
    - **Daily Logs**: A new log file is created each day.
    - **Content**: Records every file transfer, including source, destination, size, and transfer duration.
    - **Formats**: Natively supports both **JSON** and **XML** formats for easy integration with monitoring tools.

2.  **State Logging**: A simple file (JSON or XML format) is used to track the state of running jobs. This log records the progress (files remaining, total size, etc.) and status (e.g., `ACTIVE`, `INACTIVE`) of each job as it runs. This allows for potential future features like pausing or resuming jobs.


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
Console.WriteLine(LanguageService.T("menu.welcome"));
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
    Expected result: Job added to the list (Status: OK).
- **Test 2:** Create a job with an invalid path. 
    Expected result: Error message displayed (Status: OK).
- **Test 3:** List jobs when no jobs are created. 
    Expected result: "Empty list" message displayed (Status: OK).
- **Test 4:** Change language during execution. 
    Expected result: UI translates immediately (Status: OK).

## Prerequisites
- .NET 10.0 SDK
- Git


## Installation & Run
```bash
git clone <your-repo-url>
cd easysave
dotnet build
dotnet run
```


## Future Roadmap
- **UI Refactoring**: Extract the UI logic currently housed in `Program.cs` into dedicated, separate `View` classes to fully comply with MVVM standards.
- **Telemetry & Logging**: Implement a robust logging system capable of recording job executions in both JSON and XML formats.
- **Progress Enhancements**: Improve the real-time progress display for massive data volumes.


# EasySave - User Manual

## Introduction
Welcome to EasySave! This console application allows you to easily configure, manage, and execute backup jobs for your directories.

## Getting Started
1. Open your terminal and navigate to the application folder.
2. Run the application using the command: `dotnet run`
3. **Language Selection**: Upon startup, you will be prompted to choose your preferred language. Type `en` for English or `fr` for French and press Enter.
4. **Format Selection**: you will also be prompted to choose your preferred format for the log files. Type `xml` for XML format or `json` for JSON format and press Enter.

## Main Menu

Once started, you will be presented with the main menu. Here are the available options:

### 1. Create a Backup Job
This option allows you to define a new backup task. You will be prompted for the following information:
- **Name**: A unique name to identify the job (e.g., "My Documents Backup").
- **Source Directory**: The full, absolute path of the folder you want to back up (e.g., `C:\Users\YourUser\Documents`).
- **Target Directory**: The full, absolute path where the backup will be stored (e.g., `D:\Backups`).

**Note**: The application enforces a maximum number of backup jobs(`5`). It also validates that all provided paths are valid and absolute.

### 2. Display All Jobs
Lists all backup jobs that have been created. Each job is displayed with a numeric ID, its name, and the date it was created. This ID is used to run or identify jobs.

### 4. Search for a Backup Job
Lets you find a specific job by typing its exact name. The application will confirm if the job exists.

### 5. Run Backup Jobs
This is the primary option for executing your backups. You can run a single job or multiple jobs sequentially. After selecting this option, you will be shown a list of all available jobs and prompted to enter your selection.

The following input formats are supported:
- **Single Job**: Enter a single ID number (e.g., `2`) to run only job #2.
- **Range of Jobs**: Enter a dash-separated range (e.g., `1-3`) to run jobs #1, #2, and #3.
- **Specific Jobs**: Enter a semicolon-separated list (e.g., `1;3`) to run job #1 and job #3.

### 6. Change Language
Instantly switch the application's display language between English (`en`) and French (`fr`). The change is applied immediately to the interface.

### 0. Exit
Safely closes the EasySave application.

## Real-Time Progress
When a backup job is running, EasySave provides real-time feedback directly in your console. The following information is displayed as the job progresses:
- **Overall Progress**: The completion percentage of the entire job.
- **Remaining Work**: The number of files and total size (in MB) left to copy.
- **Current File**: The name and size (in MB) of the file currently being transferred.

## Step-by-Step Example
1. Launch the application and select your language (`en` or `fr`).
2. Press `1` to create a new job.
3. Name it `MyWorkBackup`, set the source to `C:\Work` and the target to `D:\Backup`.
4. Press `2` to display your jobs. You will see your newly created job listed with ID `1`.
5. Press `5` to run backup jobs.
6. When prompted for your selection, type `1` and press Enter.
7. Watch the progress in real-time until completion!


## Authors
AUGER Benjamin
DOUBLET Amaury
ROURE Antoine
NGOUONPE-FEZEU-TAMEU jeffrey
