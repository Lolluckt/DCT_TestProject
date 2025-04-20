using CryptoTrackerApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CryptoTrackerApp.Views
{
    public partial class CurrencyDetailsView : Page
    {
        public CurrencyDetailsView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetService<CurrencyDetailsViewModel>();
            Unloaded += (s, e) => { if (PlotView != null) PlotView.Model = null; };
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
