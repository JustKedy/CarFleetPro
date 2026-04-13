namespace CarFleetPro.Mobile.Converters;

/// <summary>
/// IsLoading=true  → IsVisible=false  (gerçek içerik gizlenir)
/// IsLoading=false → IsVisible=true   (gerçek içerik gösterilir)
/// Skeleton Screen pattern'inde kullanılır.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => value is bool b && !b;

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => value is bool b && !b;
}
