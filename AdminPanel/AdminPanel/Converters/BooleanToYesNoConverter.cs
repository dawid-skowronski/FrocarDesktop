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
            return "Nie"; // Domyślna wartość, gdy value nie jest typu bool
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == AvaloniaProperty.UnsetValue)
            {
                return false; // Domyślna wartość dla unset
            }
            if (value is string strValue)
            {
                return strValue == "Tak";
            }
            return false; // Domyślna wartość, gdy value nie jest stringiem
        }
    }
}