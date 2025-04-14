using System.Windows;
using CryptoTrackerApp.ViewModels;
using CryptoTrackerApp.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoTrackerApp.Views
{
    public partial class ShellView : Window
    {
        public ShellView()
        {
            InitializeComponent();
            var vm = App.ServiceProvider.GetService<ShellViewModel>();
            DataContext = vm;
            var navService = App.ServiceProvider.GetService<NavigationService>();
            navService.SetFrame(MainFrame);
            ShellViewModel.SetNavService(navService);
            navService.NavigateTo("MainView");
        }
    }
}
