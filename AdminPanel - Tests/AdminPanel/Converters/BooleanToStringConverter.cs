using Avalonia.Data.Converters;
using System;
using System.Globalization;

public class BooleanToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return boolValue.ToString().ToLower();
        return "false";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string strValue)
            return bool.TryParse(strValue, out bool result) && result;
        return false;
    }
}