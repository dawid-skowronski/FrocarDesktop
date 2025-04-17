using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AdminPanel.Converters
{
    public class ComboBoxItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return new ComboBoxItem { Content = stringValue };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ComboBoxItem comboBoxItem && comboBoxItem.Content is string stringValue)
            {
                return stringValue;
            }
            return null;
        }
    }
}