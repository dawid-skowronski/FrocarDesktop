using ReactiveUI;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdminPanel.Models;
using AdminPanel.Services;
using System;

namespace AdminPanel.ViewModels
{
    public class CarMapViewModel : ViewModelBase
    {
        private List<CarListing> _cars;
        public List<CarListing> Cars
        {
            get => _cars;
            set => this.RaiseAndSetIfChanged(ref _cars, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public CarMapViewModel()
        {
            Cars = new List<CarListing>();
        }

        public async Task LoadCarsAsync()
        {
            ErrorMessage = string.Empty;

            try
            {
                var cars = await ApiService.GetCarListings();
                if (cars != null)
                {
                    Cars = cars;
                }
                else
                {
                    ErrorMessage = "Nie удалось pobrać listy pojazdów.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas ładowania pojazdów: {ex.Message}";
            }
        }
    }
}