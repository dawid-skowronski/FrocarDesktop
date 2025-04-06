using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using AdminPanel.Models;
using AdminPanel.Services;
using Avalonia.Controls;
using System;
using System.Linq;
using AdminPanel.Views;
using Avalonia.Styling;

namespace AdminPanel.ViewModels
{
    public class CreateCarViewModel : ViewModelBase
    {
        public string Brand { get; set; } = "";
        public string EngineCapacity { get; set; } = "";
        private object _fuelTypeItem;
        public object FuelTypeItem
        {
            get => _fuelTypeItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _fuelTypeItem, value);
                FuelType = (value as ComboBoxItem)?.Content?.ToString();
            }
        }
        public string FuelType { get; set; }
        public string Seats { get; set; } = "";
        private object _carTypeItem;
        public object CarTypeItem
        {
            get => _carTypeItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _carTypeItem, value);
                CarType = (value as ComboBoxItem)?.Content?.ToString();
            }
        }
        public string CarType { get; set; }
        public string FeaturesText { get; set; } = "";
        public string Latitude { get; set; } = "";
        public string Longitude { get; set; } = "";
        public string Address { get; set; } = "";
        public string UserId { get; set; } = "1";
        public string RentalPricePerDay { get; set; } = "";

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
                !int.TryParse(UserId, out int userId) ||
                !double.TryParse(RentalPricePerDay, out double rentalPricePerDay) ||
                string.IsNullOrWhiteSpace(Brand) ||
                string.IsNullOrWhiteSpace(FuelType) ||
                string.IsNullOrWhiteSpace(CarType) ||
                string.IsNullOrWhiteSpace(RentalPricePerDay))
            {
                ErrorMessage = "Wszystkie pola są wymagane i muszą mieć poprawny format.";
                return;
            }

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
                Features = featuresList,
                Latitude = latitude,
                Longitude = longitude,
                UserId = userId,
                IsAvailable = true,
                RentalPricePerDay = rentalPricePerDay
            };

            var result = await ApiService.CreateCar(car);
            if (result.IsSuccess)
            {
                await ShowMessageBox("Sukces", "Pojazd został dodany!");

                Brand = "";
                EngineCapacity = "";
                FuelTypeItem = null;
                FuelType = null;
                Seats = "";
                CarTypeItem = null;
                CarType = null;
                FeaturesText = "";
                Latitude = "";
                Longitude = "";
                Address = "";
                UserId = "1";
                RentalPricePerDay = "";

                this.RaisePropertyChanged(nameof(Brand));
                this.RaisePropertyChanged(nameof(EngineCapacity));
                this.RaisePropertyChanged(nameof(FuelTypeItem));
                this.RaisePropertyChanged(nameof(Seats));
                this.RaisePropertyChanged(nameof(CarTypeItem));
                this.RaisePropertyChanged(nameof(FeaturesText));
                this.RaisePropertyChanged(nameof(Latitude));
                this.RaisePropertyChanged(nameof(Longitude));
                this.RaisePropertyChanged(nameof(Address));
                this.RaisePropertyChanged(nameof(UserId));
                this.RaisePropertyChanged(nameof(RentalPricePerDay));
            }
            else
            {
                ErrorMessage = "Wystąpił błąd podczas dodawania pojazdu.";
            }
        }

        private async Task ShowMessageBox(string title, string message)
        {
            var dialog = new MessageBoxView(title)
            {
                Message = message,
                RequestedThemeVariant = App.MainWindow?.RequestedThemeVariant ?? ThemeVariant.Default
            };
            await dialog.ShowDialog(App.MainWindow);
        }
    }
}