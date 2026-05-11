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

            bool isFile = false;

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
    }
}