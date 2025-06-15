using CryptoTradingIdeas.Core.Enums;

namespace CryptoTradingIdeas.Core.Models.Dtos;

public abstract class SuccessfulTrade
{
    public abstract Ideas Idea { get; }
    public abstract decimal Profit { get; }
    public abstract decimal Capital { get; }
    public abstract DateTime TimeStampUtc { get; }
}