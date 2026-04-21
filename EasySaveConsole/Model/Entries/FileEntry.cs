using System;

// FileEntry is a small immutable value type describing one file to copy
// RelativePath is used to rebuild the same folder structure under the target root
// struct used bc it's lighter than a class
public readonly record struct FileEntry(
    string SourceFullPath,
    string RelativePath,
    long LengthBytes,
    DateTime LastWriteTimeUtc
);
