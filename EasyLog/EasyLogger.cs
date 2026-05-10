using System.Text.Json;
using System.Xml.Linq;

namespace EasyLog
{
    public class EasyLogger
    {
        private readonly string _logDirectory;

        public EasyLogger(string logDirectory)
        { 
            _logDirectory = logDirectory;
        }

        public void Save(LogEntry? entry, string format)
        {
            if (entry != null)
            {
                Directory.CreateDirectory(_logDirectory);
                if (format == "xml")
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

            XDocument doc = File.Exists(fullPath) ? XDocument.Load(fullPath) : new XDocument(new XElement("Logs"));

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

            doc.Root!.Add(element);
            doc.Save(fullPath);
        }

        private void WriteJson(LogEntry entry)
        {
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";
            string fullPath = Path.Combine(_logDirectory, fileName);
            List<JsonElement> entries = [];
            if (File.Exists(fullPath))
            {
                string existingJson = File.ReadAllText(fullPath);

                if (!string.IsNullOrWhiteSpace(existingJson))
                {
                    try
                    {
                        entries = JsonSerializer.Deserialize<List<JsonElement>>(existingJson) ?? [];
                    }
                    catch (JsonException)
                    {
                        entries = [];
                    }
                }
            }

            entries.Add(JsonSerializer.SerializeToElement(entry, entry.GetType()));

            string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(fullPath, json);
        }
    }
}