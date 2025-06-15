using CryptoTradingIdeas.Core.Enums;

namespace CryptoTradingIdeas.Core.Models.Dtos;

public class TriangularArbitrageOpportunity(
    Exchanges exchange,
    TradeTransaction firstTransaction,
    TradeTransaction secondTransaction,
    TradeTransaction thirdTransaction)
{
    public Exchanges Exchange { get; } = exchange;
    public TradeTransaction FirstTransaction { get; } = firstTransaction;
    public TradeTransaction SecondTransaction { get; } = secondTransaction;
    public TradeTransaction ThirdTransaction { get; } = thirdTransaction;

    public decimal PotentialGain =>
        FirstTransaction.ConversionRate *
        SecondTransaction.ConversionRate *
        ThirdTransaction.ConversionRate
        - 1;
}