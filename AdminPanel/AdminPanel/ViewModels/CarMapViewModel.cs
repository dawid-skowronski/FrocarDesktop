using ReactiveUI;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdminPanel.Models;
using AdminPanel.Services;
using System;
using System.Linq;
using System.Reactive;
using System.Net.Http;
using System.Text.Json;
using System.Globalization;
using Avalonia.Controls;
using System.Text.Json.Serialization;

namespace AdminPanel.ViewModels
{
    public class CarMapViewModel : ViewModelBase
    {
        private List<CarListing> _cars;
        private List<CarListing> _filteredCars;

        public List<CarListing> Cars
        {
            get => _cars;
            set => this.RaiseAndSetIfChanged(ref _cars, value);
        }

        public List<CarListing> FilteredCars
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

        private string _selectedCoordinates = "";
        public string SelectedCoordinates
        {
            get => _selectedCoordinates;
            set => this.RaiseAndSetIfChanged(ref _selectedCoordinates, value);
        }

        public string BrandFilter { get => _brandFilter; set => this.RaiseAndSetIfChanged(ref _brandFilter, value); }
        private string _brandFilter = "";

        public string MinEngineCapacity { get => _minEngineCapacity; set => this.RaiseAndSetIfChanged(ref _minEngineCapacity, value); }
        private string _minEngineCapacity;

        public string MaxEngineCapacity { get => _maxEngineCapacity; set => this.RaiseAndSetIfChanged(ref _maxEngineCapacity, value); }
        private string _maxEngineCapacity;

        public object FuelTypeFilter { get => _fuelTypeFilter; set => this.RaiseAndSetIfChanged(ref _fuelTypeFilter, value); }
        private object _fuelTypeFilter;

        public object CarTypeFilter { get => _carTypeFilter; set => this.RaiseAndSetIfChanged(ref _carTypeFilter, value); }
        private object _carTypeFilter;

        public string MinSeatsFilter { get => _minSeatsFilter; set => this.RaiseAndSetIfChanged(ref _minSeatsFilter, value); }
        private string _minSeatsFilter;

        public string MinPrice { get => _minPrice; set => this.RaiseAndSetIfChanged(ref _minPrice, value); }
        private string _minPrice;

        public string MaxPrice { get => _maxPrice; set => this.RaiseAndSetIfChanged(ref _maxPrice, value); }
        private string _maxPrice;

        public string Address { get => _address; set => this.RaiseAndSetIfChanged(ref _address, value); }
        private string _address = "";

        public string MaxRange { get => _maxRange; set => this.RaiseAndSetIfChanged(ref _maxRange, value); }
        private string _maxRange = "";

        public bool ShowOnlyAvailable { get => _showOnlyAvailable; set => this.RaiseAndSetIfChanged(ref _showOnlyAvailable, value); }
        private bool _showOnlyAvailable = false;

        public double? Latitude { get => _latitude; set => this.RaiseAndSetIfChanged(ref _latitude, value); }
        private double? _latitude;

        public double? Longitude { get => _longitude; set => this.RaiseAndSetIfChanged(ref _longitude, value); }
        private double? _longitude;

        public ReactiveCommand<Unit, Unit> FilterCarsCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetFiltersCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

        public CarMapViewModel()
        {
            Cars = new List<CarListing>();
            FilteredCars = new List<CarListing>();

            FilterCarsCommand = ReactiveCommand.Create(FilterCars);
            ResetFiltersCommand = ReactiveCommand.Create(ResetFilters);
            RefreshCommand = ReactiveCommand.CreateFromTask(LoadCarsAsync);

            FuelTypeFilter = new ComboBoxItem { Content = "Wszystkie rodzaje paliwa" };
            CarTypeFilter = new ComboBoxItem { Content = "Wszystkie typy nadwozia" };
        }

        public async Task LoadCarsAsync()
        {
            ErrorMessage = string.Empty;
            try
            {
                var cars = await CarService.GetCarListings();
                if (cars != null)
                {
                    Cars = cars;
                    FilteredCars = cars;
                }
                else
                {
                    ErrorMessage = "Nie udało się pobrać listy pojazdów.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas ładowania pojazdów: {ex.Message}";
            }
        }

        public async void FilterCars()
        {
            if (Cars == null) return;

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
                        SelectedCoordinates = "";
                        Latitude = null;
                        Longitude = null;
                        return;
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var results = JsonSerializer.Deserialize<List<NominatimResult>>(jsonResponse);

                    if (results != null && results.Count > 0)
                    {
                        var result = results[0];
                        if (double.TryParse(result.Lon, NumberStyles.Float, CultureInfo.InvariantCulture, out double lon) &&
                            double.TryParse(result.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
                        {
                            Latitude = lat;
                            Longitude = lon;
                            SelectedCoordinates = $"Współrzędne adresu: Lat: {lat}, Lon: {lon}";
                            ErrorMessage = "";
                        }
                        else
                        {
                            ErrorMessage = "Nie udało się sparsować współrzędnych.";
                            SelectedCoordinates = "";
                            Latitude = null;
                            Longitude = null;
                        }
                    }
                    else
                    {
                        ErrorMessage = "Nie znaleziono adresu.";
                        SelectedCoordinates = "";
                        Latitude = null;
                        Longitude = null;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Błąd: {ex.Message}";
                    SelectedCoordinates = "";
                    Latitude = null;
                    Longitude = null;
                    return;
                }
            }

            string? fuelType = FuelTypeFilter is ComboBoxItem fuelItem ? fuelItem.Content?.ToString() : null;
            string? carType = CarTypeFilter is ComboBoxItem carItem ? carItem.Content?.ToString() : null;

            var filtered = Cars.Where(car =>
            {
                bool matches = true;

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
                    ErrorMessage = $"Odległość do {car.Brand}: {distance:F2} km";
                }

                if (ShowOnlyAvailable)
                    matches &= car.IsAvailable;

                return matches;
            }).ToList();

            FilteredCars = filtered;
        }

        public void ResetFilters()
        {
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
            ShowOnlyAvailable = false;
            Latitude = null;
            Longitude = null;
            SelectedCoordinates = "";
            ErrorMessage = "";
            FilteredCars = Cars?.ToList() ?? new List<CarListing>();
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

        public string GetCarInfo(CarListing car)
        {
            return $"Marka: {car.Brand}\n" +
                   $"Typ nadwozia: {car.CarType}\n" +
                   $"Pojemność silnika: {car.EngineCapacity}L\n" +
                   $"Rodzaj paliwa: {car.FuelType}\n" +
                   $"Liczba miejsc: {car.Seats}\n" +
                   $"Wyposażenie: {(car.Features != null && car.Features.Any() ? string.Join(", ", car.Features) : "Brak")}\n" +
                   $"Dostępność: {(car.IsAvailable ? "Dostępny" : "Niedostępny")}\n" +
                   $"Cena za dzień: {car.RentalPricePerDay} PLN";
        }
    }

    public class NominatimResult
    {
        [JsonPropertyName("lat")]
        public string Lat { get; set; }

        [JsonPropertyName("lon")]
        public string Lon { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
    }
}