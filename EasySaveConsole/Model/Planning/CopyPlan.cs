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

        public int TotalFiles => Files.Count + Directories.Count;

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
        public List<FileEntry> GetPriorityFiles(List<string> priorityExtensions)
        {
            return Files.Where(f => IsPriorityFile(f, priorityExtensions)).ToList();
        }

        public List<FileEntry> GetNonPriorityFiles(List<string> priorityExtensions)
        {
            return Files.Where(f => !IsPriorityFile(f, priorityExtensions)).ToList();
        }
        private static bool IsPriorityFile(FileEntry file, List<string> priorityExtensions)
        {
            if (priorityExtensions == null || priorityExtensions.Count == 0)
                return false;

            string fileExtension = Path.GetExtension(file.SourceFullPath).ToLower();
            return priorityExtensions.Any(ext => 
                ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        }
    }
}
