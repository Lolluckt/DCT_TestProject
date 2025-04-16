using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CryptoTrackerApp.Infrastructure.Http;
using CryptoTrackerApp.Models;
using Microsoft.Extensions.Configuration;
using OxyPlot;
using OxyPlot.Axes;

namespace CryptoTrackerApp.Services
{
    public sealed class CoinGeckoService : ICoinGeckoService
    {
        private readonly HttpClient _http;
        private readonly IRequestBuilder _requestBuilder;

        public CoinGeckoService(HttpClient http, IConfiguration cfg)
        {
            _http = http;

            var baseUrl = cfg["CoinGecko:BaseUrl"] ?? "https://api.coingecko.com/api/v3/";
            var apiKey = cfg["CoinGecko:ApiKey"] ?? string.Empty;

            _requestBuilder = new CoinGeckoRequestBuilder(baseUrl, apiKey);
        }

        #region Public API
        public async Task<IEnumerable<Currency>> GetTopCurrenciesAsync(int count)
        {
            var url = _requestBuilder.BuildUrl("coins/markets", new()
            {
                ["vs_currency"] = "usd",
                ["order"] = "market_cap_desc",
                ["per_page"] = count.ToString(),
                ["page"] = "1",
                ["sparkline"] = "false"
            });

            var root = await GetJsonRootAsync(url);
            var list = new List<Currency>();

            foreach (var item in root.EnumerateArray())
            {
                list.Add(new Currency
                {
                    Id = item.GetProperty("id").GetString(),
                    Symbol = item.GetProperty("symbol").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    CurrentPrice = item.GetProperty("current_price").GetDecimal(),
                    MarketCap = item.GetProperty("market_cap").GetDecimal(),
                    PriceChangePercentage24h = item.GetProperty("price_change_percentage_24h").GetDecimal(),
                    PriceChange24h = item.GetProperty("price_change_24h").GetDecimal()
                });
            }
            return list;
        }

        public async Task<CurrencyDetails> GetCurrencyDetailsAsync(string coinId)
        {
            var url = _requestBuilder.BuildUrl($"coins/{coinId}", new()
            {
                ["localization"] = "false",
                ["tickers"] = "false",
                ["market_data"] = "true",
                ["community_data"] = "false",
                ["developer_data"] = "false",
                ["sparkline"] = "false"
            });

            var root = await GetJsonRootAsync(url);
            var market = root.GetProperty("market_data");

            var details = new CurrencyDetails
            {
                Id = root.GetProperty("id").GetString(),
                Symbol = root.GetProperty("symbol").GetString(),
                Name = root.GetProperty("name").GetString(),
                CurrentPrice = GetNestedDecimal(market, "current_price", "usd"),
                MarketCap = GetNestedDecimal(market, "market_cap", "usd"),
                TotalVolume = GetNestedDecimal(market, "total_volume", "usd"),
                PriceChangePercentage24h = GetNestedDecimal(market, "price_change_percentage_24h_in_currency", "usd"),
                PriceChange24h = market.GetProperty("price_change_24h").GetDecimal()
            };

            if (root.TryGetProperty("description", out var desc) &&
                desc.TryGetProperty("en", out var en))
            {
                details.Description = en.GetString();
            }

            return details;
        }

        public async Task<IEnumerable<Currency>> SearchCurrenciesAsync(string query)
        {
            var url = _requestBuilder.BuildUrl("search", new() { ["query"] = query });
            var root = await GetJsonRootAsync(url);

            var list = new List<Currency>();
            if (root.TryGetProperty("coins", out var coins))
            {
                foreach (var c in coins.EnumerateArray())
                {
                    list.Add(new Currency
                    {
                        Id = c.GetProperty("id").GetString(),
                        Name = c.GetProperty("name").GetString(),
                        Symbol = c.GetProperty("symbol").GetString()
                    });
                }
            }
            return list;
        }

        public async Task<List<DataPoint>> GetCoinMarketChartAsync(string coinId, int days)
        {
            var url = _requestBuilder.BuildUrl($"coins/{coinId}/market_chart", new()
            {
                ["vs_currency"] = "usd",
                ["days"] = days.ToString()
            });

            var root = await GetJsonRootAsync(url);
            var points = new List<DataPoint>();

            if (root.TryGetProperty("prices", out var prices))
            {
                foreach (var p in prices.EnumerateArray())
                {
                    var ts = p[0].GetDouble();
                    var price = p[1].GetDouble();
                    var dt = DateTimeOffset.FromUnixTimeMilliseconds((long)ts).UtcDateTime;
                    points.Add(new DataPoint(DateTimeAxis.ToDouble(dt), price));
                }
            }
            return points;
        }

        public async Task<List<CandleStickPoint>> GetCoinOHLCAsync(string coinId, int days)
        {
            var url = _requestBuilder.BuildUrl($"coins/{coinId}/ohlc", new()
            {
                ["vs_currency"] = "usd",
                ["days"] = days.ToString()
            });

            var json = await _http.GetStringAsync(url);
            var raw = JsonSerializer.Deserialize<List<List<double>>>(json);

            var candles = new List<CandleStickPoint>();
            if (raw is { Count: > 0 })
            {
                foreach (var arr in raw)
                {
                    var dt = DateTimeOffset.FromUnixTimeMilliseconds((long)arr[0]).UtcDateTime;
                    candles.Add(new CandleStickPoint(
                        DateTimeAxis.ToDouble(dt),
                        arr[1], arr[2], arr[3], arr[4]));
                }
            }
            return candles;
        }
        #endregion

        #region Helpers
        private async Task<JsonElement> GetJsonRootAsync(string url)
        {
            try
            {
                using var rsp = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                rsp.EnsureSuccessStatusCode();

                await using var stream = await rsp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                return doc.RootElement.Clone();
            }
            catch (Exception ex)
            {
                throw new ApiException("Failed to retrieve data from CoinGecko API", ex);
            }
        }

        private static decimal GetNestedDecimal(JsonElement root, string obj, string prop) =>
            root.TryGetProperty(obj, out var o) &&
            o.TryGetProperty(prop, out var p)
                ? p.GetDecimal()
                : 0m;
        #endregion
    }
}
