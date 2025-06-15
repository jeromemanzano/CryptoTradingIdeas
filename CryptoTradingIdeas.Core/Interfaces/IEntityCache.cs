using CryptoTradingIdeas.Core.Injection;
using CryptoTradingIdeas.Core.Interfaces.Models;
using DynamicData;

namespace CryptoTradingIdeas.Core.Interfaces;

public interface IEntityCache<TValue, TKey> : ISingletonDependency
    where TKey : notnull
    where TValue : class, IEntity<TKey>
{
    IObservable<IChangeSet<TValue, TKey>> Connect();

    void AddOrUpdate(params IEnumerable<TValue> value);
}