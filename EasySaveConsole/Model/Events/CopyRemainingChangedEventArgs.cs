using System;

// EventArgs for remaining work changes
// Add custom info in the event
// FilesRemaining/BytesRemaining are counters after processing the current file
public sealed class CopyRemainingChangedEventArgs : EventArgs
{
    public CopyRemainingChangedEventArgs(int filesRemaining, long bytesRemaining)
    {
        FilesRemaining = filesRemaining;
        BytesRemaining = bytesRemaining;
    }

    public int FilesRemaining { get; }

    public long BytesRemaining { get; }
}
