using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CryptoTrackerApp.Models;
using Microsoft.Extensions.Configuration;
using OxyPlot;
using OxyPlot.Axes;

namespace CryptoTrackerApp.Services
{
    public class CoinGeckoService : ICoinGeckoService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly IRequestBuilder _requestBuilder;

        public CoinGeckoService(IConfiguration config, HttpClient httpClient = null)
        {
            _http = httpClient ?? new HttpClient();
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("CryptoTrackerApp/1.0");
            _baseUrl = config["CoinGecko:BaseUrl"] ?? "https://api.coingecko.com/api/v3/";
            _apiKey = config["CoinGecko:ApiKey"];
            _requestBuilder = new CoinGeckoRequestBuilder(_baseUrl, _apiKey);
        }

        public async Task<IEnumerable<Currency>> GetTopCurrenciesAsync(int count)
        {
            var url = _requestBuilder.BuildUrl($"coins/markets", new Dictionary<string, string>
            {
                ["vs_currency"] = "usd",
                ["order"] = "market_cap_desc",
                ["per_page"] = count.ToString(),
                ["page"] = "1",
                ["sparkline"] = "false"
            });

            var response = await ExecuteRequestAsync(url);

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            var list = new List<Currency>();

            foreach (var item in root.EnumerateArray())
            {
                var c = new Currency
                {
                    Id = GetJsonPropertyString(item, "id"),
                    Symbol = GetJsonPropertyString(item, "symbol"),
                    Name = GetJsonPropertyString(item, "name"),
                    CurrentPrice = GetJsonPropertyDecimal(item, "current_price"),
                    MarketCap = GetJsonPropertyDecimal(item, "market_cap"),
                    PriceChangePercentage24h = GetJsonPropertyDecimal(item, "price_change_percentage_24h")
                };
                list.Add(c);
            }
            return list;
        }

        public async Task<CurrencyDetails> GetCurrencyDetailsAsync(string coinId)
        {
            var url = _requestBuilder.BuildUrl($"coins/{coinId}", new Dictionary<string, string>
            {
                ["localization"] = "false",
                ["tickers"] = "false",
                ["market_data"] = "true",
                ["community_data"] = "false",
                ["developer_data"] = "false",
                ["sparkline"] = "false"
            });

            var response = await ExecuteRequestAsync(url);

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            var details = new CurrencyDetails
            {
                Id = GetJsonPropertyString(root, "id"),
                Symbol = GetJsonPropertyString(root, "symbol"),
                Name = GetJsonPropertyString(root, "name")
            };

            if (root.TryGetProperty("market_data", out var marketData))
            {
                details.CurrentPrice = GetNestedJsonPropertyDecimal(marketData, "current_price", "usd");
                details.MarketCap = GetNestedJsonPropertyDecimal(marketData, "market_cap", "usd");
                details.TotalVolume = GetNestedJsonPropertyDecimal(marketData, "total_volume", "usd");
                details.PriceChangePercentage24h = GetJsonPropertyDecimal(marketData, "price_change_percentage_24h");
            }

            if (root.TryGetProperty("description", out var desc) && desc.TryGetProperty("en", out var enDesc))
            {
                details.Description = enDesc.GetString();
            }

            return details;
        }

        public async Task<IEnumerable<Currency>> SearchCurrenciesAsync(string query)
        {
            var url = _requestBuilder.BuildUrl("search", new Dictionary<string, string>
            {
                ["query"] = query
            });

            var response = await ExecuteRequestAsync(url);

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            var list = new List<Currency>();

            if (root.TryGetProperty("coins", out var coinsElem))
            {
                foreach (var coinElem in coinsElem.EnumerateArray())
                {
                    var c = new Currency
                    {
                        Id = GetJsonPropertyString(coinElem, "id"),
                        Name = GetJsonPropertyString(coinElem, "name"),
                        Symbol = GetJsonPropertyString(coinElem, "symbol"),
                        CurrentPrice = 0,
                        MarketCap = 0,
                        PriceChangePercentage24h = 0
                    };
                    list.Add(c);
                }
            }
            return list;
        }

