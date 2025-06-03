using AdminPanel.Converters;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using ExCSS;
using System;
using System.Globalization;
using Xunit;

namespace AdminPanel.Tests
{
    public class ConverterTests
    {
        [Fact]
        public void BooleanToColorConverter_Convert_ReturnsGreenForTrue()
        {
            // Arrange
            var converter = new BooleanToColorConverter();
            var input = true;

            // Act
            var result = converter.Convert(input, typeof(SolidColorBrush), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(Avalonia.Media.Colors.Green, ((SolidColorBrush)result).Color);
        }

        [Fact]
        public void BooleanToColorConverter_Convert_ReturnsRedForFalse()
        {
            // Arrange
            var converter = new BooleanToColorConverter();
            var input = false;

            // Act
            var result = converter.Convert(input, typeof(SolidColorBrush), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(Avalonia.Media.Colors.Red, ((SolidColorBrush)result).Color);
        }

        [Fact]
        public void BooleanToColorConverter_Convert_ReturnsRedForInvalidInput()
        {
            // Arrange
            var converter = new BooleanToColorConverter();
            var input = "invalid";

            // Act
            var result = converter.Convert(input, typeof(SolidColorBrush), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(Avalonia.Media.Colors.Red, ((SolidColorBrush)result).Color);
        }

        [Fact]
        public void ComboBoxItemConverter_Convert_ReturnsComboBoxItemForString()
        {
            // Arrange
            var converter = new ComboBoxItemConverter();
            var input = "TestItem";

            // Act
            var result = converter.Convert(input, typeof(ComboBoxItem), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.IsType<ComboBoxItem>(result);
            Assert.Equal("TestItem", ((ComboBoxItem)result).Content);
        }

        [Fact]
        public void ComboBoxItemConverter_Convert_ReturnsNullForNonString()
        {
            // Arrange
            var converter = new ComboBoxItemConverter();
            var input = 123;

            // Act
            var result = converter.Convert(input, typeof(ComboBoxItem), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ComboBoxItemConverter_ConvertBack_ReturnsStringFromComboBoxItem()
        {
            // Arrange
            var converter = new ComboBoxItemConverter();
            var input = new ComboBoxItem { Content = "TestItem" };

            // Act
            var result = converter.ConvertBack(input, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("TestItem", result);
        }

        [Fact]
        public void ComboBoxItemConverter_ConvertBack_ReturnsNullForInvalidInput()
        {
            // Arrange
            var converter = new ComboBoxItemConverter();
            var input = 123;

            // Act
            var result = converter.ConvertBack(input, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DecimalToVisibilityConverter_Convert_ReturnsVisibleForPositive()
        {
            // Arrange
            var converter = new DecimalToVisibilityConverter();
            var input = 10.5m;

            // Act
            var result = converter.Convert(input, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void DecimalToVisibilityConverter_Convert_ReturnsHiddenForZeroOrNegative()
        {
            // Arrange
            var converter = new DecimalToVisibilityConverter();

            // Act
            var resultZero = converter.Convert(0m, typeof(Visibility), null, CultureInfo.InvariantCulture);
            var resultNegative = converter.Convert(-5.2m, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal(Visibility.Hidden, resultZero);
            Assert.Equal(Visibility.Hidden, resultNegative);
        }

        [Fact]
        public void DecimalToVisibilityConverter_Convert_ReturnsHiddenForInvalidInput()
        {
            // Arrange
            var converter = new DecimalToVisibilityConverter();
            var input = "invalid";

            // Act
            var result = converter.Convert(input, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal(Visibility.Hidden, result);
        }

        [Fact]
        public void BooleanToYesNoConverter_Convert_ReturnsTakForTrue()
        {
            // Arrange
            var converter = new BooleanToYesNoConverter();
            var input = true;

            // Act
            var result = converter.Convert(input, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("Tak", result);
        }

        [Fact]
        public void BooleanToYesNoConverter_Convert_ReturnsNieForFalse()
        {
            // Arrange
            var converter = new BooleanToYesNoConverter();
            var input = false;

            // Act
            var result = converter.Convert(input, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("Nie", result);
        }

        [Fact]
        public void BooleanToYesNoConverter_Convert_ReturnsNieForInvalidInput()
        {
            // Arrange
            var converter = new BooleanToYesNoConverter();
            var input = "invalid";

            // Act
            var result = converter.Convert(input, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("Nie", result);
        }

        [Fact]
        public void BooleanToYesNoConverter_ConvertBack_ReturnsTrueForTak()
        {
            // Arrange
            var converter = new BooleanToYesNoConverter();
            var input = "Tak";

            // Act
            var result = converter.ConvertBack(input, typeof(bool), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.True((bool)result);
        }

        [Fact]
        public void BooleanToYesNoConverter_ConvertBack_ReturnsFalseForNieOrInvalid()
        {
            // Arrange
            var converter = new BooleanToYesNoConverter();

            // Act
            var resultNie = converter.ConvertBack("Nie", typeof(bool), null, CultureInfo.InvariantCulture);
            var resultInvalid = converter.ConvertBack("invalid", typeof(bool), null, CultureInfo.InvariantCulture);
            var resultUnset = converter.ConvertBack(Avalonia.AvaloniaProperty.UnsetValue, typeof(bool), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.False((bool)resultNie);
            Assert.False((bool)resultInvalid);
            Assert.False((bool)resultUnset);
        }

        [Fact]
        public void BooleanToStringConverter_Convert_ReturnsLowercaseString()
        {
            // Arrange
            var converter = new BooleanToStringConverter();

            // Act
            var resultTrue = converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);
            var resultFalse = converter.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("true", resultTrue);
            Assert.Equal("false", resultFalse);
        }

        [Fact]
        public void BooleanToStringConverter_Convert_ReturnsFalseForInvalidInput()
        {
            // Arrange
            var converter = new BooleanToStringConverter();
            var input = "invalid";

            // Act
            var result = converter.Convert(input, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("false", result);
        }

        [Fact]
        public void BooleanToStringConverter_ConvertBack_ReturnsBoolFromString()
        {
            // Arrange
            var converter = new BooleanToStringConverter();

            // Act
            var resultTrue = converter.ConvertBack("true", typeof(bool), null, CultureInfo.InvariantCulture);
            var resultFalse = converter.ConvertBack("false", typeof(bool), null, CultureInfo.InvariantCulture);
            var resultInvalid = converter.ConvertBack("invalid", typeof(bool), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.True((bool)resultTrue);
            Assert.False((bool)resultFalse);
            Assert.False((bool)resultInvalid);
        }

        [Fact]
        public void StringToVisibilityConverter_Convert_ReturnsVisibleForNonEmptyString()
        {
            // Arrange
            var converter = new StringToVisibilityConverter();
            var input = "Test";

            // Act
            var result = converter.Convert(input, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void StringToVisibilityConverter_Convert_ReturnsHiddenForEmptyOrNull()
        {
            // Arrange
            var converter = new StringToVisibilityConverter();

            // Act
            var resultNull = converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);
            var resultEmpty = converter.Convert("", typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal(Visibility.Hidden, resultNull);
            Assert.Equal(Visibility.Hidden, resultEmpty);
        }

        [Fact]
        public void ComboBoxItemBooleanConverter_Convert_ReturnsComboBoxItemForBool()
        {
            // Arrange
            var converter = new ComboBoxItemBooleanConverter();

            // Act
            var resultTrue = converter.Convert(true, typeof(ComboBoxItem), null, CultureInfo.InvariantCulture);
            var resultFalse = converter.Convert(false, typeof(ComboBoxItem), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.IsType<ComboBoxItem>(resultTrue);
            Assert.Equal("Tak", ((ComboBoxItem)resultTrue).Content);
            Assert.IsType<ComboBoxItem>(resultFalse);
            Assert.Equal("Nie", ((ComboBoxItem)resultFalse).Content);
        }

        [Fact]
        public void ComboBoxItemBooleanConverter_Convert_ReturnsNullForInvalidInput()
        {
            // Arrange
            var converter = new ComboBoxItemBooleanConverter();
            var input = "invalid";

            // Act
            var result = converter.Convert(input, typeof(ComboBoxItem), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ComboBoxItemBooleanConverter_ConvertBack_ReturnsBoolFromComboBoxItem()
        {
            // Arrange
            var converter = new ComboBoxItemBooleanConverter();
            var inputTrue = new ComboBoxItem { Content = "Tak" };
            var inputFalse = new ComboBoxItem { Content = "Nie" };

            // Act
            var resultTrue = converter.ConvertBack(inputTrue, typeof(bool), null, CultureInfo.InvariantCulture);
            var resultFalse = converter.ConvertBack(inputFalse, typeof(bool), null, CultureInfo.InvariantCulture);
            var resultInvalid = converter.ConvertBack(123, typeof(bool), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.True((bool)resultTrue);
            Assert.False((bool)resultFalse);
            Assert.False((bool)resultInvalid);
        }
    }
}