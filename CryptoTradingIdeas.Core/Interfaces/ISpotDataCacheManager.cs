using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Injection;
using CryptoTradingIdeas.Core.Models;
using DynamicData;

namespace CryptoTradingIdeas.Core.Interfaces;

/// <summary>
/// Manages the caching of spot market data for different exchanges.
/// </summary>
public interface ISpotDataCacheManager : IDisposable, ISingletonDependency
{
    /// <summary>
    /// Gets the source cache containing spot market data, indexed by symbol and exchange.
    /// </summary>
    ISourceCache<SpotData, (string, Exchanges)> SpotDataCache { get; }

    /// <summary>
    /// Initializes the spot data cache manager and sets up necessary data structures.
    /// </summary>
    void Initialize();
}