using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoTrackerApp.Models;
using CryptoTrackerApp.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Threading.Tasks;
using CryptoTrackerApp.Infrastructure;
using System.Collections.Generic;

namespace CryptoTrackerApp.ViewModels
{
    public partial class MainViewModel : BaseViewModel, INavigable
    {
        private readonly ICoinGeckoService _coinService;
        public ObservableCollection<Currency> Currencies { get; } = new();
        public ICollectionView CurrenciesView { get; }

        [ObservableProperty]
        private string searchQuery;
        public List<string> SortOptions { get; } = new List<string> { "Name", "Price", "MarketCap" };

        [ObservableProperty]
        private string selectedSortOption = "MarketCap";

        [ObservableProperty]
        private Currency selectedCurrency;

        [ObservableProperty]
        private bool isAscendingSort = false;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string statusMessage;

        public MainViewModel(ICoinGeckoService coinService)
        {
            _coinService = coinService;
            CurrenciesView = CollectionViewSource.GetDefaultView(Currencies);
        }

        [RelayCommand]
        public async Task LoadTopCurrenciesAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading top currencies...";

                Currencies.Clear();
                var list = await _coinService.GetTopCurrenciesAsync(10);
                foreach (var c in list)
                    Currencies.Add(c);

                SortCurrenciesCommand.Execute(null);
                StatusMessage = $"Loaded {Currencies.Count} currencies";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine("Ошибка загрузки валют: " + ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SearchCurrenciesAsync()
        {
            try
            {
                IsLoading = true;

                if (string.IsNullOrWhiteSpace(SearchQuery))
                {
                    await LoadTopCurrenciesAsync();
                    return;
                }

                StatusMessage = $"Searching for '{SearchQuery}'...";
                Currencies.Clear();
                var result = await _coinService.SearchCurrenciesAsync(SearchQuery);
                foreach (var c in result)
                    Currencies.Add(c);

                SortCurrenciesCommand.Execute(null);
                StatusMessage = $"Found {Currencies.Count} results for '{SearchQuery}'";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Search error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine("Ошибка поиска: " + ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void ClearSearch()
        {
            SearchQuery = string.Empty;
            _ = LoadTopCurrenciesAsync();
        }

        [RelayCommand]
        public void SortCurrencies()
        {
            if (CurrenciesView == null) return;

            CurrenciesView.SortDescriptions.Clear();

            ListSortDirection direction = IsAscendingSort ? ListSortDirection.Ascending : ListSortDirection.Descending;

            switch (SelectedSortOption)
            {
                case "Name":
                    CurrenciesView.SortDescriptions.Add(new SortDescription(nameof(Currency.Name), direction));
                    break;
                case "Price":
                    CurrenciesView.SortDescriptions.Add(new SortDescription(nameof(Currency.CurrentPrice), direction));
                    break;
                case "MarketCap":
                    CurrenciesView.SortDescriptions.Add(new SortDescription(nameof(Currency.MarketCap), direction));
                    break;
                default:
                    CurrenciesView.SortDescriptions.Add(new SortDescription(nameof(Currency.MarketCap), ListSortDirection.Descending));
                    break;
            }

            CurrenciesView.Refresh();
        }

        [RelayCommand]
        public void SortByColumn(string columnName)
        {
            if (SelectedSortOption == columnName)
            {
                IsAscendingSort = !IsAscendingSort;
            }
            else
            {
                SelectedSortOption = columnName;
                IsAscendingSort = columnName == "Name";
            }

            SortCurrenciesCommand.Execute(null);
        }

        [RelayCommand]
        public void NavigateToDetails()
        {
            if (SelectedCurrency != null)
            {
                ShellViewModel.CurrentNavService.NavigateTo("CurrencyDetailsView", SelectedCurrency.Id);
            }
        }

        public async void OnNavigatedTo(object parameter)
        {
            await LoadTopCurrenciesAsync();
        }
    }
}