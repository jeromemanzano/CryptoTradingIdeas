using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using Binance.Net.Clients;
using Binance.Net.Enums;
using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.Core.Models;

namespace CryptoTradingIdeas.Core.Services;

public class BinanceExchangeService : ExchangeServiceBase, IExchangeService
{
    private readonly BinanceRestClient _binanceClient = new();
    private readonly IConnectableObservable<SpotData> _spotLatestDataStream;
    private readonly CompositeDisposable _compositeDisposable = new();
    private readonly HashSet<string> _activelyTradingSymbolName = [];
    private readonly HashSet<string> _spotQuoteAssetSymbols = [];

    public IObservable<SpotData> SpotLatestDataStream => _spotLatestDataStream;

    public BinanceExchangeService()
    {
        _spotLatestDataStream = GetRefreshTimerObservable()
            .Select(_ => _binanceClient.SpotApi.ExchangeData.GetTickersAsync().ToObservable())
            .Concat() // Respects sequential order
            .Where(result => result is { Success: true, Data.Length: > 0 })
            .SelectMany(result =>
            {
                var timestamp = DateTime.UtcNow;
                return result
                    .Data
                    // Check if symbol is actively trading
                    .Where(binanceTick => _activelyTradingSymbolName.Contains(binanceTick.Symbol))
                    .Select(binanceTick =>
                    {
                        var (baseSymbol, quoteSymbol) = GetBaseAndQuoteSymbols(binanceTick.Symbol);

                        return new SpotData
                        {
                            BaseSymbol = baseSymbol,
                            QuoteSymbol = quoteSymbol,
                            BidPrice = binanceTick.BestBidPrice,
                            BidQuantity = binanceTick.BestBidQuantity,
                            AskQuantity = binanceTick.BestAskQuantity,
                            AskPrice = binanceTick.BestAskPrice,
                            LatestPrice = binanceTick.LastPrice,
                            Exchange = Exchanges.Binance,
                            LastUpdateTimestamp = timestamp
                        };
                    });
            })
            .Publish();
    }

    public async Task StartStreamingAsync()
    {
        // 1. Start with retrieving exchange info so we can set _spotQuoteAssetSymbols and _activelyTradingSymbolName
        var exchangeInfoResult = await _binanceClient
            .SpotApi
            .ExchangeData
            .GetExchangeInfoAsync(PermissionType.Spot)
            .ConfigureAwait(false);

        if (!exchangeInfoResult.Success)
            throw new InvalidOperationException($"Failed to initialize {nameof(BinanceExchangeService)}");

        foreach (var symbol in exchangeInfoResult
                     .Data
                     .Symbols
                     .Where(symbol => symbol.Status is SymbolStatus.Trading))
        {
            // _spotQuoteAssetSymbols - will be used to parse the base and quote from symbol
            // e.g. BTCUSDT to base: BTC quote: USDT
            _spotQuoteAssetSymbols.Add(symbol.QuoteAsset);

            // _activelyTradingSymbolName - will be used for filtering out pairs that are not actively traded
            _activelyTradingSymbolName.Add(symbol.Name);
        }

        // 2. Only after Connect() is executed will SpotLatestData start streaming data
        _spotLatestDataStream.Connect().DisposeWith(_compositeDisposable);
    }

    /// <remarks>
    /// Parse the pair symbol into base and quote symbol. e.g. BTCUSDT to (BTC, USDT)
    /// </remarks>
    private (string BaseSymbol, string QuoteSymbol) GetBaseAndQuoteSymbols(string pairName)
    {
        foreach (var quoteSymbol in _spotQuoteAssetSymbols.Where(pairName.EndsWith))
        {
            return (pairName.Replace(quoteSymbol, string.Empty), quoteSymbol);
        }

        throw new FormatException($"Unable to retrieve base and quote symbols for pair name: {pairName}");
    }

    public void Dispose()
    {
        _compositeDisposable.Dispose();
    }
}