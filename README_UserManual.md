# EasySave - User Manual

## Introduction
Welcome to EasySave! This console application allows you to easily configure, manage, and execute backup jobs for your directories.

## Getting Started
1. Open your terminal and navigate to the application folder.
2. Run the application using the command: `dotnet run`
3. **Language Selection**: Upon startup, you will be prompted to choose your preferred language. Type `en` for English or `fr` for French and press Enter.
4. **Format Selection**: You will also be prompted to choose the file format for the log files. Type `json` for JSON format or `xml` for XML format and press Enter.

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
