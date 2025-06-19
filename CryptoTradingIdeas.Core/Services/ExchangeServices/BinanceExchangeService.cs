using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Spot;
using CryptoExchange.Net.Objects;
using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.Core.Interfaces.Services;
using CryptoTradingIdeas.Core.Models.Entities;

namespace CryptoTradingIdeas.Core.Services.ExchangeServices;

public sealed class BinanceExchangeService(
    IEntityCache<SpotData, (string, Exchanges)> spotDataCache,
    IEntityCache<LeveragedToken, (string, string, Exchanges)> leveragedTokenCache)
    : ExchangeServiceBase(spotDataCache, leveragedTokenCache), IExchangeService
{
    private readonly BinanceRestClient _binanceClient = new();

    public override Exchanges Exchange => Exchanges.Binance;

    protected override IObservable<SpotData> GetSpotDataStream()
    {
        return GetRefreshTimerObservable()
            .Select(_ => _binanceClient.SpotApi.ExchangeData.GetTickersAsync().ToObservable())
            .Concat() // Follows sequential order
            .Where(result => result is { Success: true, Data.Length: > 0 })
            .SelectMany(result =>
            {
                var timestamp = DateTime.UtcNow;
                return result
                    .Data
                    // Checks if symbol is actively trading
                    .Where(binanceTick => IsTradePairActivelyTrading(binanceTick.Symbol))
                    .Select(binanceTick =>
                    {
                        var (baseSymbol, quoteSymbol) = GetBaseAndQuoteSymbols(binanceTick.Symbol);

                        return new SpotData
                        {
                            BaseSymbol = baseSymbol,
                            QuoteSymbol = quoteSymbol,
                            BidPrice = binanceTick.BestBidPrice,
                            AskPrice = binanceTick.BestAskPrice,
                            LatestPrice = binanceTick.LastPrice,
                            Exchange = Exchanges.Binance,
                            LastUpdateTimestamp = timestamp
                        };
                    })
                    // We're returning stable coin pairs first to simplify the logic for Triangular Arbitrage.
                    .OrderByDescending(spotData => SpotHelper.StableCoinSymbols.Contains(spotData.QuoteSymbol));
            });
    }

    protected override async Task<IReadOnlyCollection<PairSymbol>> GetExchangePairSymbolsAsync()
    {
        var exchangeInfoResult = await _binanceClient
            .SpotApi
            .ExchangeData
            .GetExchangeInfoAsync(PermissionType.Spot)
            .ConfigureAwait(false);

        if (!exchangeInfoResult.Success)
            throw new InvalidOperationException($"Failed to initialize {nameof(BinanceExchangeService)}");

        return exchangeInfoResult
            .Data
            .Symbols
            .Where(symbol => symbol.Status is SymbolStatus.Trading)
            .Select(symbol => new PairSymbol(
                ExchangePairSymbol: symbol.Name,
                BaseSymbol: symbol.BaseAsset,
                QuoteSymbol: symbol.QuoteAsset))
            .ToArray();
    }

    public async Task<(decimal Price, decimal Quantity)[]> GetMarketAsksAsync(string unifiedPairSymbol)
    {
        var orderBookResult = await GetOrderBookResultAsync(unifiedPairSymbol);

        return orderBookResult.Data.Asks.Select(entry => (entry.Price, entry.Quantity)).ToArray();
    }

    public async Task<(decimal Price, decimal Quantity)[]> GetMarketBidsAsync(string unifiedPairSymbol)
    {
        var orderBookResult = await GetOrderBookResultAsync(unifiedPairSymbol);

        return orderBookResult.Data.Bids.Select(entry => (entry.Price, entry.Quantity)).ToArray();
    }

    private async Task<WebCallResult<BinanceOrderBook>> GetOrderBookResultAsync(string unifiedPairSymbol)
    {
        var orderBookResult = await _binanceClient
            .SpotApi
            .ExchangeData
            .GetOrderBookAsync(unifiedPairSymbol, limit: 10)
            .ConfigureAwait(false);

        if (!orderBookResult.Success)
            throw new InvalidOperationException($"Failed to get order book for {nameof(BinanceExchangeService)}");

        return orderBookResult;
    }
}
