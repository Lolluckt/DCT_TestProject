using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CryptoTrackerApp.Models;
using CryptoTrackerApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoTrackerApp.Views
{
    public partial class MainView : Page
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetService<MainViewModel>();
        }

        private void Currency_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm
                && sender is FrameworkElement el
                && el.Tag is Currency cur)
            {
                vm.SelectedCurrency = cur;
                vm.NavigateToDetails();
            }
        }
    }
}