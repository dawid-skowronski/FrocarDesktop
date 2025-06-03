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
                var result = await RentalService.GetCarRentals();
                bool isSuccess = result.IsSuccess;
                List<CarRentalDto> rentals = result.Rentals;
                string message = result.Message;

                if (isSuccess && rentals != null)
                {
                    Rentals.Clear();
                    foreach (var rental in rentals)
                    {
                        var userResult = await UserService.GetUserFromId(rental.UserId);
                        if (userResult.IsSuccess && userResult.User != null)
                        {
                            rental.Username = userResult.User.Username;
                        }
                        else
                        {
                            rental.Username = $"Nieznany (ID: {rental.UserId})";
                        }

                        rental.CancelCommand = ReactiveCommand.CreateFromTask<int>(CancelRental);
                        rental.ResumeCommand = ReactiveCommand.CreateFromTask<int>(ResumeRental);
                        rental.DeleteCommand = ReactiveCommand.CreateFromTask<int>(DeleteRental);
                        Rentals.Add(rental);
                    }
                }
                else
                {
                    ErrorMessage = message ?? "Nie udało się pobrać listy wypożyczeń. Sprawdź połączenie z API lub dostępność endpointu.";
                    Rentals.Clear();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Wystąpił nieoczekiwany błąd podczas ładowania wypożyczeń: {ex.Message}";
                await ShowMessageBox("Błąd", ErrorMessage);
            }
        }

        private async Task CancelRental(int rentalId)
        {
            try
            {
                if (App.MainWindow == null)
                {
                    ErrorMessage = "Błąd: Główne okno aplikacji nie jest dostępne.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                bool confirmed = await ShowConfirmCancelDialog(rentalId);
                if (!confirmed)
                {
                    return;
                }

                var rentalToCancel = Rentals.FirstOrDefault(r => r.CarRentalId == rentalId);
                if (rentalToCancel == null)
                {
                    ErrorMessage = $"Nie znaleziono wypożyczenia o ID {rentalId}.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                var statusResult = await RentalService.UpdateRentalStatus(rentalId, "Cancelled");
                if (!statusResult.IsSuccess)
                {
                    ErrorMessage = statusResult.Message ?? "Nie udało się anulować wypożyczenia.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    rentalToCancel.RentalStatus = "Cancelled";
                });

                var availabilityResult = await CarService.UpdateCarAvailability(rentalToCancel.CarListingId, true);
                if (!availabilityResult.IsSuccess)
                {
                    ErrorMessage = availabilityResult.Message ?? "Nie udało się przywrócić dostępności samochodu.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                await ShowMessageBox("Sukces", "Wypożyczenie zostało anulowane, a samochód jest ponownie dostępny!");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas anulowania wypożyczenia: {ex.Message}";
                await ShowMessageBox("Błąd", ErrorMessage);
            }
        }

        private async Task ResumeRental(int rentalId)
        {
            try
            {
                if (App.MainWindow == null)
                {
                    ErrorMessage = "Błąd: Główne okno aplikacji nie jest dostępne.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                var rentalToResume = Rentals.FirstOrDefault(r => r.CarRentalId == rentalId);
                if (rentalToResume == null)
                {
                    ErrorMessage = $"Nie znaleziono wypożyczenia o ID {rentalId}.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                if (rentalToResume.RentalEndDate < DateTime.Now)
                {
                    ErrorMessage = $"Nie można wznowić wypożyczenia o ID {rentalId}, ponieważ termin zakończenia ({rentalToResume.RentalEndDate:dd.MM.yyyy HH:mm}) już minął.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                bool confirmed = await ShowConfirmResumeDialog(rentalId);
                if (!confirmed)
                {
                    return;
                }

                var statusResult = await RentalService.UpdateRentalStatus(rentalId, "Active");
                if (!statusResult.IsSuccess)
                {
                    ErrorMessage = statusResult.Message ?? "Nie udało się wznowić wypożyczenia.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    rentalToResume.RentalStatus = "Active";
                });

                var availabilityResult = await CarService.UpdateCarAvailability(rentalToResume.CarListingId, false);
                if (!availabilityResult.IsSuccess)
                {
                    ErrorMessage = availabilityResult.Message ?? "Nie udało się ustawić samochodu jako niedostępny.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                await ShowMessageBox("Sukces", "Wypożyczenie zostało wznowione, a samochód jest ponownie niedostępny!");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas wznawiania wypożyczenia: {ex.Message}";
                await ShowMessageBox("Błąd", ErrorMessage);
            }
        }

        private async Task DeleteRental(int rentalId)
        {
            try
            {
                if (App.MainWindow == null)
                {
                    ErrorMessage = "Błąd: Główne okno aplikacji nie jest dostępne.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                bool confirmed = await ShowConfirmDeleteDialog(rentalId);
                if (!confirmed)
                {
                    return;
                }

                var rentalToDelete = Rentals.FirstOrDefault(r => r.CarRentalId == rentalId);
                if (rentalToDelete == null)
                {
                    ErrorMessage = $"Nie znaleziono wypożyczenia o ID {rentalId}.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                var deleteResult = await RentalService.DeleteCarRental(rentalId);
                if (!deleteResult.IsSuccess)
                {
                    ErrorMessage = deleteResult.Message ?? "Nie udało się usunąć wypożyczenia.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                var availabilityResult = await CarService.UpdateCarAvailability(rentalToDelete.CarListingId, true);
                if (!availabilityResult.IsSuccess)
                {
                    ErrorMessage = availabilityResult.Message ?? "Nie udało się przywrócić dostępności samochodu.";
                    await ShowMessageBox("Błąd", ErrorMessage);
                    return;
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Rentals.Remove(rentalToDelete);
                });

                await ShowMessageBox("Sukces", "Wypożyczenie zostało usunięte, a samochód jest ponownie dostępny!");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas usuwania wypożyczenia: {ex.Message}";
                await ShowMessageBox("Błąd", ErrorMessage);
            }
        }

        private async Task<bool> ShowConfirmCancelDialog(int rentalId)
        {
            try
            {
                var dialog = new ConfirmMessageBoxView("Potwierdzenie anulowania")
                {
                    Message = $"Czy na pewno chcesz anulować wypożyczenie o ID {rentalId}?",
                    RequestedThemeVariant = App.MainWindow?.RequestedThemeVariant ?? ThemeVariant.Default
                };
                await dialog.ShowDialog(App.MainWindow);
                return await dialog.Result.Task;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> ShowConfirmResumeDialog(int rentalId)
        {
            try
            {
                var dialog = new ConfirmMessageBoxView("Potwierdzenie wznowienia")
                {
                    Message = $"Czy na pewno chcesz wznowić wypożyczenie o ID {rentalId}?",
                    RequestedThemeVariant = App.MainWindow?.RequestedThemeVariant ?? ThemeVariant.Default
                };
                await dialog.ShowDialog(App.MainWindow);
                return await dialog.Result.Task;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> ShowConfirmDeleteDialog(int rentalId)
        {
            try
            {
                var dialog = new ConfirmMessageBoxView("Potwierdzenie usunięcia")
                {
                    Message = $"Czy na pewno chcesz usunąć wypożyczenie o ID {rentalId}? Tej operacji nie można cofnąć.",
                    RequestedThemeVariant = App.MainWindow?.RequestedThemeVariant ?? ThemeVariant.Default
                };
                await dialog.ShowDialog(App.MainWindow);
                return await dialog.Result.Task;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task ShowMessageBox(string title, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    return;
                }

                if (App.MainWindow == null)
                {
                    return;
                }

                var dialog = new MessageBoxView(title)
                {
                    Message = message,
                    RequestedThemeVariant = App.MainWindow?.RequestedThemeVariant ?? ThemeVariant.Default
                };
                await dialog.ShowDialog(App.MainWindow);
            }
            catch (Exception)
            {
            }
        }
    }
}