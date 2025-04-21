using System.Windows.Controls;
using CryptoTrackerApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoTrackerApp.Views
{
    public partial class ConverterView : Page
    {
        public ConverterView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetService<ConverterViewModel>();
        }
    }
}