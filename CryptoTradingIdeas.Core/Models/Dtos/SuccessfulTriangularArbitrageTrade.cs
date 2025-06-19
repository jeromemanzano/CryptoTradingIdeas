using CryptoTradingIdeas.Core.Enums;

namespace CryptoTradingIdeas.Core.Models.Dtos;

public class SuccessfulTriangularArbitrageTrade : SuccessfulTrade
{
    public override Ideas Idea => Ideas.TriangularArbitrage;
    public override DateTime TimeStampUtc { get; } = DateTime.UtcNow;
    public Exchanges Exchange { get; }
    public override decimal Profit { get; }
    public override decimal Capital { get; }
    public string Sequence { get; }

    public SuccessfulTriangularArbitrageTrade(
        TriangularArbitrageOpportunity opportunity,
        decimal profit,
        decimal capital)
    {
        Exchange = opportunity.Exchange;
        Profit = profit;
        Capital = capital;
        Sequence = $"{opportunity.FirstTransaction.PairSymbols}->{opportunity.SecondTransaction.PairSymbols}->{opportunity.ThirdTransaction.PairSymbols}";
    }
}