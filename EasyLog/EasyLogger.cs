using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace EasyLog
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LogFileFormats
    {
        xml,
        json,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LogActions
    {
        FileCopy,
        DirectoryCreation
    }

    public class EasyLogger
    {
        private readonly string _logDirectory;
        private static readonly object _fileLock = new();

        public EasyLogger(string logDirectory)
        {
            _logDirectory = logDirectory;
        }

        public void Log(LogEntry? entry, LogFileFormats format)
        {
            if (entry != null)
            {
                Directory.CreateDirectory(_logDirectory);
                if (format == LogFileFormats.xml)
                {
                    WriteXml(entry);
                }
                else
                {
                    WriteJson(entry);
                }
            }
        }

        private void WriteXml(LogEntry entry)
        {
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.xml";
            string fullPath = Path.Combine(_logDirectory, fileName);

            var element = new XElement("Entry",
                new XElement("Action", entry.Action),
                new XElement("Name", entry.Name),
                new XElement("Time", entry.Time));

            if (entry is FileLogEntry file)
            {
                element.Add(
                    new XElement("FileSource", file.FileSource),
                    new XElement("FileTarget", file.FileTarget),
                    new XElement("FileSize", file.FileSize),
                    new XElement("FileTransfertTime", file.FileTransfertTime),
                    new XElement("EncryptionTime", file.EncryptionTime)
                );
            }
            else if (entry is DirectoryLogEntry dir)
            {
                element.Add(new XElement("Target", dir.Target));
            }

            lock (_fileLock)
            {
                XDocument doc;
                if (File.Exists(fullPath))
                {
                    try
                    {
                        doc = XDocument.Load(fullPath);
                    }
                    catch (Exception)
                    {
                        File.Delete(fullPath);
                        doc = new XDocument(new XElement("Logs"));
                    }
                }
                else
                {
                    doc = new XDocument(new XElement("Logs"));
                }

                doc.Root!.Add(element);
                doc.Save(fullPath);
            }
        }

        private void WriteJson(LogEntry entry)
        {
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";
            string fullPath = Path.Combine(_logDirectory, fileName);

            var serialized = JsonSerializer.SerializeToElement(entry, entry.GetType());

            lock (_fileLock)
            {
                List<JsonElement> entries = [];
                if (File.Exists(fullPath))
                {
                    string existingJson = File.ReadAllText(fullPath);
                    try
                    {
                        entries = JsonSerializer.Deserialize<List<JsonElement>>(existingJson) ?? [];
                    }
                    catch (JsonException)
                    {
                        entries = [];
                    }
                }

                entries.Add(serialized);

                string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(fullPath, json);
            }
        }
    }
}