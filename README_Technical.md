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


## Authors
ROURE Antoine
AUGER Benjamin
NGOUONPE-FEZEU-TAMEU jeffrey
DOUBLET Amaury