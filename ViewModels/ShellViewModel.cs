using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoTrackerApp.Infrastructure;

namespace CryptoTrackerApp.ViewModels
{
    public partial class ShellViewModel : BaseViewModel
    {
        public static NavigationService CurrentNavService { get; private set; }

        public static void SetNavService(NavigationService navService)
        {
            CurrentNavService = navService;
        }

        [RelayCommand]
        public void NavigateHome()
        {
            CurrentNavService.NavigateTo("MainView");
        }

        [RelayCommand]
        public void GoBack()
        {
            CurrentNavService.GoBack();
        }
    }
}
