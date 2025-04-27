using System;
using System.Collections.Generic;
using System.Reactive;
using ReactiveUI;

namespace AdminPanel.Models
{
    public class CarRentalDto : ReactiveObject
    {
        private string _rentalStatus;

        public int CarRentalId { get; set; }
        public int CarListingId { get; set; }
        public CarListing CarListing { get; set; }
        public int UserId { get; set; }
        public UserDto User { get; set; }
        public string Username { get; set; }
        public DateTime RentalStartDate { get; set; }
        public DateTime RentalEndDate { get; set; }
        public decimal RentalPrice { get; set; }

        public string RentalStatus
        {
            get => _rentalStatus;
            set => this.RaiseAndSetIfChanged(ref _rentalStatus, value);
        }

        public ReactiveCommand<int, Unit> CancelCommand { get; set; }
        public ReactiveCommand<int, Unit> ResumeCommand { get; set; }
        public ReactiveCommand<int, Unit> DeleteCommand { get; set; } // Nowy command dla przycisku "Usuń"
    }
}