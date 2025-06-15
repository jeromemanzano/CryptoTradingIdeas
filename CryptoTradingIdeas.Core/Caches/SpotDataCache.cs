using System.Reactive.Linq;
using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.Core.Models.Entities;
using DynamicData;

namespace CryptoTradingIdeas.Core.Caches;

public class SpotDataEntityCache : IEntityCache<SpotData, (string, Exchanges)>
{
    private readonly SourceCache<SpotData, (string, Exchanges)> _spotDataCache =
        new(spotData => (spotData.UnifiedPairSymbol, spotData.Exchange));

    public IObservable<IChangeSet<SpotData, (string, Exchanges)>> Connect() =>
        _spotDataCache
            .Connect()
            .Publish()
            .RefCount();

    public void AddOrUpdate(params IEnumerable<SpotData> spotData)
    {
        _spotDataCache.AddOrUpdate(spotData);
    }
}