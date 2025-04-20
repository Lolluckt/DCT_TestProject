using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTrackerApp.Models;
using OxyPlot;

namespace CryptoTrackerApp.Services
{
    public interface ICoinGeckoService
    {
        Task<IEnumerable<Currency>> GetTopCurrenciesAsync(int count);
        Task<CurrencyDetails> GetCurrencyDetailsAsync(string coinId);
        Task<IEnumerable<Currency>> SearchCurrenciesAsync(string query);
        Task<List<DataPoint>> GetCoinMarketChartAsync(string coinId, int days);
        Task<List<CandleStickPoint>> GetCoinOHLCAsync(string coinId, int days);
        Task<IEnumerable<Ticker>> GetCoinTickersAsync(string coinId, string baseSymbol);
    }
}
