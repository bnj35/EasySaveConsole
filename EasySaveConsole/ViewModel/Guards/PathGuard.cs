using System;
using System.IO;

// Guard utilities: reject dangerous path combinations
// Main case: target inside source would cause an infinite copy loop
public static class PathGuard
{
    public static void ThrowIfDestinationInsideSource(string sourceDirectory, string targetDirectory)
    {
        // Validate parameters early
        if (string.IsNullOrWhiteSpace(sourceDirectory))
        {
            throw new ArgumentException("Source directory is required.", nameof(sourceDirectory));
        }

        if (string.IsNullOrWhiteSpace(targetDirectory))
        {
            throw new ArgumentException("Target directory is required.", nameof(targetDirectory));
        }

        // Normalize to full paths and ensure a trailing separator for safe prefix checks
        string sourceFullPath = NormalizeDirectoryPath(sourceDirectory);
        string targetFullPath = NormalizeDirectoryPath(targetDirectory);

        // Disallow target == sources
        if (IsSamePath(sourceFullPath, targetFullPath))
        {
            throw new InvalidOperationException("Target directory cannot be the same as the source directory.");
        }

        // Disallow target inside source (example: /data -> /data/backup)
        if (IsSubPathOf(targetFullPath, sourceFullPath))
        {
            throw new InvalidOperationException("Target directory cannot be inside the source directory (would cause an infinite loop).");
        }
    }

    public static string NormalizeDirectoryPath(string path)
    {
        // GetFullPath also resolves relative segments like '.' and '..'
        string full = Path.GetFullPath(path);

        // Add a trailing separator so that "C:\foo" doesn't match "C:\foobar"
        // Ensure trailing separator so prefix checks are safe
        if (!full.EndsWith(Path.DirectorySeparatorChar) && !full.EndsWith(Path.AltDirectorySeparatorChar))
        {
            full += Path.DirectorySeparatorChar;
        }

        return full;
    }

    private static bool IsSubPathOf(string candidateFullPathWithSep, string parentFullPathWithSep)
    {
        // Windows: case-insensitive by default; Unix-like: case-sensitive
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return candidateFullPathWithSep.StartsWith(parentFullPathWithSep, comparison);
    }

    private static bool IsSamePath(string aFullPathWithSep, string bFullPathWithSep)
    {
        // Same OS-specific comparison rules as IsSubPathOf
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(aFullPathWithSep, bFullPathWithSep, comparison);
    }
}
