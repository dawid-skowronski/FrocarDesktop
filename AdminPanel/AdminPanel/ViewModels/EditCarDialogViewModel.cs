using AdminPanel.Models;
using AdminPanel.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;
using Avalonia.Controls;

namespace AdminPanel.ViewModels
{
    public class EditCarDialogViewModel : ViewModelBase
    {
        private CarListing _carListing;
        private string _featuresString;
        private string _errorMessage = string.Empty;
        private string _address = string.Empty;
        private string _latitude;
        private string _longitude;
        private string _title;
        private readonly Window _dialog;

        private string _engineCapacity;
        private string _seats;
        private string _rentalPricePerDay;

        public CarListing CarListing
        {
            get => _carListing;
            set => this.RaiseAndSetIfChanged(ref _carListing, value);
        }

        public string FeaturesString
        {
            get => _featuresString;
            set => this.RaiseAndSetIfChanged(ref _featuresString, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public string Address
        {
            get => _address;
            set => this.RaiseAndSetIfChanged(ref _address, value);
        }

        public string Latitude
        {
            get => _latitude;
            set => this.RaiseAndSetIfChanged(ref _latitude, value);
        }

        public string Longitude
        {
            get => _longitude;
            set => this.RaiseAndSetIfChanged(ref _longitude, value);
        }

        public string EngineCapacity
        {
            get => _engineCapacity;
            set => this.RaiseAndSetIfChanged(ref _engineCapacity, value);
        }

        public string Seats
        {
            get => _seats;
            set => this.RaiseAndSetIfChanged(ref _seats, value);
        }

        public string RentalPricePerDay
        {
            get => _rentalPricePerDay;
            set => this.RaiseAndSetIfChanged(ref _rentalPricePerDay, value);
        }

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string Message { get; }

        public List<string> FuelTypes { get; } = new List<string>
        {
            "Benzyna", "Diesel", "Benzyna + gaz", "Elektryczny", "Hybryda", "Etanol", "Wodór"
        };

        public List<string> CarTypes { get; } = new List<string>
        {
            "Sedan", "Kombi", "Kompakt", "Coupe", "Kabriolet", "SUV", "Minivan"
        };

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchAddressCommand { get; }

        public EditCarDialogViewModel(CarListing car, Window dialog)
        {
            _dialog = dialog;
            CarListing = new CarListing
            {
                Id = car.Id,
                Brand = car.Brand,
                EngineCapacity = car.EngineCapacity,
                FuelType = car.FuelType,
                Seats = car.Seats,
                CarType = car.CarType,
                Features = car.Features?.ToList(),
                Latitude = car.Latitude,
                Longitude = car.Longitude,
                UserId = car.UserId,
                IsAvailable = car.IsAvailable,
                IsApproved = car.IsApproved,
                RentalPricePerDay = car.RentalPricePerDay
            };

            EngineCapacity = car.EngineCapacity.ToString("N1", CultureInfo.CurrentCulture);
            Seats = car.Seats.ToString();
            RentalPricePerDay = car.RentalPricePerDay.ToString("N2", CultureInfo.CurrentCulture);
            Latitude = car.Latitude.ToString("N6", CultureInfo.CurrentCulture);
            Longitude = car.Longitude.ToString("N6", CultureInfo.CurrentCulture);
            FeaturesString = string.Join(", ", car.Features ?? new List<string>());

            _title = "Edycja pojazdu";
            FetchUsernameAndSetTitle(car.UserId, car.Brand, car.Id);

            Message = $"Edytuj dane pojazdu o ID {car.Id}";

            SaveCommand = ReactiveCommand.CreateFromTask(Save);
            CancelCommand = ReactiveCommand.Create(() => _dialog.Close());
            SearchAddressCommand = ReactiveCommand.CreateFromTask(SearchAddress);
        }

        private async void FetchUsernameAndSetTitle(int userId, string brand, int carId)
        {
            var (isSuccess, user, message) = await UserService.GetUserFromId(userId);
            if (isSuccess && user != null)
            {
                Title = $"{brand} o id {carId} użytkownika {user.Username}";
            }
            else
            {
                ErrorMessage = message;
                Title = $"{brand} o id {carId} - właściciel nieznany";
            }
        }

        private async Task SearchAddress()
        {
            if (string.IsNullOrWhiteSpace(Address))
            {
                ErrorMessage = "Wprowadź adres.";
                return;
            }

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "frocar (frocar@gmail.com)");

                var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(Address)}";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Błąd serwera: {response.StatusCode}";
                    return;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<List<NominatimResult>>(jsonResponse);

                if (results != null && results.Count > 0)
                {
                    var result = results[0];
                    if (double.TryParse(result.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat) &&
                        double.TryParse(result.Lon, NumberStyles.Float, CultureInfo.InvariantCulture, out double lon))
                    {
                        CarListing.Latitude = lat;
                        CarListing.Longitude = lon;

                        Latitude = lat.ToString("N6", CultureInfo.CurrentCulture);
                        Longitude = lon.ToString("N6", CultureInfo.CurrentCulture);

                        ErrorMessage = "";
                    }
                    else
                    {
                        ErrorMessage = "Nie udało się sparsować współrzędnych.";
                    }
                }
                else
                {
                    ErrorMessage = "Nie znaleziono adresu.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd: {ex.Message}";
            }
        }

        private async Task Save()
        {
            try
            {
                ErrorMessage = "";

                if (!double.TryParse(EngineCapacity, NumberStyles.Float, CultureInfo.CurrentCulture, out double engineCapacity))
                {
                    ErrorMessage = "Pojemność silnika musi być poprawną liczbą.";
                    return;
                }
                if (engineCapacity <= 0)
                {
                    ErrorMessage = "Pojemność silnika musi być większa od 0.";
                    return;
                }
                CarListing.EngineCapacity = engineCapacity;

                if (!int.TryParse(Seats, out int seats))
                {
                    ErrorMessage = "Liczba miejsc musi być poprawną liczbą całkowitą.";
                    return;
                }
                if (seats <= 0)
                {
                    ErrorMessage = "Liczba miejsc musi być większa od 0.";
                    return;
                }
                CarListing.Seats = seats;

                if (!double.TryParse(RentalPricePerDay, NumberStyles.Float, CultureInfo.CurrentCulture, out double rentalPrice))
                {
                    ErrorMessage = "Cena za dzień musi być poprawną liczbą.";
                    return;
                }
                if (rentalPrice <= 0)
                {
                    ErrorMessage = "Cena za dzień musi być większa od 0.";
                    return;
                }
                CarListing.RentalPricePerDay = rentalPrice;

                if (!double.TryParse(Latitude, NumberStyles.Float, CultureInfo.CurrentCulture, out double lat))
                {
                    ErrorMessage = "Szerokość geograficzna musi być poprawną liczbą.";
                    return;
                }
                CarListing.Latitude = lat;

                if (!double.TryParse(Longitude, NumberStyles.Float, CultureInfo.CurrentCulture, out double lon))
                {
                    ErrorMessage = "Długość geograficzna musi być poprawną liczbą.";
                    return;
                }
                CarListing.Longitude = lon;

                CarListing.Features = string.IsNullOrWhiteSpace(FeaturesString)
                    ? new List<string>()
                    : FeaturesString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim())
                        .Where(f => !string.IsNullOrEmpty(f))
                        .ToList();

                var (isSuccess, message) = await CarService.UpdateCarListing(CarListing);
                if (isSuccess)
                {
                    _dialog.Close();
                }
                else
                {
                    ErrorMessage = message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas zapisywania: {ex.Message}";
            }
        }
    }
}