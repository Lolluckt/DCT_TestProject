using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoTrackerApp.Infrastructure;
using CryptoTrackerApp.Models;
using CryptoTrackerApp.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace CryptoTrackerApp.ViewModels
{
    public partial class CurrencyDetailsViewModel : BaseViewModel, INavigable
    {
        private readonly ICoinGeckoService _coinService;

        [ObservableProperty] private CurrencyDetails details;
        [ObservableProperty] private PlotModel priceChart;
        [ObservableProperty] private bool isCandlestick;
        [ObservableProperty] private bool isAscendingSort;

        public ObservableCollection<Ticker> Tickers { get; } = new();
        public ICollectionView TickersView { get; }

        public CurrencyDetailsViewModel(ICoinGeckoService coinService)
        {
            _coinService = coinService;
            IsCandlestick = false;
            IsAscendingSort = true;
            TickersView = CollectionViewSource.GetDefaultView(Tickers);
        }

        #region Computed properties
        public string DescriptionPlain =>
            string.IsNullOrWhiteSpace(Details?.Description)
                ? "No description provided."
                : Regex.Replace(Details.Description, "<.*?>", string.Empty).Trim();

        public string SortButtonText => IsAscendingSort ? "Price ▲" : "Price ▼";
        #endregion

        [RelayCommand]
        public void GoBack() => base.GoBack();

        [RelayCommand]
        private async Task ToggleChartStyleAsync()
        {
            if (Details == null) return;
            IsCandlestick = !IsCandlestick;
            await LoadChartAsync(Details.Id);
        }

        [RelayCommand]
        private void OpenTradeUrl(Ticker? ticker)
        {
            if (ticker == null || string.IsNullOrWhiteSpace(ticker.TradeUrl))
                return;

            OpenUrl(ticker.TradeUrl);
        }

        [RelayCommand]
        private void SortTickersByPrice()
        {
            IsAscendingSort = !IsAscendingSort;
            OnPropertyChanged(nameof(SortButtonText));

            TickersView.SortDescriptions.Clear();
            var direction = IsAscendingSort
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            TickersView.SortDescriptions.Add(
                new SortDescription(nameof(Ticker.LastPriceUsd), direction));

            TickersView.Refresh();
        }

        public async void OnNavigatedTo(object parameter)
        {
            if (parameter is string coinId)
            {
                await ExecuteWithLoadingAsync(async () =>
                {
                    Details = await _coinService.GetCurrencyDetailsAsync(coinId);
                    OnPropertyChanged(nameof(DescriptionPlain));

                    await LoadChartAsync(coinId);

                    Tickers.Clear();
                    var tickers = await _coinService.GetCoinTickersAsync(coinId, Details.Symbol);
                    foreach (var t in tickers)
                        Tickers.Add(t);

                    SortTickersByPrice();
                }, $"Loading {coinId} details...");
            }
        }

        private async Task LoadChartAsync(string coinId)
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                PlotModel model;

                if (!IsCandlestick)
                {
                    var data = await _coinService.GetCoinMarketChartAsync(coinId, 7);

                    model = new PlotModel { Title = $"{Details?.Name} Price (7 Days)" };
                    model.Axes.Add(new DateTimeAxis
                    {
                        Position = AxisPosition.Bottom,
                        StringFormat = "MM-dd",
                        Title = "Date",
                        IntervalType = DateTimeIntervalType.Days
                    });
                    model.Axes.Add(new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        Title = "Price (USD)"
                    });

                    var line = new LineSeries
                    {
                        MarkerType = MarkerType.Circle,
                        Color = data[^1].Y >= data[0].Y ? OxyColors.Green : OxyColors.Red
                    };
                    line.Points.AddRange(data);
                    model.Series.Add(line);
                }
                else
                {
                    var candles = await _coinService.GetCoinOHLCAsync(coinId, 7);

                    model = new PlotModel { Title = $"{Details?.Name} Candlestick (7 Days)" };
                    model.Axes.Add(new DateTimeAxis
                    {
                        Position = AxisPosition.Bottom,
                        StringFormat = "MM-dd",
                        Title = "Date",
                        IntervalType = DateTimeIntervalType.Days
                    });
                    model.Axes.Add(new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        Title = "Price (USD)"
                    });

                    var series = new CandleStickSeries
                    {
                        IncreasingColor = OxyColors.Green,
                        DecreasingColor = OxyColors.Red
                    };
                    foreach (var c in candles)
                        series.Items.Add(new HighLowItem(c.X, c.High, c.Low, c.Open, c.Close));

                    model.Series.Add(series);
                }

                PriceChart = model;
                PriceChart.InvalidatePlot(true);
            }, "Loading chart data...");
        }
    }
}