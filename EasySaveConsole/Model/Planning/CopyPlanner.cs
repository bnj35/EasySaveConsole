using System;
using System.Collections.Generic;
using System.IO;

// Builds a CopyPlan by enumerating the source tree.
// This separates the "planning" (what to copy) from the "execution" (how to copy).
public static class CopyPlanner
{
    public static CopyPlan Build(string sourceRoot, string targetRoot)
    {
        // Safety: prevent target being inside source (infinite recursion).
        PathGuard.ThrowIfDestinationInsideSource(sourceRoot, targetRoot);

        // Fail fast if the source directory doesn't exist.
        if (!Directory.Exists(sourceRoot))
        {
            throw new DirectoryNotFoundException($"Source directory does not exist: {sourceRoot}");
        }

        // Normalize paths to absolute full paths.
        string sourceFull = Path.GetFullPath(sourceRoot);
        string targetFull = Path.GetFullPath(targetRoot);

        // Initialize the plan with roots.
        var plan = new CopyPlan
        {
            SourceRoot = sourceFull,
            TargetRoot = targetFull,
        };

        // EnumerationOptions controls how Directory.Enumerate* traverses the tree.
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
            ReturnSpecialDirectories = false,
        };

        // Enumerate directories so we can recreate empty folders in the target.
        // Directories (for empty dirs support)
        foreach (string dir in Directory.EnumerateDirectories(sourceFull, "*", options))
        {
            string relative = Path.GetRelativePath(sourceFull, dir);
            plan.Directories.Add(new DirectoryEntry(dir, relative));
        }

        // Enumerate files and record size/time info (used by progress, remaining, and future diff logic).
        // Files
        foreach (string file in Directory.EnumerateFiles(sourceFull, "*", options))
        {
            var fileInfo = new FileInfo(file);
            string relative = Path.GetRelativePath(sourceFull, file);

            plan.Files.Add(new FileEntry(
                SourceFullPath: file,
                RelativePath: relative,
                LengthBytes: fileInfo.Length,
                LastWriteTimeUtc: fileInfo.LastWriteTimeUtc
            ));

            // Aggregate total bytes so progress can be computed.
            plan.TotalBytes += fileInfo.Length;
        }

        // Validate required fields before returning.
        plan.Validate();
        return plan;
    }
}
