using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AdminPanel.Converters
{
    public class BoolToPasswordCharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? string.Empty : parameter?.ToString() ?? "●";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(BoolToPasswordCharConverter)} does not support two-way binding.");
        }
    }
}
