using ReactiveUI;
using System;
using System.Collections.Generic;
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
            set => this.RaiseAndSetIfChanged(ref _reviews, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public ReactiveCommand<Unit, Unit> LoadReviewsCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

        public ReviewsListViewModel()
        {
            RefreshCommand = ReactiveCommand.CreateFromTask(LoadReviews);
            LoadReviewsCommand = ReactiveCommand.CreateFromTask(LoadReviews);
            LoadReviewsCommand.ThrownExceptions.Subscribe(ex =>
            {
                ErrorMessage = $"Błąd ładowania recenzji: {ex.Message}";
                IsLoading = false;
            });
            LoadReviewsCommand.Execute().Subscribe();
        }

        private async Task LoadReviews()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var (isSuccess, reviews, message) = await ReviewService.GetReviews();

                if (isSuccess && reviews != null)
                {
                    foreach (var review in reviews)
                    {
                        review.DeleteCommand = ReactiveCommand.CreateFromTask<int>(async reviewId =>
                        {
                            var (deleteSuccess, deleteMessage) = await ReviewService.DeleteReview(reviewId);
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
                    Reviews = new List<ReviewDto>();
                    ErrorMessage = message;
                }
            }
            catch (Exception ex)
            {
                Reviews = new List<ReviewDto>();
                ErrorMessage = $"Błąd: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}