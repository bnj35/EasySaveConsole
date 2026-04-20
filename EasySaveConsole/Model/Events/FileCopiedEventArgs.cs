using System;

// EventArgs emitted after copying one file
// Add custom info in the event
// Includes source/destination paths and simple timing for telemetry/logging
public sealed class FileCopiedEventArgs : EventArgs
{
    public FileCopiedEventArgs(string sourcePath, string destinationPath, long bytesCopied, double transferMilliseconds)
    {
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
        BytesCopied = bytesCopied;
        TransferMilliseconds = transferMilliseconds;
    }

    public string SourcePath { get; }

    public string DestinationPath { get; }

    public long BytesCopied { get; }

    public double TransferMilliseconds { get; }
}
