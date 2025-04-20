using System;
using System.Collections.Generic;
using System.Text.Json;
using CryptoTrackerApp.Models;

namespace CryptoTrackerApp.Services.Mappers
{
    public class CoinGeckoCurrencyMapper : ICurrencyMapper
    {
        public IEnumerable<Currency> MapToCurrencyList(JsonElement root)
        {
            var list = new List<Currency>();

            foreach (var item in root.EnumerateArray())
            {
                list.Add(new Currency
                {
                    Id = item.GetProperty("id").GetString(),
                    Symbol = item.GetProperty("symbol").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    CurrentPrice = GetSafeDecimal(item, "current_price"),
                    MarketCap = GetSafeDecimal(item, "market_cap"),
                    PriceChangePercentage24h = GetSafeDecimal(item, "price_change_percentage_24h"),
                    PriceChange24h = GetSafeDecimal(item, "price_change_24h")
                });
            }
            return list;
        }

        public CurrencyDetails MapToCurrencyDetails(JsonElement root)
        {
            var market = root.GetProperty("market_data");

            var details = new CurrencyDetails
            {
                Id = root.GetProperty("id").GetString(),
                Symbol = root.GetProperty("symbol").GetString(),
                Name = root.GetProperty("name").GetString(),
                CurrentPrice = GetNestedDecimal(market, "current_price", "usd"),
                MarketCap = GetNestedDecimal(market, "market_cap", "usd"),
                TotalVolume = GetNestedDecimal(market, "total_volume", "usd"),
                PriceChangePercentage24h = GetSafeDecimal(market, "price_change_percentage_24h"),
                PriceChange24h = GetSafeDecimal(market, "price_change_24h")
            };

            if (root.TryGetProperty("description", out var desc) &&
                desc.TryGetProperty("en", out var en))
            {
                details.Description = en.GetString();
            }
            return details;
        }

        public List<string> ExtractCoinIds(JsonElement root)
        {
            var ids = new List<string>();

            if (root.TryGetProperty("coins", out var coins))
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
            return ids;
        }

        public IEnumerable<Ticker> MapToTickers(JsonElement root, string baseSymbol)
        {
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

        private static decimal GetSafeDecimal(JsonElement element, string property)
        {
            if (element.TryGetProperty(property, out var prop) && prop.ValueKind == JsonValueKind.Number)
                return prop.GetDecimal();
            return 0m;
        }

        private static decimal GetNestedDecimal(JsonElement root, string obj, string prop) =>
            root.TryGetProperty(obj, out var o) &&
            o.TryGetProperty(prop, out var p)
                ? p.GetDecimal()
                : 0m;
    }
}
