using AdminPanel.Models;
using AdminPanel.Services;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using AdminPanel.Views;
using Avalonia.Styling;
using System;

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
            _ = LoadCars();
        }

        private async Task LoadCars()
        {
            try
            {
                var cars = await ApiService.GetCarListings();
                Cars.Clear();
                foreach (var car in cars)
                {
                    car.DeleteCommand = ReactiveCommand.CreateFromTask<int>(ConfirmDeleteCar);
                    Cars.Add(car);
                }
            }
            catch (Exception ex)
            {
                await ShowMessageBox("Błąd", $"Wystąpił błąd podczas ładowania listy pojazdów: {ex.Message}");
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
                await ShowMessageBox("Błąd", $"Wystąpił błąd podczas usuwania pojazdu: {ex.Message}");
            }
        }

        private async Task DeleteCar(int carId)
        {
            try
            {
                var (isSuccess, errorMessage) = await ApiService.DeleteCarListing(carId);
                if (isSuccess)
                {
                    var carToRemove = Cars.FirstOrDefault(c => c.Id == carId);
                    if (carToRemove != null)
                    {
                        Cars.Remove(carToRemove);
                        await ShowMessageBox("Sukces", "Pojazd został usunięty!");
                    }
                    else
                    {
                        await ShowMessageBox("Błąd", "Nie znaleziono pojazdu do usunięcia.");
                    }
                }
                else
                {
                    await ShowMessageBox("Błąd", errorMessage ?? "Nie udało się usunąć pojazdu.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageBox("Błąd", $"Wystąpił błąd podczas usuwania pojazdu: {ex.Message}");
            }
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