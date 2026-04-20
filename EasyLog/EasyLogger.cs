
using System.Text.Json;

namespace EasyLog
{
    public class EasyLogger
    {
        private static EasyLogger? _instance;
        private readonly string logDirectory;

        private EasyLogger(string logDirectory)
        {
            this.logDirectory = logDirectory;
            Directory.CreateDirectory(logDirectory);
        }

        /// <summary>
        /// Returns the unique instance of the logger.
        /// Creates it if it doesn't exist yet.
        /// </summary>
        public static EasyLogger GetInstance(string logDirectory = "log")
        {
            if (_instance == null)
            {
                _instance = new EasyLogger(logDirectory);
            }
            return _instance;
        }

        /// <summary>
        /// Logs the copy of a file
        /// </summary>
        public void LogFileCopy(string backupName, string sourcePath, string destinationPath, long fileSize, double transferTime)
        {
            var entry = new FileLogEntry(
                LogActions.FILE_COPY,
                backupName,
                DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                sourcePath,
                destinationPath,
                fileSize,
                transferTime
            );
            WriteLog(entry);
        }

        /// <summary>
        /// Logs the creation of a directory
        /// </summary>
        public void LogDirectoryCreation(string backupName, string targetPath)
        {
            var entry = new DirectoryLogEntry(
                LogActions.DIRECTORY_CREATION,
                backupName,
                DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                targetPath
            );
            WriteLog(entry);
        }

        /// <summary>
        /// Writes a log entry to the daily log file.
        /// Each entry is written on its own line (JSON Lines format).
        /// </summary>
        private void WriteLog(LogEntry entry)
        {
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.jsonl";
            string fullPath = Path.Combine(logDirectory, fileName);

            var options = new JsonSerializerOptions { WriteIndented = false };
            string json = JsonSerializer.Serialize(entry, entry.GetType(), options);
            File.AppendAllText(fullPath, json + Environment.NewLine);
        }
    }
}

