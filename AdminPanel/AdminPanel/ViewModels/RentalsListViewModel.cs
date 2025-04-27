using AdminPanel.Models;
using AdminPanel.Services;
using Avalonia.Controls;
using Avalonia.Styling;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Diagnostics;
using Avalonia.Threading;
using AdminPanel.Views;
using System.Collections.Generic;

namespace AdminPanel.ViewModels
{
    public class RentalsListViewModel : ViewModelBase
    {
        private ObservableCollection<CarRentalDto> _rentals = new();
        private string _errorMessage = string.Empty;

        public ObservableCollection<CarRentalDto> Rentals
        {
            get => _rentals;
            set => this.RaiseAndSetIfChanged(ref _rentals, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

        public RentalsListViewModel()
        {
            RefreshCommand = ReactiveCommand.CreateFromTask(LoadRentals);
            _ = LoadRentals();
        }

        private async Task LoadRentals()
        {
            try
            {
                Debug.WriteLine("Rozpoczynam ładowanie wypożyczeń...");
                var result = await ApiService.GetCarRentals();
                bool isSuccess = result.IsSuccess;
                List<CarRentalDto> rentals = result.Rentals;
                string message = result.Message;

                Debug.WriteLine($"Wynik GetCarRentals: IsSuccess={isSuccess}, RentalsCount={(rentals != null ? rentals.Count : 0)}, Message={message}");

                if (isSuccess && rentals != null)
                {
                    Debug.WriteLine($"Załadowano {rentals.Count} wypożyczeń.");
                    Rentals.Clear();
                    foreach (var rental in rentals)
                    {
                        var userResult = await ApiService.GetUserFromId(rental.UserId);
                        if (userResult.IsSuccess && userResult.User != null)
                        {
                            rental.Username = userResult.User.Username;
                            Debug.WriteLine($"Pobrano użytkownika: {rental.Username} dla UserId={rental.UserId}");
                        }
                        else
                        {
                            rental.Username = $"Nieznany (ID: {rental.UserId})";
                            Debug.WriteLine($"Nie udało się pobrać użytkownika dla UserId={rental.UserId}: {userResult.Message}");
                        }

                        rental.CancelCommand = ReactiveCommand.CreateFromTask<int>(CancelRental);
                        rental.ResumeCommand = ReactiveCommand.CreateFromTask<int>(ResumeRental);
                        rental.DeleteCommand = ReactiveCommand.CreateFromTask<int>(DeleteRental); // Inicjalizacja DeleteCommand
                        Rentals.Add(rental);
                    }
                }
                else
                {
                    ErrorMessage = message ?? "Nie udało się pobrać listy wypożyczeń. Sprawdź połączenie z API lub dostępność endpointu.";
                    Debug.WriteLine($"Błąd ładowania: {ErrorMessage}");
                    await ShowMessageBox("Błąd", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Wystąpił nieoczekiwany błąd podczas ładowania wypożyczeń: {ex.Message}";
                Debug.WriteLine($"Wyjątek w LoadRentals: {ErrorMessage}\nStackTrace: {ex.StackTrace}");
                await ShowMessageBox("Błąd", ErrorMessage);
            }
        }

        private async Task CancelRental(int rentalId)
        {
            Debug.WriteLine($"CancelRental wywołane dla rentalId={rentalId}");
            try
            {
                if (App.MainWindow == null)
                {
                    ErrorMessage = "Błąd: Główne okno aplikacji nie jest dostępne.";
                    Debug.WriteLine(ErrorMessage);
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                bool confirmed = await ShowConfirmCancelDialog(rentalId);
                Debug.WriteLine($"Potwierdzenie anulowania: {confirmed}");
                if (!confirmed)
                {
                    Debug.WriteLine("Anulowanie odrzucone przez użytkownika.");
                    return;
                }

                var rentalToCancel = Rentals.FirstOrDefault(r => r.CarRentalId == rentalId);
                if (rentalToCancel == null)
                {
                    ErrorMessage = $"Nie znaleziono wypożyczenia o ID {rentalId}.";
                    Debug.WriteLine(ErrorMessage);
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                Debug.WriteLine($"Znaleziono wypożyczenie: CarRentalId={rentalToCancel.CarRentalId}, CarListingId={rentalToCancel.CarListingId}");

                // Krok 1: Zmiana statusu wypożyczenia na "Cancelled"
                Debug.WriteLine("Wywołuję UpdateRentalStatus...");
                var statusResult = await ApiService.UpdateRentalStatus(rentalId, "Cancelled");
                if (!statusResult.IsSuccess)
                {
                    ErrorMessage = statusResult.Message ?? "Nie udało się anulować wypożyczenia.";
                    Debug.WriteLine($"Błąd UpdateRentalStatus: {ErrorMessage}");
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }
                Debug.WriteLine("UpdateRentalStatus zakończone sukcesem.");

                // Aktualizacja statusu w obiekcie lokalnym
                Debug.WriteLine($"Aktualizuję RentalStatus na 'Cancelled' dla CarRentalId={rentalId}...");
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    rentalToCancel.RentalStatus = "Cancelled";
                    Debug.WriteLine("RentalStatus zaktualizowany pomyślnie.");
                });

                // Krok 2: Przywrócenie dostępności samochodu
                Debug.WriteLine($"Wywołuję UpdateCarAvailability dla CarListingId={rentalToCancel.CarListingId}...");
                var availabilityResult = await ApiService.UpdateCarAvailability(rentalToCancel.CarListingId, true);
                if (!availabilityResult.IsSuccess)
                {
                    ErrorMessage = availabilityResult.Message ?? "Nie udało się przywrócić dostępności samochodu.";
                    Debug.WriteLine($"Błąd UpdateCarAvailability: {ErrorMessage}");
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }
                Debug.WriteLine("UpdateCarAvailability zakończone sukcesem.");

                await ShowMessageBox("Sukces", "Wypożyczenie zostało anulowane, a samochód jest ponownie dostępny!");
                Debug.WriteLine("Anulowanie zakończone sukcesem.");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas anulowania wypożyczenia: {ex.Message}";
                Debug.WriteLine($"KRYTYCZNY Wyjątek w CancelRental: {ErrorMessage}\nStackTrace: {ex.StackTrace}");
                await ShowMessageBox("Błąd", ErrorMessage);
            }
        }

        private async Task ResumeRental(int rentalId)
        {
            Debug.WriteLine($"ResumeRental wywołane dla rentalId={rentalId}");
            try
            {
                if (App.MainWindow == null)
                {
                    ErrorMessage = "Błąd: Główne okno aplikacji nie jest dostępne.";
                    Debug.WriteLine(ErrorMessage);
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                var rentalToResume = Rentals.FirstOrDefault(r => r.CarRentalId == rentalId);
                if (rentalToResume == null)
                {
                    ErrorMessage = $"Nie znaleziono wypożyczenia o ID {rentalId}.";
                    Debug.WriteLine(ErrorMessage);
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                Debug.WriteLine($"Znaleziono wypożyczenie: CarRentalId={rentalToResume.CarRentalId}, CarListingId={rentalToResume.CarListingId}, RentalEndDate={rentalToResume.RentalEndDate}");

                // Sprawdzenie, czy termin wypożyczenia jeszcze nie minął
                if (rentalToResume.RentalEndDate < DateTime.Now)
                {
                    ErrorMessage = $"Nie można wznowić wypożyczenia o ID {rentalId}, ponieważ termin zakończenia ({rentalToResume.RentalEndDate:dd.MM.yyyy HH:mm}) już minął.";
                    Debug.WriteLine(ErrorMessage);
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                bool confirmed = await ShowConfirmResumeDialog(rentalId);
                Debug.WriteLine($"Potwierdzenie wznowienia: {confirmed}");
                if (!confirmed)
                {
                    Debug.WriteLine("Wznowienie odrzucone przez użytkownika.");
                    return;
                }

                // Krok 1: Zmiana statusu wypożyczenia na "Active"
                Debug.WriteLine("Wywołuję UpdateRentalStatus dla Active...");
                var statusResult = await ApiService.UpdateRentalStatus(rentalId, "Active");
                if (!statusResult.IsSuccess)
                {
                    ErrorMessage = statusResult.Message ?? "Nie udało się wznowić wypożyczenia.";
                    Debug.WriteLine($"Błąd UpdateRentalStatus: {ErrorMessage}");
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }
                Debug.WriteLine("UpdateRentalStatus zakończone sukcesem.");

                // Aktualizacja statusu w obiekcie lokalnym
                Debug.WriteLine($"Aktualizuję RentalStatus na 'Active' dla CarRentalId={rentalId}...");
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    rentalToResume.RentalStatus = "Active";
                    Debug.WriteLine("RentalStatus zaktualizowany pomyślnie.");
                });

                // Krok 2: Ustawienie samochodu jako niedostępny
                Debug.WriteLine($"Wywołuję UpdateCarAvailability dla CarListingId={rentalToResume.CarListingId}...");
                var availabilityResult = await ApiService.UpdateCarAvailability(rentalToResume.CarListingId, false);
                if (!availabilityResult.IsSuccess)
                {
                    ErrorMessage = availabilityResult.Message ?? "Nie udało się ustawić samochodu jako niedostępny.";
                    Debug.WriteLine($"Błąd UpdateCarAvailability: {ErrorMessage}");
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }
                Debug.WriteLine("UpdateCarAvailability zakończone sukcesem.");

                await ShowMessageBox("Sukces", "Wypożyczenie zostało wznowione, a samochód jest ponownie niedostępny!");
                Debug.WriteLine("Wznowienie zakończone sukcesem.");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas wznawiania wypożyczenia: {ex.Message}";
                Debug.WriteLine($"KRYTYCZNY Wyjątek w ResumeRental: {ErrorMessage}\nStackTrace: {ex.StackTrace}");
                await ShowMessageBox("Błąd", ErrorMessage);
            }
        }

        private async Task DeleteRental(int rentalId)
        {
            Debug.WriteLine($"DeleteRental wywołane dla rentalId={rentalId}");
            try
            {
                if (App.MainWindow == null)
                {
                    ErrorMessage = "Błąd: Główne okno aplikacji nie jest dostępne.";
                    Debug.WriteLine(ErrorMessage);
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                bool confirmed = await ShowConfirmDeleteDialog(rentalId);
                Debug.WriteLine($"Potwierdzenie usunięcia: {confirmed}");
                if (!confirmed)
                {
                    Debug.WriteLine("Usunięcie odrzucone przez użytkownika.");
                    return;
                }

                var rentalToDelete = Rentals.FirstOrDefault(r => r.CarRentalId == rentalId);
                if (rentalToDelete == null)
                {
                    ErrorMessage = $"Nie znaleziono wypożyczenia o ID {rentalId}.";
                    Debug.WriteLine(ErrorMessage);
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                Debug.WriteLine($"Znaleziono wypożyczenie: CarRentalId={rentalToDelete.CarRentalId}, CarListingId={rentalToDelete.CarListingId}");

                // Krok 1: Usunięcie wypożyczenia przez API
                Debug.WriteLine("Wywołuję DeleteCarRental...");
                var deleteResult = await ApiService.DeleteCarRental(rentalId);
                if (!deleteResult.IsSuccess)
                {
                    ErrorMessage = deleteResult.Message ?? "Nie udało się usunąć wypożyczenia.";
                    Debug.WriteLine($"Błąd DeleteCarRental: {ErrorMessage}");
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }
                Debug.WriteLine("DeleteCarRental zakończone sukcesem.");

                // Krok 2: Przywrócenie dostępności samochodu
                Debug.WriteLine($"Wywołuję UpdateCarAvailability dla CarListingId={rentalToDelete.CarListingId}...");
                var availabilityResult = await ApiService.UpdateCarAvailability(rentalToDelete.CarListingId, true);
                if (!availabilityResult.IsSuccess)
                {
                    ErrorMessage = availabilityResult.Message ?? "Nie udało się przywrócić dostępności samochodu.";
                    Debug.WriteLine($"Błąd UpdateCarAvailability: {ErrorMessage}");
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }
                Debug.WriteLine("UpdateCarAvailability zakończone sukcesem.");

                // Usunięcie wypożyczenia z lokalnej listy
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Rentals.Remove(rentalToDelete);
                    Debug.WriteLine($"Wypożyczenie CarRentalId={rentalId} usunięte z listy Rentals.");
                });

                await ShowMessageBox("Sukces", "Wypożyczenie zostało usunięte, a samochód jest ponownie dostępny!");
                Debug.WriteLine("Usunięcie zakończone sukcesem.");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas usuwania wypożyczenia: {ex.Message}";
                Debug.WriteLine($"KRYTYCZNY Wyjątek w DeleteRental: {ErrorMessage}\nStackTrace: {ex.StackTrace}");
                await ShowMessageBox("Błąd", ErrorMessage);
            }
        }

        private async Task<bool> ShowConfirmCancelDialog(int rentalId)
        {
            try
            {
                Debug.WriteLine($"Pokazuję ConfirmMessageBoxView dla rentalId={rentalId} (Anuluj)");
                var dialog = new ConfirmMessageBoxView("Potwierdzenie anulowania")
                {
                    Message = $"Czy na pewno chcesz anulować wypożyczenie o ID {rentalId}?",
                    RequestedThemeVariant = App.MainWindow?.RequestedThemeVariant ?? ThemeVariant.Default
                };
                await dialog.ShowDialog(App.MainWindow);
                var result = await dialog.Result.Task;
                Debug.WriteLine($"Wynik ConfirmMessageBoxView (Anuluj): {result}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wyjątek w ShowConfirmCancelDialog: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }

        private async Task<bool> ShowConfirmResumeDialog(int rentalId)
        {
            try
            {
                Debug.WriteLine($"Pokazuję ConfirmMessageBoxView dla rentalId={rentalId} (Wznów)");
                var dialog = new ConfirmMessageBoxView("Potwierdzenie wznowienia")
                {
                    Message = $"Czy na pewno chcesz wznowić wypożyczenie o ID {rentalId}?",
                    RequestedThemeVariant = App.MainWindow?.RequestedThemeVariant ?? ThemeVariant.Default
                };
                await dialog.ShowDialog(App.MainWindow);
                var result = await dialog.Result.Task;
                Debug.WriteLine($"Wynik ConfirmMessageBoxView (Wznów): {result}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wyjątek w ShowConfirmResumeDialog: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }

        private async Task<bool> ShowConfirmDeleteDialog(int rentalId)
        {
            try
            {
                Debug.WriteLine($"Pokazuję ConfirmMessageBoxView dla rentalId={rentalId} (Usuń)");
                var dialog = new ConfirmMessageBoxView("Potwierdzenie usunięcia")
                {
                    Message = $"Czy na pewno chcesz usunąć wypożyczenie o ID {rentalId}? Tej operacji nie można cofnąć.",
                    RequestedThemeVariant = App.MainWindow?.RequestedThemeVariant ?? ThemeVariant.Default
                };
                await dialog.ShowDialog(App.MainWindow);
                var result = await dialog.Result.Task;
                Debug.WriteLine($"Wynik ConfirmMessageBoxView (Usuń): {result}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wyjątek w ShowConfirmDeleteDialog: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }

        private async Task ShowMessageBox(string title, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    Debug.WriteLine("Próba wyświetlenia pustego MessageBoxView - pomijam.");
                    return;
                }

                if (App.MainWindow == null)
                {
                    Debug.WriteLine("Błąd: App.MainWindow jest null w ShowMessageBox.");
                    return;
                }

                Debug.WriteLine($"Pokazuję MessageBoxView: Title={title}, Message={message}");
                var dialog = new MessageBoxView(title)
                {
                    Message = message,
                    RequestedThemeVariant = App.MainWindow?.RequestedThemeVariant ?? ThemeVariant.Default
                };
                await dialog.ShowDialog(App.MainWindow);
                Debug.WriteLine("MessageBoxView wyświetlony pomyślnie.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wyjątek w ShowMessageBox: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }
    }
}