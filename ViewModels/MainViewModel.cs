using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoTrackerApp.Infrastructure;
using CryptoTrackerApp.Models;
using CryptoTrackerApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace CryptoTrackerApp.ViewModels
{
    public partial class MainViewModel : BaseViewModel, INavigable
    {
        private readonly ICoinGeckoService _coinService;

        public ObservableCollection<Currency> Currencies { get; } = new();
        public ICollectionView CurrenciesView { get; }

        [ObservableProperty] private string searchQuery;
        [ObservableProperty] private string selectedSortOption = "MarketCap";
        [ObservableProperty] private Currency selectedCurrency;
        [ObservableProperty] private bool isAscendingSort;
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string statusMessage;

        public List<string> SortOptions { get; } =
            new() { "Name", "Price", "MarketCap", "Change24h", "Change24hPct" };

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
                foreach (var c in await _coinService.GetTopCurrenciesAsync(10))
                    Currencies.Add(c);

                SortCurrencies();
                StatusMessage = $"Loaded {Currencies.Count} currencies";
            }
            finally { IsLoading = false; }
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
                foreach (var c in await _coinService.SearchCurrenciesAsync(SearchQuery))
                    Currencies.Add(c);

                SortCurrencies();
                StatusMessage = $"Found {Currencies.Count} results";
            }
            finally { IsLoading = false; }
        }

        [RelayCommand] public void ClearSearch() => _ = LoadTopCurrenciesAsync();

        [RelayCommand]
        public void SortByColumn(string columnName)
        {
            if (SelectedSortOption == columnName)
                IsAscendingSort = !IsAscendingSort;
            else
            {
                SelectedSortOption = columnName;
                IsAscendingSort = columnName == "Name";
            }
            SortCurrencies();
        }

        [RelayCommand]
        public void NavigateToDetails()
        {
            if (SelectedCurrency != null)
                ShellViewModel.CurrentNavService.NavigateTo("CurrencyDetailsView", SelectedCurrency.Id);
        }

        [RelayCommand]
        public void SortCurrencies()
        {
            if (CurrenciesView == null) return;

            CurrenciesView.SortDescriptions.Clear();
            var dir = IsAscendingSort ? ListSortDirection.Ascending : ListSortDirection.Descending;

            switch (SelectedSortOption)
            {
                case "Name": CurrenciesView.SortDescriptions.Add(new(nameof(Currency.Name), dir)); break;
                case "Price": CurrenciesView.SortDescriptions.Add(new(nameof(Currency.CurrentPrice), dir)); break;
                case "MarketCap": CurrenciesView.SortDescriptions.Add(new(nameof(Currency.MarketCap), dir)); break;
                case "Change24h": CurrenciesView.SortDescriptions.Add(new(nameof(Currency.PriceChange24h), dir)); break;
                case "Change24hPct": CurrenciesView.SortDescriptions.Add(new(nameof(Currency.PriceChangePercentage24h), dir)); break;
            }

            CurrenciesView.Refresh();
        }
        public async void OnNavigatedTo(object parameter) => await LoadTopCurrenciesAsync();
    }
}
