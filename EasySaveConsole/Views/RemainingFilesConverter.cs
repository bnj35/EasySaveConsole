using Avalonia.Data.Converters;
using System.Globalization;

namespace EasySaveConsole;

public class RemainingFilesConverter : IValueConverter
{
    public static readonly RemainingFilesConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.Format(LanguageService.T("main.jobs.remaining.files"), value ?? 0);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
