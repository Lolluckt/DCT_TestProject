using System;
using System.Collections.Generic;
using System.Text.Json;
using CryptoTrackerApp.Models;
using OxyPlot;
using OxyPlot.Axes;

namespace CryptoTrackerApp.Services.Mappers
{
    public class OxyPlotChartMapper : IChartMapper
    {
        public List<DataPoint> MapToDataPoints(JsonElement root)
        {
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

        public List<CandleStickPoint> MapToCandleSticks(string json)
        {
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
    }
}
