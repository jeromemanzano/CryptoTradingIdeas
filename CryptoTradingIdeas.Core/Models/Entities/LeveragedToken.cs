using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces.Models;

namespace CryptoTradingIdeas.Core.Models.Entities;

public class LeveragedToken : IEntity<(string, string, Exchanges)>
{
    public (string, string, Exchanges) Id => (BaseSymbol, QuoteSymbol, Exchange);

    public Exchanges Exchange { get; init; }
    public required string BaseSymbol { get; init; }
    public required string QuoteSymbol { get; init; }
}