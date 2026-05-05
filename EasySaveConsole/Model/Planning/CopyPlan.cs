namespace EasySaveConsole
{
    public sealed class CopyPlan
    {
        public required string SourceRoot { get; init; }//set uniquement à la création de l'objet
        public required string TargetRoot { get; init; }

        public List<DirectoryEntry> Directories { get; } = new();

        public List<FileEntry> Files { get; } = new();

        public int TotalBytes { get; set; }

        public int TotalFiles => Files.Count;

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers] // pour avoir les required -> ne marche pas sans 🤷🏼
        public CopyPlan(string sourceRoot, string targetRoot)
        {
            SourceRoot = sourceRoot;
            TargetRoot = targetRoot;
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