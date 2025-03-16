using AdminPanel.Models;
using AdminPanel.Services;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using Avalonia.Layout;

namespace AdminPanel.ViewModels
{
    public class CarsListViewModel : ViewModelBase
    {
        public ObservableCollection<CarListing> Cars { get; } = new();
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public ReactiveCommand<int, Unit> DeleteCommand { get; }

        public CarsListViewModel()
        {
            RefreshCommand = ReactiveCommand.CreateFromTask(LoadCars);
            DeleteCommand = ReactiveCommand.CreateFromTask<int>(ConfirmDeleteCar);
            _ = LoadCars(); // Pobieranie listy na starcie
        }

        private async Task LoadCars()
        {
            var cars = await ApiService.GetCarListings();
            Cars.Clear();
            foreach (var car in cars)
            {
                car.DeleteCommand = ReactiveCommand.CreateFromTask<int>(ConfirmDeleteCar);
                Cars.Add(car);
            }
        }

        private async Task ConfirmDeleteCar(int carId)
        {
            bool confirmed = await ShowConfirmDeleteDialog(carId);
            if (confirmed)
            {
                await DeleteCar(carId);
            }
        }

        private async Task DeleteCar(int carId)
        {
            var result = await ApiService.DeleteCarListing(carId);
            if (result.IsSuccess)
            {
                var carToRemove = Cars.FirstOrDefault(c => c.Id == carId);
                if (carToRemove != null)
                {
                    Cars.Remove(carToRemove);
                    await ShowMessageBox("Sukces", "Pojazd został usunięty!");
                }
            }
            else
            {
                await ShowMessageBox("Błąd", "Nie udało się usunąć pojazdu.");
            }
        }

        private async Task<bool> ShowConfirmDeleteDialog(int carId)
        {
            var window = new Window
            {
                Title = "Potwierdzenie usunięcia",
                Width = 350,
                Height = 200,
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
                Text = $"Czy na pewno chcesz usunąć pojazd o ID {carId}?",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.Black),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 20
            };

            var yesButton = new Button
            {
                Content = "Tak",
                Width = 100,
                Background = new SolidColorBrush(Color.FromRgb(107, 144, 113)), // Zielony #6B9071
                Foreground = Brushes.White,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            yesButton.Classes.Add("ok"); // Styl z App.axaml dla "ok"

            var noButton = new Button
            {
                Content = "Nie",
                Width = 100,
                Background = new SolidColorBrush(Color.FromRgb(220, 20, 60)), // Czerwony #DC143C
                Foreground = Brushes.White,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            noButton.Classes.Add("delete"); // Styl z App.axaml dla "delete"

            bool? result = null;
            yesButton.Click += (_, _) => { result = true; window.Close(); };
            noButton.Click += (_, _) => { result = false; window.Close(); };

            buttonPanel.Children.Add(yesButton);
            buttonPanel.Children.Add(noButton);

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(buttonPanel);

            window.Content = stackPanel;

            window.TransparencyLevelHint = new[] { WindowTransparencyLevel.AcrylicBlur };
            window.Background = new SolidColorBrush(Color.FromArgb(200, 240, 240, 240));

            await window.ShowDialog(App.MainWindow);
            return result ?? false; // Zwraca true, jeśli "Tak", false w przeciwnym razie
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