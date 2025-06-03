using System;
using System.Reactive;
using ReactiveUI;

namespace AdminPanel.Models
{
    public class CarRentalDto : ReactiveObject
    {
        private int _carRentalId;
        private int _carListingId;
        private int _userId;
        private DateTime _rentalStartDate;
        private DateTime _rentalEndDate;
        private decimal _rentalPrice;
        private string? _rentalStatus;

        public int CarRentalId
        {
            get => _carRentalId;
            set => this.RaiseAndSetIfChanged(ref _carRentalId, value);
        }

        public int CarListingId
        {
            get => _carListingId;
            set => this.RaiseAndSetIfChanged(ref _carListingId, value);
        }

        public CarListing? CarListing { get; set; }

        public int UserId
        {
            get => _userId;
            set => this.RaiseAndSetIfChanged(ref _userId, value);
        }

        public UserDto? User { get; set; }

        public DateTime RentalStartDate
        {
            get => _rentalStartDate;
            set => this.RaiseAndSetIfChanged(ref _rentalStartDate, value);
        }

        public DateTime RentalEndDate
        {
            get => _rentalEndDate;
            set => this.RaiseAndSetIfChanged(ref _rentalEndDate, value);
        }

        public decimal RentalPrice
        {
            get => _rentalPrice;
            set => this.RaiseAndSetIfChanged(ref _rentalPrice, value);
        }

        public string? RentalStatus
        {
            get => _rentalStatus;
            set => this.RaiseAndSetIfChanged(ref _rentalStatus, value);
        }

        public ReactiveCommand<int, Unit> CancelCommand { get; set; }
        public ReactiveCommand<int, Unit> ResumeCommand { get; set; }
        public ReactiveCommand<int, Unit> DeleteCommand { get; set; }
    }
}