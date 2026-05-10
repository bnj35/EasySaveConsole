
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace EasyLog
{
    public record LogRequest(string Format, string Type, string Entry);

    public class Program
    {
        static void Main()
        {
            EasyLogger logger = new EasyLogger("./log");
            Socket serverSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 5000);
            serverSocket.Bind(endPoint);
            Console.WriteLine("Waiting for connection..");
            byte[] buffer = new byte[2048];

            while (true)
            {
                int nbBytesReceived = serverSocket.Receive(buffer);

                string json = Encoding.UTF8.GetString(buffer, 0, nbBytesReceived);
                Console.WriteLine("Data received : {0}", json);

                LogRequest? logRequest = JsonSerializer.Deserialize<LogRequest>(json);
                if (logRequest != null)
                {
                    if (logRequest.Type == "file")
                    {
                        FileLogEntry? logEntry = JsonSerializer.Deserialize<FileLogEntry>(logRequest.Entry);
                        logger.Save(logEntry, logRequest.Format);
                    }
                    else if (logRequest.Type == "directory")
                    {
                        DirectoryLogEntry? logEntry = JsonSerializer.Deserialize<DirectoryLogEntry>(logRequest.Entry);
                        logger.Save(logEntry, logRequest.Format);
                    }
                }
            }
        }
    }
}