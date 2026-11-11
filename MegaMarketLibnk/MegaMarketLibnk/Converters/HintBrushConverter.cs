using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
namespace MegaMarketLibnk.Converters;
public sealed class HintBrushConverter : IValueConverter
{
    public static readonly HintBrushConverter Instance = new();
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hint = value as string ?? string.Empty;
        return hint.StartsWith('✓')
            ? new SolidColorBrush(Color.Parse("#66E08A"))
            : hint.StartsWith('✗')
                ? new SolidColorBrush(Color.Parse("#FF8A7A"))
                : new SolidColorBrush(Color.Parse("#B9B9B9"));
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
