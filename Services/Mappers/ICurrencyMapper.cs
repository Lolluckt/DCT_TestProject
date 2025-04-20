using System.Collections.Generic;
using System.Text.Json;
using CryptoTrackerApp.Models;

namespace CryptoTrackerApp.Services.Mappers
{
    public interface ICurrencyMapper
    {
        IEnumerable<Currency> MapToCurrencyList(JsonElement root);
        CurrencyDetails MapToCurrencyDetails(JsonElement root);
        List<string> ExtractCoinIds(JsonElement root);
        IEnumerable<Ticker> MapToTickers(JsonElement root, string baseSymbol);
    }
}
