using System.Reflection.PortableExecutable;


namespace EasySaveConsole
{

    public static class CopyPlanner
    {
        public static CopyPlan Build(
            string sourceRoot,
            string targetRoot,
            Action<int, string>? onItemScanned = null,
            CancellationToken cancellationToken = default)
        {
            PathGuard.IsLooping(sourceRoot, targetRoot);


            string targetFull = Path.GetFullPath(targetRoot);
            string sourceFull = Path.GetFullPath(sourceRoot);

            CopyPlan plan = new CopyPlan(sourceFull, targetFull);

            var parameters = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.ReparsePoint,
                ReturnSpecialDirectories = false
            };

            IEnumerable<string> list = Enumerable.Empty<string>();;

            if (File.Exists(sourceFull))
            {
                list = list.Append(sourceFull);
            }
            else
            {
                list = Directory.EnumerateFileSystemEntries(sourceFull, "*", parameters);
            }

            foreach (string entry in list)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (File.Exists(entry))
                {
                    var fileInfo = new FileInfo(entry);
                    string relativeFile = Path.GetRelativePath(sourceFull, entry);
                    plan.Files.Add(new FileEntry(entry, relativeFile, fileInfo.Length, fileInfo.LastWriteTimeUtc));

                    plan.TotalBytes += (int)fileInfo.Length;
                    onItemScanned?.Invoke(plan.Directories.Count + plan.Files.Count, relativeFile);
                }
                else
                {
                    string relativeDir = Path.GetRelativePath(sourceFull, entry);
                    plan.Directories.Add(new DirectoryEntry(entry, relativeDir));
                    onItemScanned?.Invoke(plan.Directories.Count + plan.Files.Count, relativeDir);
                }
            }
            plan.Validate();

            return plan;
        }
        public static CopyPlan GetPriorityPlan(CopyPlan originalPlan, List<string> priorityExtensions)
        {
            CopyPlan priorityPlan = new CopyPlan(originalPlan.SourceRoot, originalPlan.TargetRoot);
            
            priorityPlan.Directories.AddRange(originalPlan.Directories);
            
            var priorityFiles = originalPlan.GetPriorityFiles(priorityExtensions);
            priorityPlan.Files.AddRange(priorityFiles);
            
            priorityPlan.TotalBytes = priorityFiles.Sum(f => (int)f.LengthBytes);

            return priorityPlan;
        }
        public static CopyPlan GetNonPriorityPlan(CopyPlan originalPlan, List<string> priorityExtensions)
        {
            CopyPlan nonPriorityPlan = new CopyPlan(originalPlan.SourceRoot, originalPlan.TargetRoot);
            
            nonPriorityPlan.Directories.AddRange(originalPlan.Directories);
            
            var nonPriorityFiles = originalPlan.GetNonPriorityFiles(priorityExtensions);
            nonPriorityPlan.Files.AddRange(nonPriorityFiles);
    
            nonPriorityPlan.TotalBytes = nonPriorityFiles.Sum(f => (int)f.LengthBytes);

            return nonPriorityPlan;
        }
        public static List<string> ParsePriorityExtensions(string extensionsString)
        {
            if (string.IsNullOrWhiteSpace(extensionsString))
                return new List<string>();

            return extensionsString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => ext.Trim().ToLower())
                .Where(ext => ext.StartsWith(".") || ext.Length > 0)
                .Select(ext => ext.StartsWith(".") ? ext : $".{ext}")
                .ToList();
        }
    }
}
