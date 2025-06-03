using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AdminPanel.Converters
{
    public class BoolToYesNoConverter : IValueConverter
    {
        private const string Yes = "Tak";
        private const string No = "Nie";

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new ArgumentException($"Target type must be {nameof(String)}.", nameof(targetType));
            }

            return value is bool boolValue ? boolValue ? Yes : No : No;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new ArgumentException($"Target type must be {nameof(Boolean)}.", nameof(targetType));
            }

            return value is string strValue && strValue == Yes;
        }
    }
}