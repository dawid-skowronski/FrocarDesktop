using System;
using System.Reactive;
using ReactiveUI;

namespace AdminPanel.Models
{
    public class ReviewDto : ReactiveObject
    {
        private int _reviewId;
        private int _carRentalId;
        private int _userId;
        private int _rating;
        private string? _comment;
        private DateTime _createdAt;

        public int ReviewId
        {
            get => _reviewId;
            set => this.RaiseAndSetIfChanged(ref _reviewId, value);
        }

        public int CarRentalId
        {
            get => _carRentalId;
            set => this.RaiseAndSetIfChanged(ref _carRentalId, value);
        }

        public CarRentalDto? CarRental { get; set; }

        public int UserId
        {
            get => _userId;
            set => this.RaiseAndSetIfChanged(ref _userId, value);
        }

        public UserDto? User { get; set; }

        public int Rating
        {
            get => _rating;
            set => this.RaiseAndSetIfChanged(ref _rating, value);
        }

        public string? Comment
        {
            get => _comment;
            set => this.RaiseAndSetIfChanged(ref _comment, value);
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => this.RaiseAndSetIfChanged(ref _createdAt, value);
        }

        public ReactiveCommand<int, Unit> DeleteCommand { get; set; }
    }
}