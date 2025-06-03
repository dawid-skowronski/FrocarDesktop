using AdminPanel.Models;
using AdminPanel.Services;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace AdminPanel.ViewModels
{
    public class StatisticsViewModel : ViewModelBase
    {
        private StatisticsDto _statistics;
        private string _errorMessage;
        private bool _isLoading;

        public StatisticsDto Statistics
        {
            get => _statistics;
            set
            {
                this.RaiseAndSetIfChanged(ref _statistics, value);
                this.RaisePropertyChanged(nameof(TopSpenderFormatted));
                this.RaisePropertyChanged(nameof(UserWithMostCarsFormatted));
                this.RaisePropertyChanged(nameof(Statistics.TopSpenders));
                this.RaisePropertyChanged(nameof(Statistics.TopProfitableCars));
                this.RaisePropertyChanged(nameof(Statistics.TopRatedCars));
                this.RaisePropertyChanged(nameof(Statistics.MostExpensiveRentalCost));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public string TopSpenderFormatted
        {
            get
            {
                if (Statistics == null || string.IsNullOrEmpty(Statistics.TopSpenderUsername))
                    return "Najwięcej wydał: Brak danych (0,00 zł)";
                return $"Najwięcej wydał: {Statistics.TopSpenderUsername} ({Statistics.TopSpenderAmount:C})";
            }
        }

        public string UserWithMostCarsFormatted
        {
            get
            {
                if (Statistics == null || string.IsNullOrEmpty(Statistics.UserWithMostCarsUsername))
                    return "Najwięcej wystawionych pojazdów: Brak danych (0)";
                return $"Najwięcej wystawionych pojazdów: {Statistics.UserWithMostCarsUsername} ({Statistics.UserWithMostCarsCount})";
            }
        }

        public ReactiveCommand<Unit, Unit> LoadStatisticsCommand { get; }

        public StatisticsViewModel()
        {
            LoadStatisticsCommand = ReactiveCommand.CreateFromTask(LoadStatistics);
            LoadStatisticsCommand.Execute().Subscribe();
        }

        private async Task LoadStatistics()
        {
            IsLoading = true;
            ErrorMessage = null;

            var result = await StatisticsService.GetStatistics();
            if (result.IsSuccess)
            {
                Statistics = result.Statistics;
            }
            else
            {
                ErrorMessage = result.Message;
            }

            IsLoading = false;
        }
    }
}