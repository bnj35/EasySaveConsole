namespace EasyLog
{
    public class EasyLogger
    {
        private static EasyLogger? _instance;
        private readonly ILogWriter writer;
        private string _dateFormat;

        private EasyLogger(ILogWriter writer, string dateFormat)
        {
            this.writer = writer;
            this._dateFormat = dateFormat;
        }

        /// <summary>
        /// Returns the unique instance of the logger.
        /// Creates it if it doesn't exist yet.
        /// </summary>
        public static EasyLogger GetInstance(string logDirectory, string dateFormat)
        {
            if (_instance == null)
            {
                ILogWriter writer = new FileLogWriter(logDirectory);
                _instance = new EasyLogger(writer, dateFormat);
            }
            return _instance;
        }

        /// <summary>
        /// Logs the copy of a file
        /// </summary>
        public void LogFileCopy(string backupName, string sourcePath, string destinationPath, long fileSize, double transferTime)
        {
            FileLogEntry entry = new FileLogEntry(
                Configuration.FILE_COPY,
                backupName,
                DateTime.Now.ToString(_dateFormat),
                sourcePath,
                destinationPath,
                fileSize,
                transferTime
            );
            writer.Write(entry);
        }

        /// <summary>
        /// Logs the creation of a directory
        /// </summary>
        public void LogDirectoryCreation(string backupName, string targetPath)
        {
            DirectoryLogEntry entry = new DirectoryLogEntry(
                Configuration.DIRECTORY_CREATION,
                backupName,
                DateTime.Now.ToString(_dateFormat),
                targetPath
            );
            writer.Write(entry);
        }
    }
}