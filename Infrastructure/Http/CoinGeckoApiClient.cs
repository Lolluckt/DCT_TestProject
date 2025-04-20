using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CryptoTrackerApp.Infrastructure.Http
{
    public class CoinGeckoApiClient : IApiClient
    {
        private readonly HttpClient _http;
        private readonly IRequestBuilder _requestBuilder;

        public CoinGeckoApiClient(HttpClient http, IConfiguration cfg)
        {
            _http = http;

            var baseUrl = cfg["CoinGecko:BaseUrl"] ?? "https://api.coingecko.com/api/v3/";
            var apiKey = cfg["CoinGecko:ApiKey"] ?? string.Empty;

            _requestBuilder = new CoinGeckoRequestBuilder(baseUrl, apiKey);
        }

        public async Task<JsonElement> GetAsync(string endpoint, Dictionary<string, string> parameters)
        {
            try
            {
                var url = _requestBuilder.BuildUrl(endpoint, parameters);
                using var rsp = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                rsp.EnsureSuccessStatusCode();

                await using var stream = await rsp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                return doc.RootElement.Clone();
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to retrieve data from CoinGecko API: {endpoint}", ex);
            }
        }

        public async Task<string> GetRawAsync(string endpoint, Dictionary<string, string> parameters)
        {
            try
            {
                var url = _requestBuilder.BuildUrl(endpoint, parameters);
                return await _http.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to retrieve raw data from CoinGecko API: {endpoint}", ex);
            }
        }
    }
}