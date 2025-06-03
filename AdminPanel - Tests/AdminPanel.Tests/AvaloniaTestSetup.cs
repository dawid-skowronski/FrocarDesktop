using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Headless;
using System;

namespace AdminPanel.Tests
{
    public class AvaloniaTestSetup
    {
        private static bool _isInitialized;
        private static readonly object _lock = new();

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<AdminPanel.App>()
                .UsePlatformDetect()
                .WithInterFont()
                .UseReactiveUI()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions());
        }

        public static void Initialize()
        {
            lock (_lock)
            {
                if (!_isInitialized)
                {
                    try
                    {
                        BuildAvaloniaApp().SetupWithoutStarting();
                        _isInitialized = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to initialize Avalonia: {ex.Message}");
                        throw;
                    }
                }
            }
        }
    }
}