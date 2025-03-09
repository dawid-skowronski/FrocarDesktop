using AdminPanel.Models;
using AdminPanel.Services;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace AdminPanel.ViewModels
{
    public class CarsListViewModel : ViewModelBase
    {
        public ObservableCollection<CarListing> Cars { get; } = new();
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

        public CarsListViewModel()
        {
            RefreshCommand = ReactiveCommand.CreateFromTask(LoadCars);
            _ = LoadCars(); // Pobieranie listy na starcie
        }

        private async Task LoadCars()
        {
            var cars = await ApiService.GetCarListings();
            Cars.Clear();
            foreach (var car in cars)
            {
                Cars.Add(car);
            }
        }
    }
}
