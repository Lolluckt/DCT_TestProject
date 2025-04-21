using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoTrackerApp.Infrastructure;
using System;
using System.Windows;

namespace CryptoTrackerApp.ViewModels
{
    public partial class ShellViewModel : BaseViewModel
    {
        public static NavigationService CurrentNavService { get; private set; }
        public static void SetNavService(NavigationService nav) => CurrentNavService = nav;

        [ObservableProperty] private bool isDark;

        [RelayCommand]
        public void NavigateHome() => NavigateTo("MainView");

        [RelayCommand]
        public void GoBack() => base.GoBack();

        [RelayCommand]
        public void NavigateToConverter() => NavigateTo("ConverterView");

        [RelayCommand]
        public void ToggleTheme()
        {
            IsDark = !IsDark;
            var dict = new ResourceDictionary
            {
                Source = new Uri($"/Themes/{(IsDark ? "Dark" : "Light")}.xaml", UriKind.Relative)
            };

            var appDicts = Application.Current.Resources.MergedDictionaries;
            if (appDicts.Count == 0)
                appDicts.Add(dict);
            else
                appDicts[0] = dict;
        }
    }
}
