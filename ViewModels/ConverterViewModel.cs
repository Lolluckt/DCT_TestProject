using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoTrackerApp.Infrastructure;
using CryptoTrackerApp.Models;
using CryptoTrackerApp.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CryptoTrackerApp.ViewModels
{
    public partial class ConverterViewModel : BaseViewModel, INavigable
    {
        private readonly ICoinGeckoService _coinService;
        private readonly CollectionViewSource _sourceViewSource;
        private readonly CollectionViewSource _targetViewSource;

        [ObservableProperty] private ObservableCollection<Currency> sourceCurrencies = new();
        [ObservableProperty] private ObservableCollection<Currency> targetCurrencies = new();
        [ObservableProperty] private Currency selectedSourceCurrency;
        [ObservableProperty] private Currency selectedTargetCurrency;
        [ObservableProperty] private decimal amountToConvert = 1;
        [ObservableProperty] private decimal convertedAmount;
        [ObservableProperty] private string conversionRate;
        [ObservableProperty] private Currency previewCurrency;
        [ObservableProperty] private string searchQuery;

        public ICollectionView FilteredSourceCurrencies => _sourceViewSource?.View;
        public ICollectionView FilteredTargetCurrencies => _targetViewSource?.View;

        public ConverterViewModel(ICoinGeckoService coinService)
        {
            _coinService = coinService;

            _sourceViewSource = new CollectionViewSource { Source = SourceCurrencies };
            _targetViewSource = new CollectionViewSource { Source = TargetCurrencies };

            _sourceViewSource.Filter += FilterCurrencies;
            _targetViewSource.Filter += FilterCurrencies;

            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(SearchQuery))
                {
                    _sourceViewSource.View.Refresh();
                    _targetViewSource.View.Refresh();
                }
            };
        }

        private void FilterCurrencies(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchQuery) || e.Item is not Currency currency)
            {
                e.Accepted = true;
                return;
            }

            e.Accepted =
                currency.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                currency.Symbol.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase);
        }

        public async void OnNavigatedTo(object parameter) => await LoadCurrenciesAsync();

        [RelayCommand]
        private async Task LoadCurrenciesAsync()
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                string sourceSymbol = SelectedSourceCurrency?.Symbol;
                string targetSymbol = SelectedTargetCurrency?.Symbol;
                decimal amount = AmountToConvert;

                var currencies = await _coinService.GetTopCurrenciesAsync(100);
                var newSourceCurrencies = new List<Currency>(currencies);
                var newTargetCurrencies = new List<Currency>(currencies);

                var newSourceCurrency = !string.IsNullOrEmpty(sourceSymbol)
                    ? newSourceCurrencies.FirstOrDefault(c => c.Symbol?.Equals(sourceSymbol, StringComparison.OrdinalIgnoreCase) == true)
                    : newSourceCurrencies.FirstOrDefault(c => c.Symbol?.Equals("btc", StringComparison.OrdinalIgnoreCase) == true)
                      ?? newSourceCurrencies.FirstOrDefault();

                var newTargetCurrency = !string.IsNullOrEmpty(targetSymbol)
                    ? newTargetCurrencies.FirstOrDefault(c => c.Symbol?.Equals(targetSymbol, StringComparison.OrdinalIgnoreCase) == true)
                    : newTargetCurrencies.FirstOrDefault(c => c.Symbol?.Equals("eth", StringComparison.OrdinalIgnoreCase) == true)
                      ?? newTargetCurrencies.FirstOrDefault();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SelectedSourceCurrency = null;
                    SelectedTargetCurrency = null;
                    SourceCurrencies.Clear();
                    TargetCurrencies.Clear();

                    foreach (var c in newSourceCurrencies)
                    {
                        SourceCurrencies.Add(c);
                    }

                    foreach (var c in newTargetCurrencies)
                    {
                        TargetCurrencies.Add(c);
                    }
                    _sourceViewSource.View.Refresh();
                    _targetViewSource.View.Refresh();
                    SelectedSourceCurrency = newSourceCurrency;
                    SelectedTargetCurrency = newTargetCurrency;

                    if (amount > 0)
                        AmountToConvert = amount;
                });
                await ConvertCurrencyAsync();

            }, "Loading currencies...");
        }

        partial void OnSelectedSourceCurrencyChanged(Currency value)
        {
            PreviewCurrency = value;
            _ = ConvertCurrencyAsync();
        }

        partial void OnSelectedTargetCurrencyChanged(Currency value) => _ = ConvertCurrencyAsync();

        partial void OnAmountToConvertChanged(decimal value) => _ = ConvertCurrencyAsync();

        [RelayCommand]
        private async Task ConvertCurrencyAsync()
        {
            if (SelectedSourceCurrency == null || SelectedTargetCurrency == null || AmountToConvert <= 0)
            {
                ConvertedAmount = 0;
                ConversionRate = "N/A";
                return;
            }

            await ExecuteWithLoadingAsync(() =>
            {
                decimal sourceToUsd = SelectedSourceCurrency.CurrentPrice;
                decimal targetToUsd = SelectedTargetCurrency.CurrentPrice;

                if (sourceToUsd <= 0 || targetToUsd <= 0)
                {
                    ConvertedAmount = 0;
                    ConversionRate = "Unable to calculate rate";
                    return Task.CompletedTask;
                }

                decimal rate = sourceToUsd / targetToUsd;
                ConvertedAmount = AmountToConvert * rate;
                ConversionRate =
                    $"1 {SelectedSourceCurrency.Symbol?.ToUpper()} = {rate:N8} {SelectedTargetCurrency.Symbol?.ToUpper()}";
                return Task.CompletedTask;
            }, "Converting...");
        }

        [RelayCommand]
        private void SwapCurrencies()
        {
            var tmp = SelectedSourceCurrency;
            SelectedSourceCurrency = SelectedTargetCurrency;
            SelectedTargetCurrency = tmp;
        }
    }
}
