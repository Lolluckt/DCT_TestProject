using OxyPlot;
using OxyPlot.Series;

namespace CryptoTrackerApp.Models
{
    public class CandleStickPoint : HighLowItem
    {
        public CandleStickPoint(double x, double open, double high, double low, double close)
            : base(x, high, low, open, close)
        {
        }
    }
}