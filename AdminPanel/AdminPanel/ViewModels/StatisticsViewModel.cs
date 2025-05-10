using AdminPanel.Models;
using AdminPanel.Services;
using ReactiveUI;
using System;
using System.Diagnostics;
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
            Debug.WriteLine("StatisticsViewModel: Tworzenie instancji...");
            LoadStatisticsCommand = ReactiveCommand.CreateFromTask(LoadStatistics);

            Debug.WriteLine("StatisticsViewModel: Inicjalizacja widoku, wywołuję LoadStatisticsCommand...");
            LoadStatisticsCommand.Execute().Subscribe(
                _ => Debug.WriteLine("StatisticsViewModel: LoadStatisticsCommand zakończone sukcesem."),
                ex => Debug.WriteLine($"StatisticsViewModel: Błąd w LoadStatisticsCommand: {ex.Message}")
            );

            this.WhenAnyValue(x => x.Statistics)
                .Subscribe(statistics => Debug.WriteLine($"Statistics zmienione: TotalUsers={statistics?.TotalUsers}"));
        }

        private async Task LoadStatistics()
        {
            Debug.WriteLine("LoadStatistics: Rozpoczynam ładowanie statystyk...");
            IsLoading = true;
            ErrorMessage = null;

            var result = await ApiService.GetStatistics();
            Debug.WriteLine($"LoadStatistics: Wynik ApiService.GetStatistics - IsSuccess: {result.IsSuccess}, Message: {result.Message}");

            if (result.IsSuccess)
            {
                Debug.WriteLine($"LoadStatistics: Statystyki załadowane. TotalUsers: {result.Statistics?.TotalUsers}, TotalCars: {result.Statistics?.TotalCars}");
                Statistics = result.Statistics;
            }
            else
            {
                Debug.WriteLine($"LoadStatistics: Błąd podczas ładowania statystyk: {result.Message}");
                ErrorMessage = result.Message;
            }

            IsLoading = false;
            IsLoading = false;
            Debug.WriteLine("LoadStatistics: Zakończono ładowanie statystyk.");
        }
    }
}