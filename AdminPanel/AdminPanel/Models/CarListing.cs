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
        public bool IsAvailable { get; set; } // Nowe pole z JSON-a
        public double RentalPricePerDay { get; set; } // Nowe pole z JSON-a

        public string FeaturesAsString => string.Join(", ", Features ?? new List<string>());
        public ReactiveCommand<int, Unit>? DeleteCommand { get; set; }
    }
}