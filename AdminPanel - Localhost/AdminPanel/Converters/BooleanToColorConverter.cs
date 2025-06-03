using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AdminPanel.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        private static readonly Color TrueColor = Colors.Green;
        private static readonly Color FalseColor = Colors.Red;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (targetType != typeof(IBrush))
            {
                throw new ArgumentException($"Target type must be {nameof(IBrush)}.", nameof(targetType));
            }

            if (value is bool isTrue)
            {
                return new SolidColorBrush(isTrue ? TrueColor : FalseColor);
            }

            return new SolidColorBrush(FalseColor);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(BooleanToColorConverter)} does not support two-way binding.");
        }
    }
}