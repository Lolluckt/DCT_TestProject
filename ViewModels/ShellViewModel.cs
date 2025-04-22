using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoTrackerApp.Infrastructure;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace CryptoTrackerApp.ViewModels
{
    public partial class ShellViewModel : BaseViewModel
    {
        private const string THEME_SETTINGS_FILE = "theme_settings.xml";

        public static NavigationService CurrentNavService { get; private set; }
        public static void SetNavService(NavigationService nav) => CurrentNavService = nav;

        [ObservableProperty] private bool isDark;

        public ShellViewModel()
        {
            LoadThemePreference();
            ApplyTheme(IsDark);
        }

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
            ApplyTheme(IsDark);
            SaveThemePreference();
        }

        private void ApplyTheme(bool isDark)
        {
            var dict = new ResourceDictionary
            {
                Source = new Uri($"/Themes/{(isDark ? "Dark" : "Light")}.xaml", UriKind.Relative)
            };

            var appDicts = Application.Current.Resources.MergedDictionaries;
            if (appDicts.Count == 0)
                appDicts.Add(dict);
            else
                appDicts[0] = dict;
        }

        private void SaveThemePreference()
        {
            try
            {
                using (var writer = new StreamWriter(THEME_SETTINGS_FILE))
                {
                    var serializer = new XmlSerializer(typeof(ThemeSettings));
                    serializer.Serialize(writer, new ThemeSettings { IsDarkTheme = IsDark });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving theme settings: {ex.Message}");
            }
        }

        private void LoadThemePreference()
        {
            try
            {
                if (File.Exists(THEME_SETTINGS_FILE))
                {
                    using (var reader = new StreamReader(THEME_SETTINGS_FILE))
                    {
                        var serializer = new XmlSerializer(typeof(ThemeSettings));
                        var settings = (ThemeSettings)serializer.Deserialize(reader);
                        IsDark = settings.IsDarkTheme;
                    }
                }
                else
                {
                    IsDark = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading theme settings: {ex.Message}");
                IsDark = false;
            }
        }
    }
    public class ThemeSettings
    {
        public bool IsDarkTheme { get; set; }
    }
}
