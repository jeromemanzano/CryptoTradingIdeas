using CryptoTradingIdeas.Core.Enums;

namespace CryptoTradingIdeas.Core.Models;

public record class SpotData
{
    public string PairName => $"{BaseSymbol}{QuoteSymbol}";
    public Exchanges Exchange { get; set; }
    public string BaseSymbol { get; set; }
    public string QuoteSymbol { get; set; }
    public decimal? LatestPrice { get; set; }
    public decimal? AskPrice { get; set; }
    public decimal? AskQuantity { get; set; }
    public decimal? BidPrice { get; set; }
    public decimal? BidQuantity { get; set; }
    public DateTime LastUpdateTimestamp { get; set; }
}