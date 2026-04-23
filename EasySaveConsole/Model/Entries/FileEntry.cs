public sealed class FileEntry
{
    public string SourceFullPath { get; set; }
    public string RelativePath { get; set; }
    public long LengthBytes { get; set; }
    public DateTime LastWriteTimeUtc { get; set; }

    public FileEntry(string sourceFullPath, string relativePath, long lengthBytes, DateTime lastWriteTimeUtc)
    {
        SourceFullPath = sourceFullPath;
        RelativePath = relativePath;
        LengthBytes = lengthBytes;
        LastWriteTimeUtc = lastWriteTimeUtc;
    }
}