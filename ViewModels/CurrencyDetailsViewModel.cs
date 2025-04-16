using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoTrackerApp.Infrastructure;
using CryptoTrackerApp.Models;
using CryptoTrackerApp.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Text.RegularExpressions;

namespace CryptoTrackerApp.ViewModels
{
    public partial class CurrencyDetailsViewModel : BaseViewModel, INavigable
    {
        private readonly ICoinGeckoService _coinService;

        [ObservableProperty] private CurrencyDetails details;
        [ObservableProperty] private PlotModel priceChart;
        [ObservableProperty] private bool isCandlestick;

        public CurrencyDetailsViewModel(ICoinGeckoService coinService)
        {
            _coinService = coinService;
            IsCandlestick = false;
        }

        public string DescriptionPlain =>
            string.IsNullOrWhiteSpace(Details?.Description)
                ? "No description provided."
                : Regex.Replace(Details.Description, "<.*?>", string.Empty).Trim();
        [RelayCommand] public void GoBack() => ShellViewModel.CurrentNavService.GoBack();

        [RelayCommand]
        public async Task ToggleChartStyleAsync()
        {
            if (Details == null) return;
            IsCandlestick = !IsCandlestick;
            await LoadChartAsync(Details.Id);
        }
        public async void OnNavigatedTo(object parameter)
        {
            if (parameter is string coinId)
            {
                try
                {
                    Details = await _coinService.GetCurrencyDetailsAsync(coinId);
                    OnPropertyChanged(nameof(DescriptionPlain));
                    await LoadChartAsync(coinId);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Navigation error: " + ex.Message);
                }
            }
        }
        private async Task LoadChartAsync(string coinId)
        {
            try
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Plot error: " + ex.Message);
            }
        }
    }
}
