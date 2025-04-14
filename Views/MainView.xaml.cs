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
            this.DataContext = App.ServiceProvider.GetService<MainViewModel>();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && sender is ComboBox comboBox)
            {
                vm.SelectedSortOption = comboBox.SelectedValue?.ToString() ?? "MarketCap";
                vm.SortCurrenciesCommand.Execute(null);
            }
        }

        private void ListView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.NavigateToDetailsCommand.Execute(null);
            }
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is StackPanel stackPanel && DataContext is MainViewModel vm)
            {
                string columnName = stackPanel.Tag?.ToString();
                if (!string.IsNullOrEmpty(columnName))
                {
                    vm.SortByColumnCommand.Execute(columnName);
                }
            }
        }
    }
}