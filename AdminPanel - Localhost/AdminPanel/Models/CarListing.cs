using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace AdminPanel.Models
{
    public class CarListing : ReactiveObject
    {
        private int _id;
        private string? _brand;
        private double _engineCapacity;
        private string? _fuelType;
        private int _seats;
        private string? _carType;
        private double _latitude;
        private double _longitude;
        private int _userId;
        private string? _username;
        private bool _isAvailable;
        private bool _isApproved;
        private double _rentalPricePerDay;
        private double? _averageRating;

        public int Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        public string? Brand
        {
            get => _brand;
            set => this.RaiseAndSetIfChanged(ref _brand, value);
        }

        public double EngineCapacity
        {
            get => _engineCapacity;
            set => this.RaiseAndSetIfChanged(ref _engineCapacity, value);
        }

        public string? FuelType
        {
            get => _fuelType;
            set => this.RaiseAndSetIfChanged(ref _fuelType, value);
        }

        public int Seats
        {
            get => _seats;
            set => this.RaiseAndSetIfChanged(ref _seats, value);
        }

        public string? CarType
        {
            get => _carType;
            set => this.RaiseAndSetIfChanged(ref _carType, value);
        }

        public List<string>? Features { get; set; }

        public double Latitude
        {
            get => _latitude;
            set => this.RaiseAndSetIfChanged(ref _latitude, value);
        }

        public double Longitude
        {
            get => _longitude;
            set => this.RaiseAndSetIfChanged(ref _longitude, value);
        }

        public int UserId
        {
            get => _userId;
            set => this.RaiseAndSetIfChanged(ref _userId, value);
        }

        public string? Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set => this.RaiseAndSetIfChanged(ref _isAvailable, value);
        }

        public bool IsApproved
        {
            get => _isApproved;
            set => this.RaiseAndSetIfChanged(ref _isApproved, value);
        }

        public double RentalPricePerDay
        {
            get => _rentalPricePerDay;
            set => this.RaiseAndSetIfChanged(ref _rentalPricePerDay, value);
        }

        public double? AverageRating
        {
            get => _averageRating;
            set => this.RaiseAndSetIfChanged(ref _averageRating, value);
        }

        public string FeaturesAsString => Features != null && Features.Any()
            ? string.Join(", ", Features)
            : "Brak udogodnień";

        public string LocationString => $"{Latitude:F6}, \n{Longitude:F6}";

        public ReactiveCommand<int, Unit>? DeleteCommand { get; set; }
        public ReactiveCommand<int, Unit>? EditCommand { get; set; }
        public ReactiveCommand<int, Unit>? ApproveCommand { get; set; }
    }
}