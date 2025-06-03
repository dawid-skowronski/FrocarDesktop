using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Xunit;

namespace AdminPanel.Tests
{
    public class AppTests
    {
        [Fact]
        public void App_Initializes_Without_Errors()
        {
            // Arranging: Create an instance of the App
            var app = new App();

            // Acting: Initialize the app with Avalonia markup
            app.Initialize();

            // Asserting: Verify the app instance is not null
            Assert.NotNull(app);
        }

        [Fact]
        public void App_Contains_ViewLocator_DataTemplate()
        {
            // Arranging: Create and initialize the app
            var app = new App();
            app.Initialize();

            // Acting: Check for ViewLocator in DataTemplates
            var dataTemplates = app.DataTemplates;

            // Asserting: Verify ViewLocator is present
            Assert.NotEmpty(dataTemplates);
            Assert.Contains(dataTemplates, dt => dt is ViewLocator);
        }

        [Fact]
        public void App_Resources_Contain_Light_Theme()
        {
            // Arranging: Create and initialize the app
            var app = new App();
            app.Initialize();

            // Acting: Access theme dictionaries
            var resources = app.Resources;
            var themeDictionaries = (ResourceDictionary)resources["ThemeDictionaries"];
            var lightTheme = (ResourceDictionary)themeDictionaries["Light"];

            // Asserting: Verify light theme and key brushes
            Assert.NotNull(lightTheme);
            Assert.Equal(Color.Parse("#F8F9FA"), ((SolidColorBrush)lightTheme["ThemeBackgroundBrush"]).Color);
            Assert.Equal(Color.Parse("#2E7D32"), ((SolidColorBrush)lightTheme["ThemeAccentBrush"]).Color);
            Assert.Equal(Color.Parse("#D32F2F"), ((SolidColorBrush)lightTheme["ErrorForeground"]).Color);
        }

        [Fact]
        public void App_Resources_Contain_Dark_Theme()
        {
            // Arranging: Create and initialize the app
            var app = new App();
            app.Initialize();

            // Acting: Access theme dictionaries
            var resources = app.Resources;
            var themeDictionaries = (ResourceDictionary)resources["ThemeDictionaries"];
            var darkTheme = (ResourceDictionary)themeDictionaries["Dark"];

            // Asserting: Verify dark theme and key brushes
            Assert.NotNull(darkTheme);
            Assert.Equal(Color.Parse("#1E1E1E"), ((SolidColorBrush)darkTheme["ThemeBackgroundBrush"]).Color);
            Assert.Equal(Color.Parse("#2E7D32"), ((SolidColorBrush)darkTheme["ThemeAccentBrush"]).Color);
            Assert.Equal(Color.Parse("#121212"), ((SolidColorBrush)darkTheme["Czarny"]).Color);
        }

        [Fact]
        public void App_Resources_Contain_Converters()
        {
            // Arranging: Create and initialize the app
            var app = new App();
            app.Initialize();

            // Acting: Access resources
            var resources = app.Resources;

            // Asserting: Verify converters are present
            Assert.NotNull(resources["BooleanToYesNoConverter"]);
            Assert.NotNull(resources["BooleanToColorConverter"]);
            Assert.NotNull(resources["DecimalToVisibilityConverter"]);
            Assert.NotNull(resources["StringToVisibilityConverter"]);
        }

        [Fact]
        public void App_Styles_Contain_FluentTheme()
        {
            // Arranging: Create and initialize the app
            var app = new App();
            app.Initialize();

            // Acting: Check styles for FluentTheme
            var styles = app.Styles;

            // Asserting: Verify FluentTheme is present
            Assert.Contains(styles, style => style is FluentTheme);
        }
    }
}