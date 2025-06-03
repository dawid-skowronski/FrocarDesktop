using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AdminPanel.Views;
using AdminPanel.ViewModels;

namespace AdminPanel
{
    public class App : Application
    {
        public static MainWindow MainWindow { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow = new MainWindow
                {
                    Content = new HomePage
                    {
                        DataContext = new HomePageViewModel()
                    }
                };

                desktop.MainWindow = MainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
