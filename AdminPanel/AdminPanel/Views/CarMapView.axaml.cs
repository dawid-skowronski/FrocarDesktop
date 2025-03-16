using AdminPanel.ViewModels;
using Avalonia.Controls;
using Mapsui.UI.Avalonia;
using Mapsui.Tiling;
using Mapsui.Projections;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Styles;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AdminPanel.Views
{
    public partial class CarMapView : UserControl
    {
        private readonly MapControl _mapControl;
        private readonly CarMapViewModel _viewModel;
        private MemoryLayer _carsLayer;

        public CarMapView()
        {
            InitializeComponent();
            _viewModel = new CarMapViewModel();
            DataContext = _viewModel;

            _mapControl = this.FindControl<MapControl>("CarMap");
            _mapControl.Map = CreateMap();

            // £adujemy dane po inicjalizacji
            LoadCarsOnMap();
        }

        private Mapsui.Map CreateMap()
        {
            var map = new Mapsui.Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());

            _carsLayer = new MemoryLayer
            {
                Name = "CarsLayer",
                Features = Array.Empty<IFeature>(),
                Style = new SymbolStyle
                {
                    SymbolType = SymbolType.Ellipse,
                    Fill = new Brush(Color.Blue), // Niebieskie elipsy
                    Outline = new Pen(Color.White, 2), // Bia³a obwódka
                    SymbolScale = 0.4
                }
            };
            map.Layers.Add(_carsLayer);

            // Domyœlne centrum na Legnicê
            (double x, double y) = SphericalMercator.FromLonLat(16.17950, 51.20800);
            map.Home = n => n.CenterOnAndZoomTo(new MPoint(x, y), 10); // Mniejszy zoom dla widoku ogólnego

            return map;
        }

        private async void LoadCarsOnMap()
        {
            await _viewModel.LoadCarsAsync();

            if (_viewModel.Cars != null && _viewModel.Cars.Any())
            {
                var features = _viewModel.Cars.Select(car =>
                {
                    var position = SphericalMercator.FromLonLat(car.Longitude, car.Latitude);
                    return new PointFeature(new MPoint(position.x, position.y));
                }).ToList();

                _carsLayer.Features = features;

                // Automatyczne dopasowanie widoku do wszystkich punktów
                if (features.Any())
                {
                    var bounds = CalculateBounds(features);
                    _mapControl.Map.Navigator.ZoomToBox(bounds);
                }

                _mapControl.Refresh();
            }
        }
        private MRect CalculateBounds(IEnumerable<IFeature> features)
        {
            // Konwertujemy IFeature na PointFeature i u¿ywamy w³aœciwoœci Point
            var points = features
                .OfType<PointFeature>() // Filtrujemy tylko PointFeature
                .Select(f => f.Point) // U¿ywamy Point zamiast Geometry
                .Where(p => p != null) // Sprawdzamy null
                .ToList();

            if (!points.Any())
            {
                return new MRect(0, 0, 0, 0); // Domyœlny prostok¹t
            }

            var minX = points.Min(p => p.X);
            var maxX = points.Max(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxY = points.Max(p => p.Y);

            return new MRect(minX, minY, maxX, maxY);
        }
    }
}