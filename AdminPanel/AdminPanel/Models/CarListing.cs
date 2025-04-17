using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace AdminPanel.Models
{
    public class CarListing
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public double EngineCapacity { get; set; }
        public string FuelType { get; set; }
        public int Seats { get; set; }
        public string CarType { get; set; }
        public List<string>? Features { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int UserId { get; set; }
        private string _username; // Nowa właściwość
        public string Username // Publiczny getter i setter
        {
            get => _username;
            set => _username = value;
        }
        public bool IsAvailable { get; set; }
        public bool IsApproved { get; set; }
        public double RentalPricePerDay { get; set; }

        public string FeaturesAsString => string.Join(", ", Features ?? new List<string>());
        public ReactiveCommand<int, Unit>? DeleteCommand { get; set; }
        public ReactiveCommand<int, Unit>? EditCommand { get; set; }
        public string LocationString => $"{Latitude:F6}, \n{Longitude:F6}";
    }
}