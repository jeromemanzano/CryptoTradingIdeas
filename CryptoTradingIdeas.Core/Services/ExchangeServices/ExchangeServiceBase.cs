using System.Reactive.Disposables;
using System.Reactive.Linq;
using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.Core.Models.Entities;
using ReactiveUI;

namespace CryptoTradingIdeas.Core.Services.ExchangeServices;

public abstract class ExchangeServiceBase(IEntityCache<SpotData, (string, Exchanges)> spotDataCache)
{
    private const int RefreshIntervalSeconds = 5;

    /// <remarks>
    /// Contains actively traded pair symbols for the exchange with <see cref="PairSymbol.ExchangePairSymbol"/> as key.
    /// </remarks>
    private readonly Dictionary<string, PairSymbol> _activelyTradedPairSymbols = new();

    /// <remarks>
    /// Key is the UnifiedPairSymbol (e.g., "BTCUSDT"), and value is <see cref="PairSymbol.ExchangePairSymbol"/>
    /// </remarks>
    private readonly Dictionary<string, string> _unifiedPairSymbols = new();

    private readonly CompositeDisposable _serviceCompositeDisposable = new();

    protected static IObservable<long> GetRefreshTimerObservable() => Observable.Timer(
        dueTime: TimeSpan.Zero,
        period: TimeSpan.FromSeconds(RefreshIntervalSeconds),
        RxApp.TaskpoolScheduler);

    protected abstract IObservable<SpotData> GetSpotDataStream();

    protected abstract Task<IReadOnlyCollection<PairSymbol>> GetExchangePairSymbolsAsync();

    protected bool IsTradePairActivelyTrading(string exchangePairSymbol)
    {
        return _activelyTradedPairSymbols.ContainsKey(exchangePairSymbol);
    }

    protected (string BaseSymbol, string quoteSymbol) GetBaseAndQuoteSymbols(string exchangePairSymbol)
    {
        if (_activelyTradedPairSymbols.TryGetValue(exchangePairSymbol, out var pairSymbol))
            return (pairSymbol.BaseSymbol, pairSymbol.QuoteSymbol);

        throw new KeyNotFoundException($"Exchange pair symbol '{exchangePairSymbol}' not found in actively traded pairs.");
    }

    protected string GetExchangePairSymbol(string unifiedPairSymbol)
    {
        if (_unifiedPairSymbols.TryGetValue(unifiedPairSymbol, out var exchangePairSymbol))
            return exchangePairSymbol;

        throw new KeyNotFoundException($"Unified pair symbol '{unifiedPairSymbol}' not found in exchange pair symbols.");
    }

    public async Task StartStreamingAsync()
    {
        // 1. Start with retrieving exchange pair symbols
        foreach (var pairSymbol in await GetExchangePairSymbolsAsync())
        {
            _activelyTradedPairSymbols.Add(pairSymbol.ExchangePairSymbol, pairSymbol);
            _unifiedPairSymbols.Add($"{pairSymbol.BaseSymbol}{pairSymbol.QuoteSymbol}", pairSymbol.ExchangePairSymbol);
        }

        // 2. Then, start streaming spot data
        GetSpotDataStream()
            .Subscribe(onNext: spotData => spotDataCache.AddOrUpdate(spotData))
            .DisposeWith(_serviceCompositeDisposable);
    }

    public void Dispose()
    {
        _serviceCompositeDisposable.Dispose();
    }

    protected record PairSymbol(string ExchangePairSymbol, string BaseSymbol, string QuoteSymbol);
}