using System.Globalization;
using Avalonia.Data.Converters;
namespace MegaMarketLibnk.Converters;
public sealed class CurrencyConverter : IValueConverter
{
    public static readonly CurrencyConverter Instance = new();
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal d)
            return $"{d.ToString("0,0", CultureInfo.GetCultureInfo("ru-RU")).Replace(',', ' ')} ₽";
        if (value is int i)
            return $"{i.ToString("0,0", CultureInfo.GetCultureInfo("ru-RU")).Replace(',', ' ')} ₽";
        if (value is double db)
            return $"{db.ToString("0,0", CultureInfo.GetCultureInfo("ru-RU")).Replace(',', ' ')} ₽";
        return value;
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
