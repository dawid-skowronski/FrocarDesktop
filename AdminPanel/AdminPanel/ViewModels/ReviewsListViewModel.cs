using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AdminPanel.Models;
using AdminPanel.Services;

namespace AdminPanel.ViewModels
{
    public class ReviewsListViewModel : ViewModelBase
    {
        private List<ReviewDto> _reviews = new List<ReviewDto>();
        private bool _isLoading;
        private string _errorMessage;

        public List<ReviewDto> Reviews
        {
            get => _reviews;
            set
            {
                Debug.WriteLine($"ReviewsListViewModel: Ustawiam Reviews, Count={value?.Count ?? 0}");
                this.RaiseAndSetIfChanged(ref _reviews, value);
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                Debug.WriteLine($"ReviewsListViewModel: IsLoading={value}");
                this.RaiseAndSetIfChanged(ref _isLoading, value);
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                Debug.WriteLine($"ReviewsListViewModel: ErrorMessage={value}");
                this.RaiseAndSetIfChanged(ref _errorMessage, value);
            }
        }

        public ReactiveCommand<Unit, Unit> LoadReviewsCommand { get; }

        public ReviewsListViewModel()
        {
            LoadReviewsCommand = ReactiveCommand.CreateFromTask(LoadReviews);
            LoadReviewsCommand.ThrownExceptions.Subscribe(ex =>
            {
                Debug.WriteLine($"Błąd w LoadReviewsCommand: {ex.Message}\nStackTrace: {ex.StackTrace}");
                ErrorMessage = $"Błąd ładowania recenzji: {ex.Message}";
                IsLoading = false; // Upewniamy się, że IsLoading się resetuje
            });
            Debug.WriteLine("ReviewsListViewModel: Inicjalizacja, wywołuję LoadReviewsCommand...");
            LoadReviewsCommand.Execute().Subscribe();
        }

        private async Task LoadReviews()
        {
            Debug.WriteLine("LoadReviews: Start");
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                Debug.WriteLine("LoadReviews: Wywołuję ApiService.GetReviews...");
                var (isSuccess, reviews, message) = await ApiService.GetReviews();

                Debug.WriteLine($"LoadReviews: Wynik ApiService.GetReviews - IsSuccess: {isSuccess}, Message: {message}, ReviewsCount: {reviews?.Count ?? 0}");

                if (isSuccess && reviews != null)
                {
                    foreach (var review in reviews)
                    {
                        review.DeleteCommand = ReactiveCommand.CreateFromTask<int>(async reviewId =>
                        {
                            Debug.WriteLine($"DeleteReview: Wywołuję DeleteReview dla ReviewId={reviewId}");
                            var (deleteSuccess, deleteMessage) = await ApiService.DeleteReview(reviewId);
                            Debug.WriteLine($"DeleteReview: ReviewId={reviewId}, Success={deleteSuccess}, Message={deleteMessage}");
                            if (deleteSuccess)
                            {
                                Reviews = Reviews.Where(r => r.ReviewId != reviewId).ToList();
                            }
                            else
                            {
                                ErrorMessage = deleteMessage;
                            }
                        });
                    }
                    Reviews = reviews.OrderByDescending(r => r.CreatedAt).ToList();
                }
                else
                {
                    Debug.WriteLine($"LoadReviews: Błąd - {message}");
                    Reviews = new List<ReviewDto>();
                    ErrorMessage = message;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadReviews: Wyjątek: {ex.Message}\nStackTrace: {ex.StackTrace}");
                Reviews = new List<ReviewDto>();
                ErrorMessage = $"Błąd: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine("LoadReviews: Koniec");
            }
        }
    }
}