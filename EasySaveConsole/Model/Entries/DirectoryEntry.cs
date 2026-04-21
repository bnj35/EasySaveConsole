// DirectoryEntry is a small immutable value type describing one directory to recreate
// This is what allows the copy to preserve empty directories
// struct used bc it's lighter than a class
public readonly record struct DirectoryEntry(
    string SourceFullPath,
    string RelativePath
);
