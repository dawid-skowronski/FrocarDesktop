using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AdminPanel.Converters
{
    public class BoolToToggleIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "🙈" : "🙊";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(BoolToToggleIconConverter)} does not support two-way binding.");
        }
    }
}
