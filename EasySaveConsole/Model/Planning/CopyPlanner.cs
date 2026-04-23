using System.ComponentModel;
using System.IO.Enumeration;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;

public static class CopyPlanner
{
    public static CopyPlan Build(string sourceRoot, string targetRoot)
    {
        PathGuard.IsLooping(sourceRoot,targetRoot);

        string targetFull = Path.GetFullPath(targetRoot);
        string sourceFull = Path.GetFullPath(sourceRoot);

        string? sourceDirectory = Path.GetDirectoryName(sourceFull);

        if (File.Exists(sourceRoot))
        {

            if (string.IsNullOrWhiteSpace(sourceDirectory))
            {
                throw new InvalidOperationException(LanguageService.T("error.copyplanner.source.parent"));
            }

            CopyPlan plan = new CopyPlan(sourceDirectory,targetFull);

            var fileInfo = new FileInfo(sourceFull);

            string relative = Path.GetFileName(sourceFull);

            plan.Files.Add(new FileEntry(sourceFull,relative,fileInfo.Length,fileInfo.LastWriteTimeUtc));

            plan.TotalBytes = (int)fileInfo.Length; // cast => transforme le long en int implicitement
            plan.Validate();


            return plan;
        }

        if (!Directory.Exists(sourceRoot))
        {
            throw new DirectoryNotFoundException(string.Format(LanguageService.T("error.path.notfound"), sourceRoot));
        }

        if (string.IsNullOrWhiteSpace(sourceDirectory))
        {
            throw new InvalidOperationException(LanguageService.T("error.copyplanner.source.parent"));
        }

        CopyPlan planDirectory = new CopyPlan(sourceDirectory,targetFull);

        var parameters = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
            ReturnSpecialDirectories = false
        };

        var directoryList = Directory.EnumerateDirectories(sourceFull,"*",parameters);
        var fileList = Directory.EnumerateFiles(sourceFull,"*",parameters);

        foreach (string dir in directoryList)
        {
            string relative = Path.GetRelativePath(sourceFull,dir);
            planDirectory.Directories.Add(new DirectoryEntry(dir,relative));

        }

        foreach(string file in fileList)
        {
            var fileInfo = new FileInfo(file);

            string relative = Path.GetRelativePath(sourceFull,file);
            planDirectory.Files.Add(new FileEntry(file,relative,fileInfo.Length,fileInfo.LastWriteTimeUtc));

            planDirectory.TotalBytes += (int)fileInfo.Length;
        }

        planDirectory.Validate();

        return planDirectory;
    }
}