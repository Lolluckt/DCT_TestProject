using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace CryptoTrackerApp.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string statusMessage;

        protected async Task ExecuteWithLoadingAsync(Func<Task> operation, string loadingMessage = "Loading...", string successMessage = null)
        {
            try
            {
                IsLoading = true;
                StatusMessage = loadingMessage;

                await operation();

                if (!string.IsNullOrEmpty(successMessage))
                    StatusMessage = successMessage;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                Debug.WriteLine($"Operation failed: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void NavigateTo(string viewName, object parameter = null)
        {
            ShellViewModel.CurrentNavService?.NavigateTo(viewName, parameter);
        }

        protected void GoBack()
        {
            ShellViewModel.CurrentNavService?.GoBack();
        }

        protected void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OpenUrl error: {ex.Message}");
            }
        }
    }
}