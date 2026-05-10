using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

public record LogRequest(string Format, string Type, string Entry);

public class Logger
{
	private Socket _socket;
	private Settings _settings;

	public Logger(Settings settings)
	{
		_socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
		_socket.Connect(IPAddress.Loopback, 5000);
		_settings = settings;
	}

    public void LogDirectoryCreation(string jobName, string directory)
	{
        DirectoryLogEntry entry = new(
            "DIRECTORY_CREATION",
			jobName,
			DateTime.Now.ToString(_settings.DateFormat),
			directory);

		var logRequest = new LogRequest(_settings.DefaultFileFormat, "directory", JsonSerializer.Serialize(entry));
		Console.WriteLine(JsonSerializer.Serialize(entry));

        SendLog(JsonSerializer.Serialize(logRequest));
    }

	public void LogFileCopy(string jobName, string sourceFullPath, string destFile, long fileSize, double fileTransfertTime, double encryptionTime)
	{
		FileLogEntry entry = new(
			"FILE_COPY", 
			jobName, 
			DateTime.Now.ToString(_settings.DateFormat),
			sourceFullPath,
			destFile,
			fileSize,
			fileTransfertTime,
			encryptionTime);

        var logRequest = new LogRequest(_settings.DefaultFileFormat, "file", JsonSerializer.Serialize(entry));

        SendLog(JsonSerializer.Serialize(logRequest));
    }

    private void SendLog(string logRequest)
	{
		byte[] data = Encoding.UTF8.GetBytes(logRequest);
		_socket.Send(data);
	}
}
