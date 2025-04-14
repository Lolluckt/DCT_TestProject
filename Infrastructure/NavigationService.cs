using System;
using System.Windows.Controls;
using CryptoTrackerApp.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoTrackerApp.Infrastructure
{
    public class NavigationService
    {
        private Frame _frame;

        public void SetFrame(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateTo(string pageName, object parameter = null)
        {
            Page page = pageName switch
            {
                "MainView" => App.ServiceProvider.GetService<MainView>(),
                "CurrencyDetailsView" => App.ServiceProvider.GetService<CurrencyDetailsView>(),
                _ => throw new ArgumentException($"Unknown page '{pageName}'"),
            };

            if (page.DataContext is INavigable navObj)
            {
                navObj.OnNavigatedTo(parameter);
            }

            _frame.Navigate(page);
        }

        public void GoBack()
        {
            if (_frame.CanGoBack)
                _frame.GoBack();
        }
    }

    public interface INavigable
    {
        void OnNavigatedTo(object parameter);
    }
}
