using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoTrackerApp.Infrastructure;
using CryptoTrackerApp.Models;
using CryptoTrackerApp.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CryptoTrackerApp.ViewModels
{
    public partial class MainViewModel : BaseViewModel, INavigable
    {
        private readonly ICoinGeckoService _coinService;

        public ObservableCollection<Currency> Currencies { get; } = new();
        public ICollectionView CurrenciesView { get; }

        public List<string> SortOptions { get; } =
            new() { "Name", "Price", "MarketCap", "Change24h", "Change24hPct" };

        [ObservableProperty]
        private string searchQuery;

        [ObservableProperty]
        private string selectedSortOption = "MarketCap";

        [ObservableProperty]
        private bool isAscendingSort;

        [ObservableProperty]
        private Currency selectedCurrency;

        public MainViewModel(ICoinGeckoService coinService)
        {
            _coinService = coinService;
            CurrenciesView = CollectionViewSource.GetDefaultView(Currencies);
            IsAscendingSort = SelectedSortOption == "Name";
        }

        partial void OnSelectedSortOptionChanged(string oldValue, string newValue)
        {
            IsAscendingSort = newValue == "Name";
            SortCurrencies();
        }

        partial void OnIsAscendingSortChanged(bool oldValue, bool newValue)
        {
            SortCurrencies();
        }

        [RelayCommand]
        public async Task LoadTopCurrenciesAsync()
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                Currencies.Clear();
                foreach (var c in await _coinService.GetTopCurrenciesAsync(10))
                    Currencies.Add(c);
            }, "Loading top currencies...", $"Loaded {Currencies.Count} currencies");
        }

        [RelayCommand]
        public async Task SearchCurrenciesAsync()
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(SearchQuery))
                {
                    await LoadTopCurrenciesAsync();
                    return;
                }

                Currencies.Clear();
                foreach (var c in await _coinService.SearchCurrenciesAsync(SearchQuery))
                    Currencies.Add(c);
            }, $"Searching for '{SearchQuery}'...", $"Found {Currencies.Count} results");
        }

        [RelayCommand]
        public void SortCurrencies()
        {
            if (CurrenciesView == null) return;

            CurrenciesView.SortDescriptions.Clear();
            var dir = IsAscendingSort ? ListSortDirection.Ascending : ListSortDirection.Descending;

            switch (SelectedSortOption)
            {
                case "Name":
                    CurrenciesView.SortDescriptions.Add(new SortDescription(nameof(Currency.Name), dir));
                    break;
                case "Price":
                    CurrenciesView.SortDescriptions.Add(new SortDescription(nameof(Currency.CurrentPrice), dir));
                    break;
                case "MarketCap":
                    CurrenciesView.SortDescriptions.Add(new SortDescription(nameof(Currency.MarketCap), dir));
                    break;
                case "Change24h":
                    CurrenciesView.SortDescriptions.Add(new SortDescription(nameof(Currency.PriceChange24h), dir));
                    break;
                case "Change24hPct":
                    CurrenciesView.SortDescriptions.Add(new SortDescription(nameof(Currency.PriceChangePercentage24h), dir));
                    break;
                default:
                    CurrenciesView.SortDescriptions.Add(new SortDescription(nameof(Currency.MarketCap), ListSortDirection.Descending));
                    break;
            }

            CurrenciesView.Refresh();
        }

        [RelayCommand]
        public void NavigateToDetails()
        {
            if (SelectedCurrency != null)
                NavigateTo("CurrencyDetailsView", SelectedCurrency.Id);
        }

        public async void OnNavigatedTo(object parameter)
        {
            await LoadTopCurrenciesAsync();
        }
    }
}