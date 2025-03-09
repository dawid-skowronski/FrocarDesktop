using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<string> ?Features { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int UserId { get; set; }

        public string FeaturesAsString => string.Join(", ", Features);

    }

}


