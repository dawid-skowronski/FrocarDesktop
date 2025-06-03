using Avalonia.Data.Converters;
using ExCSS;
using System;
using System.Globalization;

namespace AdminPanel.Converters
{
    public class DecimalToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new ArgumentException($"Target type must be {nameof(Visibility)}.", nameof(targetType));
            }

            return value is decimal decimalValue && decimalValue > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(DecimalToVisibilityConverter)} does not support two-way binding.");
        }
    }
}