using System.Net.Http;
using System.Text.Json;
using CryptoTrackerApp.Infrastructure.Http;
using CryptoTrackerApp.Models;
using CryptoTrackerApp.Services.Mappers;
using Microsoft.Extensions.Configuration;
using OxyPlot;
using OxyPlot.Axes;

namespace CryptoTrackerApp.Services
{
    public sealed class CoinGeckoService : ICoinGeckoService
    {
        private readonly IApiClient _apiClient;
        private readonly ICurrencyMapper _currencyMapper;
        private readonly IChartMapper _chartMapper;

        public CoinGeckoService(
            IApiClient apiClient,
            ICurrencyMapper currencyMapper,
            IChartMapper chartMapper)
        {
            _apiClient = apiClient;
            _currencyMapper = currencyMapper;
            _chartMapper = chartMapper;
        }

        public async Task<IEnumerable<Currency>> GetTopCurrenciesAsync(int count)
        {
            var parameters = new Dictionary<string, string>
            {
                ["vs_currency"] = "usd",
                ["order"] = "market_cap_desc",
                ["per_page"] = count.ToString(),
                ["page"] = "1",
                ["sparkline"] = "false"
            };

            var root = await _apiClient.GetAsync("coins/markets", parameters);
            return _currencyMapper.MapToCurrencyList(root);
        }

        public async Task<CurrencyDetails> GetCurrencyDetailsAsync(string coinId)
        {
            var parameters = new Dictionary<string, string>
            {
                ["localization"] = "false",
                ["tickers"] = "false",
                ["market_data"] = "true",
                ["community_data"] = "false",
                ["developer_data"] = "false",
                ["sparkline"] = "false"
            };

            var root = await _apiClient.GetAsync($"coins/{coinId}", parameters);
            return _currencyMapper.MapToCurrencyDetails(root);
        }

        public async Task<IEnumerable<Currency>> SearchCurrenciesAsync(string query)
        {
            var searchParameters = new Dictionary<string, string> { ["query"] = query };
            var searchRoot = await _apiClient.GetAsync("search", searchParameters);
            var ids = _currencyMapper.ExtractCoinIds(searchRoot);

            if (ids.Count == 0)
                return Array.Empty<Currency>();

            var marketsParameters = new Dictionary<string, string>
            {
                ["vs_currency"] = "usd",
                ["ids"] = string.Join(",", ids),
                ["order"] = "market_cap_desc",
                ["sparkline"] = "false"
            };

            var marketsRoot = await _apiClient.GetAsync("coins/markets", marketsParameters);
            return _currencyMapper.MapToCurrencyList(marketsRoot);
        }

        public async Task<List<DataPoint>> GetCoinMarketChartAsync(string coinId, int days)
        {
            var parameters = new Dictionary<string, string>
            {
                ["vs_currency"] = "usd",
                ["days"] = days.ToString()
            };

            var root = await _apiClient.GetAsync($"coins/{coinId}/market_chart", parameters);
            return _chartMapper.MapToDataPoints(root);
        }

        public async Task<List<CandleStickPoint>> GetCoinOHLCAsync(string coinId, int days)
        {
            var parameters = new Dictionary<string, string>
            {
                ["vs_currency"] = "usd",
                ["days"] = days.ToString()
            };

            var json = await _apiClient.GetRawAsync($"coins/{coinId}/ohlc", parameters);
            return _chartMapper.MapToCandleSticks(json);
        }

        public async Task<IEnumerable<Ticker>> GetCoinTickersAsync(string coinId, string baseSymbol)
        {
            var parameters = new Dictionary<string, string>
            {
                ["page"] = "1",
                ["per_page"] = "100"
            };

            var root = await _apiClient.GetAsync($"coins/{coinId}/tickers", parameters);
            return _currencyMapper.MapToTickers(root, baseSymbol);
        }
    }
}
