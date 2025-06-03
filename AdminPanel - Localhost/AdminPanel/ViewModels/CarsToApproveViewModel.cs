using AdminPanel.Models;
using AdminPanel.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Styling;
using System.Globalization;
using AdminPanel.Views;

namespace AdminPanel.ViewModels
{
    public class CarsToApproveViewModel : ViewModelBase
    {
        private ObservableCollection<CarListing> _cars = new();
        private ObservableCollection<CarListing> _filteredCars = new();

        public ObservableCollection<CarListing> Cars
        {
            get => _cars;
            set => this.RaiseAndSetIfChanged(ref _cars, value);
        }

        public ObservableCollection<CarListing> FilteredCars
        {
            get => _filteredCars;
            set => this.RaiseAndSetIfChanged(ref _filteredCars, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public string IdFilter { get => _idFilter; set => this.RaiseAndSetIfChanged(ref _idFilter, value); }
        private string _idFilter = "";

        public string UserIdFilter { get => _userIdFilter; set => this.RaiseAndSetIfChanged(ref _userIdFilter, value); }
        private string _userIdFilter = "";

        public string BrandFilter { get => _brandFilter; set => this.RaiseAndSetIfChanged(ref _brandFilter, value); }
        private string _brandFilter = "";

        public string MinEngineCapacity { get => _minEngineCapacity; set => this.RaiseAndSetIfChanged(ref _minEngineCapacity, value); }
        private string _minEngineCapacity = "";

        public string MaxEngineCapacity { get => _maxEngineCapacity; set => this.RaiseAndSetIfChanged(ref _maxEngineCapacity, value); }
        private string _maxEngineCapacity = "";

        public object FuelTypeFilter { get => _fuelTypeFilter; set => this.RaiseAndSetIfChanged(ref _fuelTypeFilter, value); }
        private object _fuelTypeFilter;

        public object CarTypeFilter { get => _carTypeFilter; set => this.RaiseAndSetIfChanged(ref _carTypeFilter, value); }
        private object _carTypeFilter;

        public string MinSeatsFilter { get => _minSeatsFilter; set => this.RaiseAndSetIfChanged(ref _minSeatsFilter, value); }
        private string _minSeatsFilter = "";

        public string MinPrice { get => _minPrice; set => this.RaiseAndSetIfChanged(ref _minPrice, value); }
        private string _minPrice = "";

        public string MaxPrice { get => _maxPrice; set => this.RaiseAndSetIfChanged(ref _maxPrice, value); }
        private string _maxPrice = "";

        public string Address { get => _address; set => this.RaiseAndSetIfChanged(ref _address, value); }
        private string _address = "";

        public string MaxRange { get => _maxRange; set => this.RaiseAndSetIfChanged(ref _maxRange, value); }
        private string _maxRange = "";

        public object IsAvailableFilter { get => _isAvailableFilter; set => this.RaiseAndSetIfChanged(ref _isAvailableFilter, value); }
        private object _isAvailableFilter;

        public double? Latitude { get => _latitude; set => this.RaiseAndSetIfChanged(ref _latitude, value); }
        private double? _latitude;

        public double? Longitude { get => _longitude; set => this.RaiseAndSetIfChanged(ref _longitude, value); }
        private double? _longitude;

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public ReactiveCommand<Unit, Unit> FilterCarsCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetFiltersCommand { get; }

        public CarsToApproveViewModel()
        {
            RefreshCommand = ReactiveCommand.CreateFromTask(LoadCars);
            FilterCarsCommand = ReactiveCommand.Create(FilterCars);
            ResetFiltersCommand = ReactiveCommand.Create(ResetFilters);

            FuelTypeFilter = new ComboBoxItem { Content = "Wszystkie rodzaje paliwa" };
            CarTypeFilter = new ComboBoxItem { Content = "Wszystkie typy nadwozia" };
            IsAvailableFilter = new ComboBoxItem { Content = "Wszystkie" };

            _ = LoadCars();
        }

        private async Task LoadCars()
        {
            try
            {
                var cars = await CarService.GetCarListings();
                Cars.Clear();
                FilteredCars.Clear();
                foreach (var car in cars.Where(c => !c.IsApproved))
                {
                    var (isSuccess, user, message) = await UserService.GetUserFromId(car.UserId);
                    car.Username = isSuccess && user != null ? user.Username : "Nieznany";
                    car.ApproveCommand = ReactiveCommand.CreateFromTask<int>(ApproveCar);
                    car.DeleteCommand = ReactiveCommand.CreateFromTask<int>(ConfirmDeleteCar);
                    Cars.Add(car);
                    FilteredCars.Add(car);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas ładowania listy pojazdów: {ex.Message}";
                await ShowMessageBox("Błąd", ErrorMessage);
            }
        }

        private async Task ApproveCar(int carId)
        {
            try
            {
                var (isSuccess, errorMessage) = await CarService.ApproveCarListing(carId);
                if (isSuccess)
                {
                    var carToRemove = Cars.FirstOrDefault(c => c.Id == carId);
                    if (carToRemove != null)
                    {
                        Cars.Remove(carToRemove);
                        FilteredCars.Remove(carToRemove);
                        await ShowMessageBox("Sukces", errorMessage ?? "Pojazd został zatwierdzony!");
                    }
                    else
                    {
                        await ShowMessageBox("Błąd", "Nie znaleziono pojazdu do zatwierdzenia.");
                    }
                }
                else
                {
                    ErrorMessage = errorMessage ?? "Nie udało się zatwierdzić pojazdu.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas zatwierdzania pojazdu: {ex.Message}";
                await ShowMessageBox("Błąd", ErrorMessage);
            }
        }

        private async Task ConfirmDeleteCar(int carId)
        {
            try
            {
                bool confirmed = await ShowConfirmDeleteDialog(carId);
                if (confirmed)
                {
                    await DeleteCar(carId);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas usuwania pojazdu: {ex.Message}";
                await ShowMessageBox("Błąd", ErrorMessage);
            }
        }

        private async Task DeleteCar(int carId)
        {
            try
            {
                var (isSuccess, errorMessage) = await CarService.DeleteCarListing(carId);
                if (isSuccess)
                {
                    var carToRemove = Cars.FirstOrDefault(c => c.Id == carId);
                    if (carToRemove != null)
                    {
                        Cars.Remove(carToRemove);
                        FilteredCars.Remove(carToRemove);
                        await ShowMessageBox("Sukces", "Pojazd został usunięty!");
                    }
                    else
                    {
                        await ShowMessageBox("Błąd", "Nie znaleziono pojazdu do usunięcia.");
                    }
                }
                else
                {
                    ErrorMessage = errorMessage ?? "Nie udało się usunąć pojazdu.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas usuwania pojazdu: {ex.Message}";
                await ShowMessageBox("Błąd", ErrorMessage);
            }
        }

        private async void FilterCars()
        {
            if (Cars == null || !Cars.Any()) return;

            if (!string.IsNullOrWhiteSpace(Address))
            {
                try
                {
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("User-Agent", "frocar (frocar@gmail.com)");

                    var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(Address)}";
                    var response = await client.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        ErrorMessage = $"Błąd serwera: {response.StatusCode}";
                        Latitude = null;
                        Longitude = null;
                        return;
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var results = JsonSerializer.Deserialize<List<NominatimResult>>(jsonResponse);

                    if (results != null && results.Count > 0)
                    {
                        var result = results[0];
                        if (double.TryParse(result.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lon) &&
                            double.TryParse(result.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
                        {
                            Latitude = lat;
                            Longitude = lon;
                            ErrorMessage = "";
                        }
                        else
                        {
                            ErrorMessage = "Nie udało się sparsować współrzędnych.";
                            Latitude = null;
                            Longitude = null;
                        }
                    }
                    else
                    {
                        ErrorMessage = "Nie znaleziono adresu.";
                        Latitude = null;
                        Longitude = null;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Błąd: {ex.Message}";
                    Latitude = null;
                    Longitude = null;
                    return;
                }
            }

            string fuelType = FuelTypeFilter is ComboBoxItem fuelItem ? fuelItem.Content?.ToString() : null;
            string carType = CarTypeFilter is ComboBoxItem carItem ? carItem.Content?.ToString() : null;
            string isAvailable = IsAvailableFilter is ComboBoxItem availableItem ? availableItem.Content?.ToString() : null;

            var filtered = Cars.Where(car =>
            {
                bool matches = true;

                if (!string.IsNullOrEmpty(IdFilter) && int.TryParse(IdFilter, out int id))
                    matches &= car.Id == id;

                if (!string.IsNullOrEmpty(UserIdFilter) && int.TryParse(UserIdFilter, out int userId))
                    matches &= car.UserId == userId;

                if (!string.IsNullOrEmpty(BrandFilter))
                    matches &= car.Brand.Contains(BrandFilter, StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrEmpty(MinEngineCapacity) && double.TryParse(MinEngineCapacity, out double minEngine))
                    matches &= car.EngineCapacity >= minEngine;
                if (!string.IsNullOrEmpty(MaxEngineCapacity) && double.TryParse(MaxEngineCapacity, out double maxEngine))
                    matches &= car.EngineCapacity <= maxEngine;

                if (!string.IsNullOrEmpty(fuelType) && fuelType != "Wszystkie rodzaje paliwa")
                    matches &= car.FuelType != null && car.FuelType.Equals(fuelType, StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrEmpty(carType) && carType != "Wszystkie typy nadwozia")
                    matches &= car.CarType != null && car.CarType.Equals(carType, StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrEmpty(MinSeatsFilter) && int.TryParse(MinSeatsFilter, out int minSeats))
                    matches &= car.Seats >= minSeats;

                if (!string.IsNullOrEmpty(MinPrice) && double.TryParse(MinPrice, out double minPrice))
                    matches &= car.RentalPricePerDay >= minPrice;
                if (!string.IsNullOrEmpty(MaxPrice) && double.TryParse(MaxPrice, out double maxPrice))
                    matches &= car.RentalPricePerDay <= maxPrice;

                if (Latitude.HasValue && Longitude.HasValue && !string.IsNullOrEmpty(MaxRange) && double.TryParse(MaxRange, out double maxRange))
                {
                    var distance = CalculateDistance(Latitude.Value, Longitude.Value, car.Latitude, car.Longitude);
                    matches &= distance <= maxRange;
                }

                if (!string.IsNullOrEmpty(isAvailable) && isAvailable != "Wszystkie")
                    matches &= car.IsAvailable == (isAvailable == "Tak");

                return matches;
            }).ToList();

            FilteredCars.Clear();
            foreach (var car in filtered)
            {
                FilteredCars.Add(car);
            }
        }

        private void ResetFilters()
        {
            IdFilter = "";
            UserIdFilter = "";
            BrandFilter = "";
            MinEngineCapacity = "";
            MaxEngineCapacity = "";
            FuelTypeFilter = new ComboBoxItem { Content = "Wszystkie rodzaje paliwa" };
            CarTypeFilter = new ComboBoxItem { Content = "Wszystkie typy nadwozia" };
            MinSeatsFilter = "";
            MinPrice = "";
            MaxPrice = "";
            Address = "";
            MaxRange = "";
            IsAvailableFilter = new ComboBoxItem { Content = "Wszystkie" };
            Latitude = null;
            Longitude = null;
            ErrorMessage = "";
            FilteredCars.Clear();
            foreach (var car in Cars)
            {
                FilteredCars.Add(car);
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        private async Task<bool> ShowConfirmDeleteDialog(int carId)
        {
            var dialog = new ConfirmMessageBoxView("Potwierdzenie usunięcia")
            {
                Message = $"Czy na pewno chcesz usunąć pojazd o ID {carId}?",
                RequestedThemeVariant = App.MainWindow?.RequestedThemeVariant ?? ThemeVariant.Default
            };
            await dialog.ShowDialog(App.MainWindow);
            return await dialog.Result.Task;
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