namespace EasyLog
{
    public class EasyLogger
    {
        private static EasyLogger? _instance;
        private readonly ILogWriter writer;
        private string DateFormat;

        private EasyLogger(ILogWriter writer, string dateFormat)
        {
            this.writer = writer;
            this.DateFormat = dateFormat;
        }

        /// <summary>
        /// Returns the unique instance of the logger.
        /// Creates it if it doesn't exist yet.
        /// </summary>
        public static EasyLogger GetInstance(string logDirectory, string dateFormat, string logFormat)
        {
            if (_instance == null)
            {
                if (logFormat == "xml")
                {
                    _instance = new EasyLogger(new XmlLogWriter(logDirectory), dateFormat);
                }
                else
                {
                    _instance = new EasyLogger(new JsonLogWriter(logDirectory), dateFormat);
                }
            }
            return _instance;
        }

        /// <summary>
        /// Logs the copy of a file
        /// </summary>
        public void LogFileCopy(string backupName, string sourcePath, string destinationPath, long fileSize, double transferTime, double encryptionTime = 0)
        {
            FileLogEntry entry = new FileLogEntry(
                Configuration.FILE_COPY,
                backupName,
                DateTime.Now.ToString(DateFormat),
                sourcePath,
                destinationPath,
                fileSize,
                transferTime,
                encryptionTime
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
                DateTime.Now.ToString(DateFormat),
                targetPath
            );
            writer.Write(entry);
        }
    }
}