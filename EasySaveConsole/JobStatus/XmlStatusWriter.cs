using System.Xml.Linq;

public sealed class XmlStatusWriter : IStatusWriter
{
    private readonly string _filePath;

    public XmlStatusWriter(string filePath)
    {
        _filePath = filePath;
    }

    public void UpdateJobStatus(BackupJobEntry entry)
    {
        XDocument doc = File.Exists(_filePath) ? XDocument.Load(_filePath) : new XDocument(new XElement("Jobs"));

        if (doc.Root == null)
        {
            doc.Add(new XElement("Jobs"));
        }

        XElement root = doc.Root!;

        XElement? existingJob = root.Elements("Job").FirstOrDefault(e => (string?)e.Element("Name") == entry.Name);

        if (existingJob != null)
        {
            existingJob.Remove();
        }

        root.Add(ToXElement(entry));
        doc.Save(_filePath);
    }

    public void ResetJobStatus()
    {
        XDocument doc = new XDocument(new XElement("Jobs"));
        doc.Save(_filePath);
    }

    private static XElement ToXElement(BackupJobEntry entry)
    {
        var element = new XElement("Job",
            new XElement("Name", entry.Name),
            new XElement("State", entry.State),
            new XElement("LastActionTimestamp", entry.LastActionTimestamp),
            new XElement("SourceDir", entry.SourceDir),
            new XElement("TargetDir", entry.TargetDir),
            new XElement("DateCreated", entry.DateCreated)
        );

        if (entry is ActiveJobEntry active)
        {
            element.Add(
                new XElement("TotalFiles", active.TotalFiles),
                new XElement("TotalBytes", active.TotalBytes),
                new XElement("ProgressPercent", active.ProgressPercent),
                new XElement("FilesRemaining", active.FilesRemaining),
                new XElement("BytesRemaining", active.BytesRemaining),
                new XElement("CurrentSourceFile", active.CurrentSourceFile),
                new XElement("CurrentDestFile", active.CurrentDestFile)
            );
        }

        return element;
    }
}