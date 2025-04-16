namespace CryptoTrackerApp.Models
{
    public class Currency
    {
        public string Id { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal MarketCap { get; set; }
        public decimal PriceChange24h { get; set; }
        public decimal PriceChangePercentage24h { get; set; }
    }
}
