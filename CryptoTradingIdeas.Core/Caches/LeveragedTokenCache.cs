using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Models.Entities;
using DynamicData;

namespace CryptoTradingIdeas.Core.Caches;

public class LeveragedTokenCache : EntityCacheBase<LeveragedToken, (string, string, Exchanges)>
{
    protected override SourceCache<LeveragedToken, (string, string, Exchanges)> InternalCache { get; } =
        new(spotData => (spotData.BaseSymbol, spotData.QuoteSymbol, spotData.Exchange));
}