using System.Net.Http;
using System.Text.Json;
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

        #region ── Public API 
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
                PriceChangePercentage24h = market.GetProperty("price_change_percentage_24h").GetDecimal(),
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
            var searchUrl = _requestBuilder.BuildUrl("search", new() { ["query"] = query });
            var searchRoot = await GetJsonRootAsync(searchUrl);
            var ids = new List<string>();

            if (searchRoot.TryGetProperty("coins", out var coins))
            {
                foreach (var c in coins.EnumerateArray())
                {
                    if (c.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                    {
                        var id = idProp.GetString();
                        if (!string.IsNullOrEmpty(id))
                            ids.Add(id);
                    }
                }
            }
            if (ids.Count == 0)
                return Array.Empty<Currency>();

            var marketsUrl = _requestBuilder.BuildUrl("coins/markets", new()
            {
                ["vs_currency"] = "usd",
                ["ids"] = string.Join(",", ids),
                ["order"] = "market_cap_desc",
                ["sparkline"] = "false"
            });
            var marketsRoot = await GetJsonRootAsync(marketsUrl);

            var result = new List<Currency>();
            foreach (var item in marketsRoot.EnumerateArray())
            {
                decimal currentPrice = 0m;
                if (item.TryGetProperty("current_price", out var cp) && cp.ValueKind == JsonValueKind.Number)
                    currentPrice = cp.GetDecimal();

                decimal marketCap = 0m;
                if (item.TryGetProperty("market_cap", out var mc) && mc.ValueKind == JsonValueKind.Number)
                    marketCap = mc.GetDecimal();

                decimal priceChange24h = 0m;
                if (item.TryGetProperty("price_change_24h", out var pc) && pc.ValueKind == JsonValueKind.Number)
                    priceChange24h = pc.GetDecimal();

                decimal priceChangePct = 0m;
                if (item.TryGetProperty("price_change_percentage_24h", out var pcp) && pcp.ValueKind == JsonValueKind.Number)
                    priceChangePct = pcp.GetDecimal();

                result.Add(new Currency
                {
                    Id = item.GetProperty("id").GetString(),
                    Symbol = item.GetProperty("symbol").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    CurrentPrice = currentPrice,
                    MarketCap = marketCap,
                    PriceChange24h = priceChange24h,
                    PriceChangePercentage24h = priceChangePct
                });
            }

            return result;
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

        public async Task<IEnumerable<Ticker>> GetCoinTickersAsync(string coinId, string baseSymbol)
        {
            var url = _requestBuilder.BuildUrl($"coins/{coinId}/tickers", new()
            {
                ["page"] = "1",
                ["per_page"] = "100"
            });

            var root = await GetJsonRootAsync(url);
            var list = new List<Ticker>();

            if (root.TryGetProperty("tickers", out var arr))
            {
                foreach (var item in arr.EnumerateArray())
                {
                    var baseSym = item.GetProperty("base").GetString();
                    if (!string.Equals(baseSym, baseSymbol, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var target = item.GetProperty("target").GetString();
                    var marketName = item.GetProperty("market").GetProperty("name").GetString() ?? string.Empty;
                    decimal lastUsd = 0m;
                    if (item.TryGetProperty("converted_last", out var conv) && conv.TryGetProperty("usd", out var usd))
                        lastUsd = usd.GetDecimal();

                    var tradeUrl = item.GetProperty("trade_url").GetString() ?? string.Empty;

                    list.Add(new Ticker
                    {
                        MarketName = marketName,
                        Base = baseSym ?? string.Empty,
                        Target = target ?? string.Empty,
                        LastPriceUsd = lastUsd,
                        TradeUrl = tradeUrl
                    });
                }
            }
            return list;
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
