using Avalonia.Data.Converters;
using ExCSS;
using System;
using System.Globalization;

namespace AdminPanel.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new ArgumentException($"Target type must be {nameof(Visibility)}.", nameof(targetType));
            }

            return value is string stringValue && !string.IsNullOrEmpty(stringValue) ? Visibility.Visible : Visibility.Hidden;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(StringToVisibilityConverter)} does not support two-way binding.");
        }
    }
}