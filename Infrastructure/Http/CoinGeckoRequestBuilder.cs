using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrackerApp.Infrastructure.Http
{
    public sealed class CoinGeckoRequestBuilder : IRequestBuilder
    {
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public CoinGeckoRequestBuilder(string baseUrl, string apiKey)
        {
            _baseUrl = baseUrl.EndsWith('/') ? baseUrl : baseUrl + '/';
            _apiKey = apiKey;
        }

        public string BuildUrl(string endpoint, Dictionary<string, string>? parameters = null)
        {
            var sb = new StringBuilder(_baseUrl);

            if (endpoint.StartsWith('/')) endpoint = endpoint[1..];
            sb.Append(endpoint);

            var hasQuery = false;
            if (parameters is { Count: > 0 })
            {
                sb.Append('?');
                hasQuery = true;

                var first = true;
                foreach (var p in parameters)
                {
                    if (!first) sb.Append('&');
                    sb.Append(Uri.EscapeDataString(p.Key))
                      .Append('=')
                      .Append(Uri.EscapeDataString(p.Value));
                    first = false;
                }
            }

            if (!string.IsNullOrEmpty(_apiKey))
            {
                sb.Append(hasQuery ? '&' : '?')
                  .Append("x_cg_demo_api_key=")
                  .Append(Uri.EscapeDataString(_apiKey));
            }

            return sb.ToString();
        }
    }
}