        public async Task<List<DataPoint>> GetCoinMarketChartAsync(string coinId, int days)
        {
            var url = _requestBuilder.BuildUrl($"coins/{coinId}/market_chart", new Dictionary<string, string>
            {
                ["vs_currency"] = "usd",
                ["days"] = days.ToString()
            });

            var response = await ExecuteRequestAsync(url);

            var dataPoints = new List<DataPoint>();
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (root.TryGetProperty("prices", out var prices))
            {
                foreach (var item in prices.EnumerateArray())
                {
                    if (item.GetArrayLength() >= 2)
                    {
                        var timestamp = item[0].GetDouble();
                        var price = item[1].GetDouble();
                        var dt = DateTimeOffset.FromUnixTimeMilliseconds((long)timestamp).DateTime;
                        dataPoints.Add(new DataPoint(DateTimeAxis.ToDouble(dt), price));
                    }
                }
            }
            return dataPoints;
        }

        public async Task<List<CandleStickPoint>> GetCoinOHLCAsync(string coinId, int days)
        {
            var url = _requestBuilder.BuildUrl($"coins/{coinId}/ohlc", new Dictionary<string, string>
            {
                ["vs_currency"] = "usd",
                ["days"] = days.ToString()
            });

            var response = await ExecuteRequestAsync(url);

            var data = JsonSerializer.Deserialize<List<List<double>>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var candles = new List<CandleStickPoint>();

            if (data != null)
            {
                foreach (var arr in data)
                {
                    if (arr.Count >= 5)
                    {
                        var ts = (long)arr[0];
                        var open = arr[1];
                        var high = arr[2];
                        var low = arr[3];
                        var close = arr[4];
                        var dt = DateTimeOffset.FromUnixTimeMilliseconds(ts).DateTime;
                        var xVal = DateTimeAxis.ToDouble(dt);
                        candles.Add(new CandleStickPoint(xVal, open, high, low, close));
                    }
                }
            }
            return candles;
        }

        #region Helper Methods

        private async Task<string> ExecuteRequestAsync(string url)
        {
            try
            {
                var response = await _http.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Request failed: {ex.Message}");
                throw new ApiException("Failed to retrieve data from CoinGecko API", ex);
            }
        }

        private string GetJsonPropertyString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() ?? string.Empty : string.Empty;
        }

        private decimal GetJsonPropertyDecimal(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null ? prop.GetDecimal() : 0;
        }

        private decimal GetNestedJsonPropertyDecimal(JsonElement element, string property1, string property2)
        {
            if (element.TryGetProperty(property1, out var prop1) &&
                prop1.TryGetProperty(property2, out var prop2) &&
                prop2.ValueKind != JsonValueKind.Null)
            {
                return prop2.GetDecimal();
            }
            return 0;
        }

        #endregion
    }
    public interface IRequestBuilder
    {
        string BuildUrl(string endpoint, Dictionary<string, string> parameters = null);
    }
    public class CoinGeckoRequestBuilder : IRequestBuilder
    {
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public CoinGeckoRequestBuilder(string baseUrl, string apiKey)
        {
            _baseUrl = baseUrl;
            _apiKey = apiKey;
        }

        public string BuildUrl(string endpoint, Dictionary<string, string> parameters = null)
        {
            var sb = new StringBuilder();
            sb.Append(_baseUrl);
            if (_baseUrl.EndsWith("/") && endpoint.StartsWith("/"))
                endpoint = endpoint.Substring(1);
            else if (!_baseUrl.EndsWith("/") && !endpoint.StartsWith("/"))
                sb.Append("/");

            sb.Append(endpoint);
            if (parameters != null && parameters.Count > 0)
            {
                sb.Append("?");
                var isFirst = true;
                foreach (var param in parameters)
                {
                    if (!isFirst)
                        sb.Append("&");

                    sb.Append($"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}");
                    isFirst = false;
                }
            }
            if (!string.IsNullOrEmpty(_apiKey))
            {
                sb.Append(parameters != null && parameters.Count > 0 ? "&" : "?");
                sb.Append($"x_cg_demo_api_key={_apiKey}");
            }

            return sb.ToString();
        }
    }
    public class ApiException : Exception
    {
        public ApiException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}