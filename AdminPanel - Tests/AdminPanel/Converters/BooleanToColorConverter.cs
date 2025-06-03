using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AdminPanel.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTrue)
            {
                return isTrue ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}