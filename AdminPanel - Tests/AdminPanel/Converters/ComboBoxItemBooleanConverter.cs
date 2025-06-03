using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminPanel.Converters
{
    public class ComboBoxItemBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return new ComboBoxItem { Content = boolValue ? "Tak" : "Nie" };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ComboBoxItem comboBoxItem && comboBoxItem.Content is string stringValue)
            {
                return stringValue == "Tak";
            }
            return false;
        }
    }
}
