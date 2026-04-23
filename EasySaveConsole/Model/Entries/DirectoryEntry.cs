public sealed class DirectoryEntry
{
    public string SourceFullPath {get; set; }
    public string RelativePath {get; set; }

    public DirectoryEntry(string sourceFullPath, string relativePath)
    {
        SourceFullPath = sourceFullPath;
        RelativePath = relativePath;
    }
}
