using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AdminPanel.Converters
{
    public class BooleanToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Tak" : "Nie";
            }
            return "Nie"; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == AvaloniaProperty.UnsetValue)
            {
                return false;
            }
            if (value is string strValue)
            {
                return strValue == "Tak";
            }
            return false; 
        }
    }
}