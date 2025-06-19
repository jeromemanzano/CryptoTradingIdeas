using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Models.Entities;
using DynamicData;

namespace CryptoTradingIdeas.Core.Caches;

public class SpotDataEntityCache : EntityCacheBase<SpotData, (string, Exchanges)>
{
    protected override SourceCache<SpotData, (string, Exchanges)> InternalCache { get; } =
        new(spotData => (spotData.UnifiedPairSymbol, spotData.Exchange));
}