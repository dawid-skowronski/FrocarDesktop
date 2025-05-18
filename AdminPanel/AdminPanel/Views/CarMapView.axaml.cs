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
using Avalonia.ReactiveUI;
using ReactiveUI;
using AdminPanel.Models;

namespace AdminPanel.Views
{
    public partial class CarMapView : UserControl
    {
        private readonly MapControl _mapControl;
        private readonly CarMapViewModel _viewModel;
        private MemoryLayer _carsLayer;
        private bool _isInitialZoom = true;

        public CarMapView()
        {
            InitializeComponent();
            _viewModel = new CarMapViewModel();
            DataContext = _viewModel;

            _mapControl = this.FindControl<MapControl>("CarMap");
            _mapControl.Map = CreateMap();

            _mapControl.Info += MapControl_Info;
            LoadCarsOnMap();

            _viewModel.WhenAnyValue(x => x.FilteredCars).Subscribe(_ => UpdateMap());
        }

        private Mapsui.Map CreateMap()
        {
            var map = new Mapsui.Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());

            _carsLayer = new MemoryLayer
            {
                Name = "CarsLayer",
                IsMapInfoLayer = true
            };
            map.Layers.Add(_carsLayer);

            (double x, double y) = SphericalMercator.FromLonLat(16.17950, 51.20800);
            map.Home = n => n.CenterOnAndZoomTo(new MPoint(x, y), 10);

            return map;
        }

        private async void LoadCarsOnMap()
        {
            await _viewModel.LoadCarsAsync();
            UpdateMap();
        }

        private void UpdateMap()
        {
            if (_viewModel.FilteredCars != null && _viewModel.FilteredCars.Any())
            {
                var features = _viewModel.FilteredCars.Select(car =>
                {
                    var position = SphericalMercator.FromLonLat(car.Longitude, car.Latitude);
                    var feature = new PointFeature(new MPoint(position.x, position.y));
                    feature["CarData"] = car;
                    var color = car.IsAvailable ? new Color(61, 148, 64) : new Color(255, 165, 0);
                    feature.Styles = new List<IStyle>
            {
                new SymbolStyle
                {
                    SymbolType = SymbolType.Ellipse,
                    Fill = new Brush(color),
                    Outline = new Pen(Color.White, 2),
                    SymbolScale = 0.4
                }
            };
                    return feature;
                }).ToList();

                _carsLayer.Features = features;
                _carsLayer.Style = null; 

                if (_isInitialZoom && features.Any())
                {
                    var bounds = CalculateBounds(features);
                    _mapControl.Map.Navigator.ZoomToBox(bounds);
                    _isInitialZoom = false;
                }

                _mapControl.Refresh();
            }
            else
            {
                _carsLayer.Features = Array.Empty<IFeature>();
                _carsLayer.Style = null; 
                _mapControl.Refresh();
            }
        }

        private void MapControl_Info(object sender, MapInfoEventArgs e)
        {
            if (e.MapInfo?.Layer == _carsLayer && e.MapInfo.Feature is PointFeature feature)
            {
                if (feature["CarData"] is CarListing car)
                {
                    var carInfo = _viewModel.GetCarInfo(car);
                    var flyout = new Flyout();
                    flyout.Placement = PlacementMode.Pointer;

                    var border = new Border
                    {
                        BorderBrush = Avalonia.Media.Brush.Parse("#2E7D32"),
                        BorderThickness = new Avalonia.Thickness(2),
                        CornerRadius = new Avalonia.CornerRadius(8),
                        Background = Avalonia.Media.Brush.Parse("#FAFAFA"),
                        Child = new StackPanel
                        {
                            Margin = new Avalonia.Thickness(15),
                            Spacing = 15,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = carInfo,
                                    FontSize = 14,
                                    Foreground = Avalonia.Media.Brush.Parse("#333333"),
                                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                    MaxWidth = 300,
                                    TextAlignment = Avalonia.Media.TextAlignment.Left
                                },
                                new Button
                                {
                                    Content = "Zamknij",
                                    Classes = { "ok" },
                                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                                    Command = ReactiveCommand.Create(() => flyout.Hide())
                                }
                            }
                        }
                    };

                    flyout.Content = border;
                    flyout.ShowAt(_mapControl);
                }
            }
        }

        private MRect CalculateBounds(IEnumerable<IFeature> features)
        {
            var points = features
                .OfType<PointFeature>()
                .Select(f => f.Point)
                .Where(p => p != null)
                .ToList();

            if (!points.Any())
            {
                return new MRect(0, 0, 0, 0);
            }

            var minX = points.Min(p => p.X);
            var maxX = points.Max(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxY = points.Max(p => p.Y);

            return new MRect(minX, minY, maxX, maxY);
        }
    }
}