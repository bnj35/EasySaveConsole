using System.Collections;
namespace EasySaveConsole
{
    public sealed class CopyPlan: IEnumerable
    {
        public required string SourceRoot { get; init; }
        public required string TargetRoot { get; init; }

        public List<DirectoryEntry> Directories { get; } = new();

        public List<FileEntry> Files { get; } = new();

        public int TotalBytes { get; set; }

        public int TotalFiles => Files.Count;

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public CopyPlan(string sourceRoot, string targetRoot)
        {
            SourceRoot = sourceRoot;
            TargetRoot = targetRoot;
        }

        // pour avoir les types des entrées dans engine
        public IEnumerator GetEnumerator()
        {
            foreach (var dir in Directories) yield return dir;
            foreach (var file in Files) yield return file;
        }
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SourceRoot))
            {
                throw new InvalidOperationException(LanguageService.T("error.copyplan.source.empty"));
            }

            if (string.IsNullOrWhiteSpace(TargetRoot))
            {
                throw new InvalidOperationException(LanguageService.T("error.copyplan.target.empty"));
            }
        }
    }
}