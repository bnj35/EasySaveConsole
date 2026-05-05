using System.Xml.Linq;

public class XmlLogWriter : ILogWriter
{
    private readonly string _logDirectory;

    public XmlLogWriter(string logDirectory)
    {
        _logDirectory = logDirectory;
        Directory.CreateDirectory(logDirectory);
    }

    public void Write(LogEntry entry)
    {
        string fileName = $"{DateTime.Now:yyyy-MM-dd}.xml";
        string fullPath = Path.Combine(_logDirectory, fileName);

        XDocument doc = File.Exists(fullPath) ? XDocument.Load(fullPath) : new XDocument(new XElement("Logs"));

        doc.Root!.Add(ToXElement(entry));
        doc.Save(fullPath);
    }

    private static XElement ToXElement(LogEntry entry)
    {
        var element = new XElement("Entry",
            new XElement("Action", entry.Action),
            new XElement("Name", entry.Name),
            new XElement("Time", entry.Time)
        );

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

        return element;
    }
}