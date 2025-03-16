using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using AdminPanel.Models;
using AdminPanel.Services;
using Avalonia.Controls;
using System;
using System.Linq;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Styling;

namespace AdminPanel.ViewModels
{
    public class CreateCarViewModel : ViewModelBase
    {
        public string Brand { get; set; } = "";
        public string EngineCapacity { get; set; } = "";
        private object _fuelTypeItem; // Nowa właściwość dla ComboBoxItem
        public object FuelTypeItem
        {
            get => _fuelTypeItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _fuelTypeItem, value);
                FuelType = (value as ComboBoxItem)?.Content?.ToString(); // Wyciągamy tekst
            }
        }
        public string FuelType { get; set; } // Przechowuje tekst wybranej opcji
        public string Seats { get; set; } = "";
        private object _carTypeItem; // Nowa właściwość dla ComboBoxItem
        public object CarTypeItem
        {
            get => _carTypeItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _carTypeItem, value);
                CarType = (value as ComboBoxItem)?.Content?.ToString(); // Wyciągamy tekst
            }
        }
        public string CarType { get; set; } // Przechowuje tekst wybranej opcji
        public string FeaturesText { get; set; } = "";
        public string Latitude { get; set; } = "";
        public string Longitude { get; set; } = "";
        public string Address { get; set; } = "";

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public ReactiveCommand<Unit, Unit> SubmitCommand { get; }

        public CreateCarViewModel()
        {
            SubmitCommand = ReactiveCommand.CreateFromTask(CreateCarAsync);
        }

        private async Task CreateCarAsync()
        {
            ErrorMessage = string.Empty;

            // Walidacja pól
            if (!double.TryParse(EngineCapacity, out double engineCapacity) ||
                !int.TryParse(Seats, out int seats) ||
                !double.TryParse(Latitude, out double latitude) ||
                !double.TryParse(Longitude, out double longitude) ||
                string.IsNullOrWhiteSpace(Brand) ||
                string.IsNullOrWhiteSpace(FuelType) || // Sprawdzamy tekst
                string.IsNullOrWhiteSpace(CarType))    // Sprawdzamy tekst
            {
                ErrorMessage = "Wszystkie pola są wymagane i muszą mieć poprawny format.";
                return;
            }

            // Konwersja tekstu na listę
            var featuresList = FeaturesText.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(f => f.Trim())
                                           .Where(f => !string.IsNullOrWhiteSpace(f))
                                           .ToList();

            var car = new CarListing
            {
                Brand = Brand,
                EngineCapacity = engineCapacity,
                FuelType = FuelType, // Używamy stringa
                Seats = seats,
                CarType = CarType,   // Używamy stringa
                Features = featuresList,
                Latitude = latitude,
                Longitude = longitude
            };

            var result = await ApiService.CreateCar(car);
            if (result.IsSuccess)
            {
                await ShowMessageBox("Sukces", "Pojazd został dodany!");

                // Resetowanie formularza
                Brand = "";
                EngineCapacity = "";
                FuelTypeItem = null; // Resetujemy ComboBoxItem
                FuelType = null;     // Resetujemy string
                Seats = "";
                CarTypeItem = null;  // Resetujemy ComboBoxItem
                CarType = null;      // Resetujemy string
                Latitude = "";
                Longitude = "";
                FeaturesText = "";

                this.RaisePropertyChanged(nameof(Brand));
                this.RaisePropertyChanged(nameof(EngineCapacity));
                this.RaisePropertyChanged(nameof(FuelTypeItem));
                this.RaisePropertyChanged(nameof(Seats));
                this.RaisePropertyChanged(nameof(CarTypeItem));
                this.RaisePropertyChanged(nameof(Latitude));
                this.RaisePropertyChanged(nameof(Longitude));
                this.RaisePropertyChanged(nameof(FeaturesText));
            }
            else
            {
                ErrorMessage = "Wystąpił błąd podczas dodawania pojazdu.";
            }
        }

        private async Task ShowMessageBox(string title, string message)
        {
            var window = new Window
            {
                Title = title,
                Width = 350,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                CanResize = false,
                ShowInTaskbar = false
            };

            var stackPanel = new StackPanel
            {
                Spacing = 15,
                Margin = new Thickness(20),
                VerticalAlignment = VerticalAlignment.Center
            };

            var textBlock = new TextBlock
            {
                Text = message,
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.Black),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            var okButton = new Button
            {
                Content = "OK",
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 100
            };
            okButton.Classes.Add("ok"); // Dodajemy klasę "ok"

            okButton.Click += (_, _) => window.Close();

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(okButton);

            window.Content = stackPanel;

            window.TransparencyLevelHint = new[] { WindowTransparencyLevel.AcrylicBlur };
            window.Background = new SolidColorBrush(Color.FromArgb(200, 240, 240, 240));

            await window.ShowDialog(App.MainWindow);
        }
    }
}