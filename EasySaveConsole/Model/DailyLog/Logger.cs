using EasyLog;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

public record LogRequest(LogFileFormats Format, LogActions Actions, string Entry);

public class Logger
{
	private Settings _settings;
	private EasyLogger _logger;

	public Logger(Settings settings)
	{
		_settings = settings;
		_logger = new EasyLogger(_settings.EasyLogSettings.DirectoryPath);
	}

    public void LogDirectoryCreation(string jobName, string directory)
	{
        DirectoryLogEntry entry = new(
            LogActions.DirectoryCreation,
			jobName,
			DateTime.Now.ToString(_settings.DateFormat),
			directory);

		Persist(entry, LogActions.DirectoryCreation);
    }

	public void LogFileCopy(string jobName, string sourceFullPath, string destFile, long fileSize, double fileTransfertTime, double encryptionTime)
	{
		FileLogEntry entry = new(
			LogActions.FileCopy, 
			jobName, 
			DateTime.Now.ToString(_settings.DateFormat),
			sourceFullPath,
			destFile,
			fileSize,
			fileTransfertTime,
			encryptionTime);

		Persist(entry, LogActions.FileCopy);
    }

	private void Persist(LogEntry entry, LogActions action)
	{
		LogRequest logRequest;
		switch (_settings.EasyLogSettings.LogStorage)
		{
			case LogStorageModes.Remote:
				logRequest = new LogRequest(_settings.LogFileFormat, action, JsonSerializer.Serialize(entry));
				SendLog(JsonSerializer.Serialize(logRequest));
				break;

			case LogStorageModes.Local:
				_logger.Log(entry, _settings.LogFileFormat);
				break;

			case LogStorageModes.Both:
				_logger.Log(entry, _settings.LogFileFormat);
				logRequest = new LogRequest(_settings.LogFileFormat, action, JsonSerializer.Serialize(entry));
				SendLog(JsonSerializer.Serialize(logRequest));
				break;

			default:
				throw new ArgumentException($"Unknown storage option: {_settings.EasyLogSettings.LogStorage}");
		}
    }

    private void SendLog(string logRequest)
	{
		try
		{
		byte[] data = Encoding.UTF8.GetBytes(logRequest);
	// ouvre une socket à chaque fois
		using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(IPAddress.Loopback, 5000);
		socket.Send(data);
		}catch( Exception ex)
		{
			Console.Error.WriteLine($"socket error Exception={ex}");
        throw;
		}

	}
}
