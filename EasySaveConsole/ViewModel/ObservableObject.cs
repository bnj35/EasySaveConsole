using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// Shared base class for objects that need change notifications
// Any class that inherits from this can notify when one property value changes
public abstract class ObservableObject : INotifyPropertyChanged
{
    // Standard .NET notification event used by INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    // Raise a notification for one property name
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // Helper used in property setters:
    // 1) compare old/new value
    // 2) assign the new value
    // 3) raise the change notification
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
