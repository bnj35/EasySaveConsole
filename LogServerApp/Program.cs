using EasyLog;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
public class Program
{
    public record LogRequest(string Format, LogActions Actions, string Entry);
    static void Main()
    {
        EasyLogger logger = new EasyLogger("/app/logs");

        Socket serverSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 5000);
        serverSocket.Bind(endPoint);
        byte[] buffer = new byte[2048];

        while (true)
        {
            int nbBytesReceived = serverSocket.Receive(buffer);
            string json = Encoding.UTF8.GetString(buffer, 0, nbBytesReceived);

            LogRequest? logRequest = JsonSerializer.Deserialize<LogRequest>(json);

            if (logRequest == null)
            {
                continue;
            }
            if (logRequest.Actions == LogActions.FileCopy)
            {
                FileLogEntry? entry = JsonSerializer.Deserialize<FileLogEntry>(logRequest.Entry);
                logger.Log(entry, logRequest.Format);
            }
            if (logRequest.Actions == LogActions.DirectoryCreation)
            {
                DirectoryLogEntry? entry = JsonSerializer.Deserialize<DirectoryLogEntry>(logRequest.Entry);
                logger.Log(entry, logRequest.Format);
            }
        }
    }
}
