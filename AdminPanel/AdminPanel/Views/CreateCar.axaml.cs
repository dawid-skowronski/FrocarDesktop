using AdminPanel.ViewModels;
using Avalonia.Controls;
using Mapsui.UI.Avalonia;
using Mapsui.Tiling;
using Mapsui.Projections;
using System;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using System.Collections.Generic;
using Mapsui.Extensions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ReactiveUI;
using static SkiaSharp.HarfBuzz.SKShaper;
using System.Text.Json.Serialization;
using System.Globalization;

namespace AdminPanel.Views
{
    public partial class CreateCar : UserControl
    {
        private readonly MapControl _mapControl;
        private readonly CreateCarViewModel _viewModel;
        private MemoryLayer _markerLayer;

        public CreateCar()
        {
            InitializeComponent();
            _viewModel = new CreateCarViewModel();
            DataContext = _viewModel;

            _mapControl = this.FindControl<MapControl>("CarLocationMap");
            _mapControl.Map = CreateMap();
            _mapControl.Info += OnMapClick;

        }

        private Mapsui.Map CreateMap()
        {
            var map = new Mapsui.Map();
            map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

            _markerLayer = new MemoryLayer
            {
                Name = "MarkerLayer",
                Features = Array.Empty<IFeature>(), // Zmiana zgodnie z Mapsui 4.0+
                Style = new SymbolStyle
                {
                    SymbolType = SymbolType.Ellipse,
                    Fill = new Brush(Color.Red), // Kolor markera
                    Outline = new Pen(Color.White, 2), // Bia³a obwódka
                    SymbolScale = 0.4 // Rozmiar
                }
            };
            map.Layers.Add(_markerLayer);

            // USTAWIAMY OD RAZU LEGNICÊ NA ŒRODEK
            (double x, double y) = SphericalMercator.FromLonLat(16.17950, 51.20800);
            map.Home = n => n.CenterOnAndZoomTo(new MPoint(x, y), 14);

            return map;
        }

        private void AddMarker(double lon, double lat)
        {
            var position = SphericalMercator.FromLonLat(lon, lat);

            _markerLayer.Features = new List<IFeature>
            {
                new PointFeature(new MPoint(position.x, position.y))
            };

            _mapControl.Map.Navigator.CenterOnAndZoomTo(new MPoint(position.x, position.y), 14);
            _mapControl.Refresh();

            Console.WriteLine($"Marker dodany na mapie: Lon = {lon}, Lat = {lat}");
        }


        private void OnMapClick(object sender, MapInfoEventArgs e)
        {
            if (e.MapInfo?.WorldPosition == null) return;

            var lonLat = SphericalMercator.ToLonLat(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y);
            _viewModel.Latitude = lonLat.Item2.ToString();
            _viewModel.Longitude = lonLat.Item1.ToString();
            _viewModel.RaisePropertyChanged(nameof(_viewModel.Latitude));
            _viewModel.RaisePropertyChanged(nameof(_viewModel.Longitude));

            // Czyszczenie starych markerów i dodanie nowego
            _markerLayer.Features = new List<IFeature>
            {
                new PointFeature(new MPoint(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y))
            };

            _mapControl.Refresh(); // Odœwie¿enie mapy
        }

        private async void OnSearchAddress(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.Address))
            {
                _viewModel.ErrorMessage = "WprowadŸ adres.";
                Console.WriteLine("Adres jest pusty");
                return;
            }

            try
            {
                Console.WriteLine($"Wyszukiwanie adresu: {_viewModel.Address}");

                using var client = new HttpClient();

                // Ustawienie User-Agent, wymagane przez Nominatim
                client.DefaultRequestHeaders.Add("User-Agent", "frocar (frocar@gmail.com)");

                var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(_viewModel.Address)}";
                var response = await client.GetAsync(url);

                Console.WriteLine($"OdpowiedŸ z serwera: {response.StatusCode}");
                //_viewModel.ErrorMessage = $"OdpowiedŸ z serwera: {response.StatusCode}";

                if (!response.IsSuccessStatusCode)
                {
                    _viewModel.ErrorMessage = $"B³¹d serwera: {response.StatusCode}";
                    Console.WriteLine($"B³¹d serwera: {response.StatusCode}");
                    return;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Treœæ odpowiedzi: {jsonResponse}");
                //_viewModel.ErrorMessage = $"Treœæ odpowiedzi: {jsonResponse}";

                var results = JsonSerializer.Deserialize<List<NominatimResult>>(jsonResponse);

                if (results != null && results.Count > 0)
                {
                    var result = results[0];

                    Console.WriteLine($"Wynik: Lat = {result.Lat}, Lon = {result.Lon}");
                    _viewModel.ErrorMessage = $"Wynik: Lat = {result.Lat}, Lon = {result.Lon}";

                    // Przypisanie do pól tekstowych
                    // Przypisanie do pól tekstowych z zamian¹ separatora na zgodny z kultur¹ systemow¹
                    _viewModel.Latitude = result.Lat.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    _viewModel.Longitude = result.Lon.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                    _viewModel.RaisePropertyChanged(nameof(_viewModel.Latitude));
                    _viewModel.RaisePropertyChanged(nameof(_viewModel.Longitude));


                    // Konwersja na double dla mapy (bezpoœrednio po przypisaniu stringów)
                    if (double.TryParse(result.Lon, NumberStyles.Float, CultureInfo.InvariantCulture, out double lon) &&
                        double.TryParse(result.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
                    {
                        AddMarker(lon, lat);
                    }
                    else
                    {
                        Console.WriteLine("Nie uda³o siê sparsowaæ wspó³rzêdnych.");
                        //_viewModel.ErrorMessage = "Nie uda³o siê sparsowaæ wspó³rzêdnych.";
                    }

                }
                else
                {
                    //_viewModel.ErrorMessage = "Nie znaleziono adresu.";
                    Console.WriteLine("Nie znaleziono adresu.");
                }
            }
            catch (Exception ex)
            {
                //_viewModel.ErrorMessage = $"B³¹d: {ex.Message}";
                Console.WriteLine($"B³¹d: {ex.Message}");
            }
        }

    }
}
