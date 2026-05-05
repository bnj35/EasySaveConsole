using System.Net;
using System.Security.Cryptography.X509Certificates;
using EasySaveConsole;
public static class PathGuard
{
    public static void IsLooping(string source_dir,string target_dir)
    {
        if (string.IsNullOrWhiteSpace(source_dir) && string.IsNullOrWhiteSpace(target_dir))
        {
            throw new ArgumentException(LanguageService.T("error.pathguard.both.empty"));
        }

        string sourceFullPath = NormalizeDirectoryPath(source_dir);
        string targetFullPath = NormalizeDirectoryPath(target_dir);

        if (IsSamePath(sourceFullPath, targetFullPath))
        {
            throw new InvalidOperationException(LanguageService.T("error.pathguard.same.path"));
        }

        if(IsSubPath(sourceFullPath, targetFullPath))
        {
            throw new InvalidOperationException(LanguageService.T("error.pathguard.sub.path"));
        }
    }

        public static string NormalizeDirectoryPath(string path)
    {
        string full = Path.GetFullPath(path);

        if (!full.EndsWith(Path.DirectorySeparatorChar) && !full.EndsWith(Path.AltDirectorySeparatorChar))
        {
            full += Path.DirectorySeparatorChar;
        }

        return full;
    }

    public static bool IsSamePath(string source_dir,string target_dir)
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        bool result = string.Equals(source_dir,target_dir,comparison);

        return result;
        
    }

    public static bool IsSubPath(string source_dir, string target_dir)
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        bool result = source_dir.StartsWith(target_dir,comparison);

        return result;
    }

}