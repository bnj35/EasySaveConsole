using System;
using System.Collections.Generic;
using System.IO;

// Builds a CopyPlan by enumerating the source tree
// This separates the "planning" (what to copy) from the "execution" (how to copy)
public static class CopyPlanner
{
    public static CopyPlan Build(string sourceRoot, string targetRoot)
    {
        // Safety: prevent target being inside source (infinite recursion)
        // We keep this guard for both directory and file sources
        PathGuard.ThrowIfDestinationInsideSource(sourceRoot, targetRoot);
// start of the "file handeling" ( I don't want to redo everything)
// you can't do file -> file it will do file -> folder/file
        // Normalize target path to an absolute full path.
        string targetFull = Path.GetFullPath(targetRoot);

        // If the source is a single file, build a plan with exactly one FileEntry
        // This allows using the same CopyEngine without changing its business logic
        if (File.Exists(sourceRoot))
        {
            string sourceFileFull = Path.GetFullPath(sourceRoot);
            string? sourceDirectory = Path.GetDirectoryName(sourceFileFull);
            if (string.IsNullOrWhiteSpace(sourceDirectory))
            {
                throw new InvalidOperationException($"Unable to determine source directory for file: {sourceRoot}");
            }

            var plan = new CopyPlan
            {
                // For a single file, SourceRoot is the parent directory (keeps RelativePath simple)
                SourceRoot = sourceDirectory,
                TargetRoot = targetFull,
            };

            var fileInfo = new FileInfo(sourceFileFull);
            string relative = Path.GetFileName(sourceFileFull);

            plan.Files.Add(new FileEntry(
                SourceFullPath: sourceFileFull,
                RelativePath: relative,
                LengthBytes: fileInfo.Length,
                LastWriteTimeUtc: fileInfo.LastWriteTimeUtc
            ));

            plan.TotalBytes = fileInfo.Length;
            plan.Validate();
            return plan;
        }
//end
        // Directory mode (existing behavior)
        // Fail fast if the source directory doesn't exist
        if (!Directory.Exists(sourceRoot))
        {
            throw new DirectoryNotFoundException($"Source directory does not exist: {sourceRoot}");
        }

        // Normalize source path to an absolute full path
        string sourceFull = Path.GetFullPath(sourceRoot);

        // Initialize the plan with roots
        var planDirectory = new CopyPlan
        {
            SourceRoot = sourceFull,
            TargetRoot = targetFull,
        };

        // EnumerationOptions controls how Directory.Enumerate* traverses the tree
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
            ReturnSpecialDirectories = false,
        };

        // Enumerate directories so we can recreate empty folders in the target
        // Directories (for empty dirs support)
        foreach (string dir in Directory.EnumerateDirectories(sourceFull, "*", options))
        {
            string relative = Path.GetRelativePath(sourceFull, dir);
            planDirectory.Directories.Add(new DirectoryEntry(dir, relative));
        }

        // Enumerate files and record size/time info (used by progress, remaining, and future diff logic)
        // Files
        foreach (string file in Directory.EnumerateFiles(sourceFull, "*", options))
        {
            var fileInfo = new FileInfo(file);
            string relative = Path.GetRelativePath(sourceFull, file);

            planDirectory.Files.Add(new FileEntry(
                SourceFullPath: file,
                RelativePath: relative,
                LengthBytes: fileInfo.Length,
                LastWriteTimeUtc: fileInfo.LastWriteTimeUtc
            ));

            // Aggregate total bytes so progress can be computed
            planDirectory.TotalBytes += fileInfo.Length;
        }

        // Validate required fields before returning
        planDirectory.Validate();
        return planDirectory;
    }
}
