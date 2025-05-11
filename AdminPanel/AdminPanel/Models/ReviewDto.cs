using System;
using System.Reactive;
using ReactiveUI;

namespace AdminPanel.Models
{
    public class ReviewDto : ReactiveObject
    {
        public int ReviewId { get; set; }
        public int CarRentalId { get; set; }
        public CarRentalDto CarRental { get; set; }
        public int UserId { get; set; }
        public UserDto User { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public ReactiveCommand<int, Unit> DeleteCommand { get; set; }
    }
}