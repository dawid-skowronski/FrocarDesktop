using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using AdminPanel.Models;
using AdminPanel.Services;
using System.Collections.Generic;
using Avalonia.Controls;
using System;
using System.Linq;
using System.Globalization; // Potrzebne do parsowania

namespace AdminPanel.ViewModels
{
    public class CreateCarViewModel : ViewModelBase
    {

        public string Brand { get; set; } = "";
        public string EngineCapacity { get; set; } = ""; // String, aby watermark działał
        public string FuelType { get; set; } = "";
        public string Seats { get; set; } = "";
        public string CarType { get; set; } = "";
        public string FeaturesText { get; set; } = ""; // Zwykłe pole tekstowe
        public string Latitude { get; set; } = "";  // String dla watermarka
        public string Longitude { get; set; } = ""; // String dla watermarka

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

            if (!double.TryParse(EngineCapacity, out double engineCapacity) ||
                !int.TryParse(Seats, out int seats) ||
                !double.TryParse(Latitude, out double latitude) ||
                !double.TryParse(Longitude, out double longitude) ||
                string.IsNullOrWhiteSpace(Brand) ||
                string.IsNullOrWhiteSpace(FuelType) ||
                string.IsNullOrWhiteSpace(CarType))
            {
                ErrorMessage = "Wszystkie pola są wymagane i muszą mieć poprawny format.";
                return;
            }

            // 🔥 KONWERSJA TEKSTU NA LISTĘ 🔥
            var featuresList = FeaturesText.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(f => f.Trim())
                                           .Where(f => !string.IsNullOrWhiteSpace(f))
                                           .ToList();

            var car = new CarListing
            {
                Brand = Brand,
                EngineCapacity = engineCapacity,
                FuelType = FuelType,
                Seats = seats,
                CarType = CarType,
                Features = featuresList, // 🚀 Teraz to jest poprawna lista!
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
                FuelType = "";
                Seats = "";
                CarType = "";
                Latitude = "";
                Longitude = "";
                FeaturesText = ""; // Resetujemy pole tekstowe

                this.RaisePropertyChanged(nameof(Brand));
                this.RaisePropertyChanged(nameof(EngineCapacity));
                this.RaisePropertyChanged(nameof(FuelType));
                this.RaisePropertyChanged(nameof(Seats));
                this.RaisePropertyChanged(nameof(CarType));
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
                Width = 300,
                Height = 150
            };

            var stackPanel = new StackPanel();

            var textBlock = new TextBlock
            {
                Text = message,
                Margin = new Avalonia.Thickness(10),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };

            var okButton = new Button
            {
                Content = "OK",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            okButton.Click += (_, _) => window.Close();

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(okButton);

            window.Content = stackPanel;

            await window.ShowDialog(App.MainWindow);
        }
    }
}
