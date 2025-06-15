
using CryptoTradingIdeas.Core.Enums;

namespace CryptoTradingIdeas.Core.Models.Dtos;

public record TradeTransaction
{
    public required string PairSymbols { get; init; }
    public required SpotTradeType Type { get; init; }
    public required decimal Price { get; init; }

    public decimal ConversionRate => Type is SpotTradeType.Buy
        ? 1 / Price
        : Price;
}