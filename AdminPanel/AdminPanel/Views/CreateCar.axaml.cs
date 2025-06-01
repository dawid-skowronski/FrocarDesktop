using AdminPanel.ViewModels;
using Avalonia.Controls;
using Mapsui.UI.Avalonia;
using Mapsui.Projections;
using System;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Styles;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using ReactiveUI;
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
                Features = Array.Empty<IFeature>(),
                Style = new SymbolStyle
                {
                    SymbolType = SymbolType.Ellipse,
                    Fill = new Brush(Color.Red), 
                    Outline = new Pen(Color.White, 2), 
                    SymbolScale = 0.4 
                }
            };
            map.Layers.Add(_markerLayer);

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

        }


        private void OnMapClick(object sender, MapInfoEventArgs e)
        {
            if (e.MapInfo?.WorldPosition == null) return;

            var lonLat = SphericalMercator.ToLonLat(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y);
            _viewModel.Latitude = lonLat.Item2.ToString();
            _viewModel.Longitude = lonLat.Item1.ToString();
            _viewModel.RaisePropertyChanged(nameof(_viewModel.Latitude));
            _viewModel.RaisePropertyChanged(nameof(_viewModel.Longitude));

            _markerLayer.Features = new List<IFeature>
            {
                new PointFeature(new MPoint(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y))
            };

            _mapControl.Refresh();
        }

        private async void OnSearchAddress(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.Address))
            {
                _viewModel.ErrorMessage = "Wprowadü adres.";
                return;
            }

            try
            {
                using var client = new HttpClient();

                client.DefaultRequestHeaders.Add("User-Agent", "frocar (frocar@gmail.com)");

                var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(_viewModel.Address)}";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _viewModel.ErrorMessage = $"B≥πd serwera: {response.StatusCode}";
                    return;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<List<NominatimResult>>(jsonResponse);

                if (results != null && results.Count > 0)
                {
                    var result = results[0];

                    _viewModel.Latitude = result.Lat.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    _viewModel.Longitude = result.Lon.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                    _viewModel.RaisePropertyChanged(nameof(_viewModel.Latitude));
                    _viewModel.RaisePropertyChanged(nameof(_viewModel.Longitude));

                    if (double.TryParse(result.Lon, NumberStyles.Float, CultureInfo.InvariantCulture, out double lon) &&
                        double.TryParse(result.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
                    {
                        AddMarker(lon, lat);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B≥πd: {ex.Message}");
            }
        }
    }
}
