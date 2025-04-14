using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoTrackerApp.Infrastructure;
using CryptoTrackerApp.Models;
using CryptoTrackerApp.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Threading.Tasks;

namespace CryptoTrackerApp.ViewModels
{
    public partial class CurrencyDetailsViewModel : BaseViewModel, INavigable
    {
        private readonly ICoinGeckoService _coinService;

        [ObservableProperty]
        private CurrencyDetails details;

        [ObservableProperty]
        private PlotModel priceChart;

        [ObservableProperty]
        private bool isCandlestick;

        public CurrencyDetailsViewModel(ICoinGeckoService coinService)
        {
            _coinService = coinService;
            IsCandlestick = false;
        }

        [RelayCommand]
        public async Task ToggleChartStyleAsync()
        {
            if (Details == null)
                return;
            IsCandlestick = !IsCandlestick;
            await LoadChartAsync(Details.Id);
        }

        [RelayCommand]
        public void GoBack()
        {
            ShellViewModel.CurrentNavService.GoBack();
        }

        private async Task LoadChartAsync(string coinId)
        {
            try
            {
                PriceChart = new PlotModel();
                OnPropertyChanged(nameof(PriceChart));

                PlotModel newModel;

                if (!IsCandlestick)
                {
                    var dataPoints = await _coinService.GetCoinMarketChartAsync(coinId, 7);
                    newModel = new PlotModel { Title = $"{Details?.Name} Price (7 Days)" };

                    var dateAxis = new DateTimeAxis
                    {
                        Position = AxisPosition.Bottom,
                        StringFormat = "MM-dd",
                        Title = "Date",
                        IntervalType = DateTimeIntervalType.Days,
                        MinorIntervalType = DateTimeIntervalType.Hours
                    };
                    var valueAxis = new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        Title = "Price (USD)"
                    };
                    newModel.Axes.Add(dateAxis);
                    newModel.Axes.Add(valueAxis);

                    var lineSeries = new LineSeries
                    {
                        MarkerType = MarkerType.Circle,
                        Title = Details?.Name
                    };
                    if (dataPoints.Count > 1)
                        lineSeries.Color = dataPoints[^1].Y >= dataPoints[0].Y ? OxyColors.Green : OxyColors.Red;
                    lineSeries.Points.AddRange(dataPoints);
                    newModel.Series.Add(lineSeries);
                }
                else
                {
                    var candleData = await _coinService.GetCoinOHLCAsync(coinId, 7);
                    newModel = new PlotModel { Title = $"{Details?.Name} Candlestick (7 Days)" };

                    var dateAxis = new DateTimeAxis
                    {
                        Position = AxisPosition.Bottom,
                        StringFormat = "MM-dd",
                        Title = "Date",
                        IntervalType = DateTimeIntervalType.Days,
                        MinorIntervalType = DateTimeIntervalType.Hours
                    };
                    var valueAxis = new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        Title = "Price (USD)"
                    };
                    newModel.Axes.Add(dateAxis);
                    newModel.Axes.Add(valueAxis);

                    var candleSeries = new CandleStickSeries
                    {
                        IncreasingColor = OxyColors.Green,
                        DecreasingColor = OxyColors.Red
                    };
                    foreach (var c in candleData)
                    {
                        candleSeries.Items.Add(new HighLowItem(c.X, c.High, c.Low, c.Open, c.Close));
                    }
                    newModel.Series.Add(candleSeries);
                }
                PriceChart = newModel;
                OnPropertyChanged(nameof(PriceChart));
                PriceChart.InvalidatePlot(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error while loading plot: " + ex.Message);
            }
        }
        public async void OnNavigatedTo(object parameter)
        {
            if (parameter is string coinId)
            {
                try
                {
                    Details = await _coinService.GetCurrencyDetailsAsync(coinId);
                    await LoadChartAsync(coinId);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error while transfering: " + ex.Message);
                }
            }
        }
    }
}
