using System;

// EventArgs for progress changes
// Add custom info in the event
// ProgressPercent is a percentage in [0..100]
public sealed class CopyProgressChangedEventArgs : EventArgs
{
    public CopyProgressChangedEventArgs(double progressPercent)
    {
        ProgressPercent = progressPercent;
    }

    public double ProgressPercent { get; }
}
