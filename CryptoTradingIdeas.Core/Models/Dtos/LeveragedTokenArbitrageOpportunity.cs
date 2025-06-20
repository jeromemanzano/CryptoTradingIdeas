using CryptoTradingIdeas.Core.Enums;
using ReactiveUI;

namespace CryptoTradingIdeas.Core.Models.Dtos;

public class LeveragedTokenArbitrageOpportunity : ReactiveObject
{
    public Exchanges Exchange { get; init; }
    public int Multiplier { get; init; }
    public required string BaseSymbol { get; init; }
    public required string QuoteSymbol { get; init; }
    public string LongPairSymbol => $"{BaseSymbol}L{QuoteSymbol}";
    public string ShortPairSymbol => $"{BaseSymbol}S{QuoteSymbol}";

    public decimal LongAmount { get; set; }
    public decimal ShortAmount { get; set; }
    public decimal CurrentLongPrice { get; set; }
    public decimal CurrentShortPrice { get; set; }

    public decimal Profit => CurrentLongPrice != decimal.Zero && CurrentShortPrice != decimal.Zero
        ? CurrentLongPrice + CurrentShortPrice - 200m
        : decimal.Zero;
}