using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces.Models;

namespace CryptoTradingIdeas.Core.Models.Entities;

public class SpotData : IEntity<(string, Exchanges)>
{
    public (string, Exchanges) Id => (UnifiedPairSymbol, Exchange);
    public string UnifiedPairSymbol => $"{BaseSymbol}{QuoteSymbol}";
    public Exchanges Exchange { get; init; }
    public required string BaseSymbol { get; init; }
    public required string QuoteSymbol { get; init; }
    public decimal LatestPrice { get; init; }
    public decimal AskPrice { get; init; }
    public decimal BidPrice { get; init; }
    public DateTime LastUpdateTimestamp { get; init; }
}