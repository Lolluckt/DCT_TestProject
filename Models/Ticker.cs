namespace CryptoTrackerApp.Models
{
    public class Ticker
    {
        public string MarketName { get; set; }
        public string Base { get; set; }
        public string Target { get; set; }
        public decimal LastPriceUsd { get; set; }
        public string TradeUrl { get; set; }
    }
}
