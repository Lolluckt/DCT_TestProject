using System.Windows.Controls;
using System.Windows.Input;
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && sender is ComboBox cb)
            {
                vm.SelectedSortOption = cb.SelectedValue?.ToString() ?? "MarketCap";
                vm.SortCurrencies();
            }
        }

        private void ListView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.NavigateToDetails();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && DataContext is MainViewModel vm)
            {
                string column = tb.Tag?.ToString();
                if (!string.IsNullOrEmpty(column))
                    vm.SortByColumn(column);
            }
        }
    }
}
