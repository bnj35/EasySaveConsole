using EasyLog;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogStorage
{
    Remote,
    Local,
    Both
}
public record LogRequest(string Format, LogActions Actions, string Entry);

public class Logger
{
	private Socket _socket;
	private Settings _settings;
	private EasyLogger _logger;

	public Logger(Settings settings)
	{
		_socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
		_socket.Connect(IPAddress.Loopback, 5000);
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
			case LogStorage.Remote:
				logRequest = new LogRequest(_settings.DefaultFileFormat, action, JsonSerializer.Serialize(entry));
				SendLog(JsonSerializer.Serialize(logRequest));
				break;

			case LogStorage.Local:
				_logger.Log(entry, _settings.DefaultFileFormat);
				break;

			case LogStorage.Both:
				_logger.Log(entry, _settings.DefaultFileFormat);
				logRequest = new LogRequest(_settings.DefaultFileFormat, action, JsonSerializer.Serialize(entry));
				SendLog(JsonSerializer.Serialize(logRequest));
				break;

			default:
				throw new ArgumentException($"Unknown storage option: {_settings.EasyLogSettings.LogStorage}");
		}
    }

    private void SendLog(string logRequest)
	{
		byte[] data = Encoding.UTF8.GetBytes(logRequest);
		_socket.Send(data);
	}
}
