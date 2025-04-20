using System.Collections.Generic;
using System.Text.Json;
using CryptoTrackerApp.Models;
using OxyPlot;

namespace CryptoTrackerApp.Services.Mappers
{
    public interface IChartMapper
    {
        List<DataPoint> MapToDataPoints(JsonElement root);
        List<CandleStickPoint> MapToCandleSticks(string json);
    }
}