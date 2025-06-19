using System.Reactive.Linq;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.Core.Interfaces.Models;
using DynamicData;

namespace CryptoTradingIdeas.Core.Caches;

public abstract class EntityCacheBase<TValue, TKey>
    : IEntityCache<TValue, TKey>
    where TValue : class, IEntity<TKey>
    where TKey : notnull
{
    protected abstract SourceCache<TValue, TKey> InternalCache { get; }

    public IObservable<IChangeSet<TValue, TKey>> Connect() =>
        InternalCache
            .Connect()
            .Publish()
            .RefCount();

    public void AddOrUpdate(params IEnumerable<TValue> newItems)
    {
        InternalCache.AddOrUpdate(newItems);
    }
}