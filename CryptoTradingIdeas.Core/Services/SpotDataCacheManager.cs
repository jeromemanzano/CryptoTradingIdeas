using System.Reactive.Disposables;
using System.Reactive.Linq;
using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.Core.Models;
using DynamicData;
using ReactiveUI;

namespace CryptoTradingIdeas.Core.Services;

public class SpotDataCacheManager : ISpotDataCacheManager
{
    private readonly IEnumerable<IExchangeService> _exchangeServices;
    private readonly CompositeDisposable _compositeDisposable = new();

    public ISourceCache<SpotData, (string, Exchanges)> SpotDataCache { get; }

    public SpotDataCacheManager(IEnumerable<IExchangeService> exchangeServices)
    {
        _exchangeServices = (IExchangeService[])exchangeServices;

        SpotDataCache = new SourceCache<SpotData, (string, Exchanges)>(data => (data.PairName, data.Exchange));

        foreach (var service in _exchangeServices)
        {
            service
                .SpotLatestDataStream
                .Subscribe(spotData => SpotDataCache.AddOrUpdate(spotData))
                .DisposeWith(_compositeDisposable);
        }
    }

    public void Initialize()
    {
        _exchangeServices
            .Select(service => Observable.FromAsync(service.StartStreamingAsync, RxApp.TaskpoolScheduler))
            .Merge(5)
            .Subscribe()
            .DisposeWith(_compositeDisposable);
    }

    public void Dispose()
    {
        SpotDataCache.Dispose();
        _compositeDisposable.Dispose();
    }
}