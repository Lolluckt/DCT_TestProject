using System.Windows;
using System.Windows.Controls;
using CryptoTrackerApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using OxyPlot.Wpf;

namespace CryptoTrackerApp.Views
{
    public partial class CurrencyDetailsView : Page
    {
        public CurrencyDetailsView()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider.GetService<CurrencyDetailsViewModel>();
            this.Unloaded += CurrencyDetailsView_Unloaded;
        }

        private void CurrencyDetailsView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (PlotView != null)
            {
                PlotView.Model = null;
            }
        }
    }
}
