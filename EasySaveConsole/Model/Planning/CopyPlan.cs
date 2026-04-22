using System;
using System.Collections.Generic;

// CopyPlan = the "what" to copy: roots + lists of directories/files + totals.
// The engine consumes this plan to perform the actual filesystem operations.
public sealed class CopyPlan
{
    public required string SourceRoot { get; init; }
    public required string TargetRoot { get; init; }

    // Directories list is used so empty folders can be recreated.
    public List<DirectoryEntry> Directories { get; } = new();

    // Files list contains metadata required for copying + progress.
    public List<FileEntry> Files { get; } = new();

    // TotalBytes is filled during planning (sum of file sizes).
    public long TotalBytes { get; internal set; }

    // TotalFiles is derived from the plan content.
    public int TotalFiles => Files.Count;

    public void Validate()
    {
        // Minimal sanity checks to avoid executing with missing roots.
        if (string.IsNullOrWhiteSpace(SourceRoot)) throw new InvalidOperationException("SourceRoot is required");
        if (string.IsNullOrWhiteSpace(TargetRoot)) throw new InvalidOperationException("TargetRoot is required");
    }
}
