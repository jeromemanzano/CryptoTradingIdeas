using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CryptoExchange.Net.Objects;
using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.Core.Interfaces.Services;
using CryptoTradingIdeas.Core.Models.Entities;
using HTX.Net.Clients;
using HTX.Net.Objects.Models;

namespace CryptoTradingIdeas.Core.Services.ExchangeServices;

public sealed class HtxExchangeService
    (IEntityCache<SpotData, (string, Exchanges)> spotDataCache)
    : ExchangeServiceBase(spotDataCache), IExchangeService
{
    private readonly HTXRestClient _htxRestClient = new();

    public Exchanges Exchange => Exchanges.Htx;

    protected override IObservable<SpotData> GetSpotDataStream()
    {
        return GetRefreshTimerObservable()
            .Select(_ => _htxRestClient.SpotApi.ExchangeData.GetTickersAsync().ToObservable())
            .Concat() // Follows sequential order
            .Where(result => result is { Success: true, Data.Ticks.Length: > 0 })
            .SelectMany(result =>
            {
                var timestamp = DateTime.UtcNow;
                return result
                    .Data
                    .Ticks
                    // Checks if symbol is actively trading
                    .Where(htxTick => IsTradePairActivelyTrading(htxTick.Symbol))
                    .Select(htxTick =>
                    {
                        var (baseSymbol, quoteSymbol) = GetBaseAndQuoteSymbols(htxTick.Symbol);

                        return new SpotData
                        {
                            BaseSymbol = baseSymbol,
                            QuoteSymbol = quoteSymbol,
                            BidPrice = htxTick.BestBidPrice,
                            AskPrice = htxTick.BestAskPrice,
                            LatestPrice = htxTick.LastTradePrice,
                            Exchange = Exchanges.Htx,
                            LastUpdateTimestamp = timestamp
                        };
                    })
                    // We're returning stable coin pairs first to simplify the logic for Triangular Arbitrage.
                    .OrderByDescending(spotData => SpotHelper.StableCoinSymbols.Contains(spotData.QuoteSymbol));
            });
    }

    protected override async Task<IReadOnlyCollection<PairSymbol>> GetExchangePairSymbolsAsync()
    {
        var exchangeInfoResult = await _htxRestClient
            .SpotApi
            .ExchangeData
            .GetSymbolsAsync()
            .ConfigureAwait(false);

        if (!exchangeInfoResult.Success)
            throw new InvalidOperationException($"Failed to initialize {nameof(HtxExchangeService)}");

        return exchangeInfoResult
            .Data
            .Where(symbol => symbol.TradeEnabled)
            .Select(symbol => new PairSymbol(
                ExchangePairSymbol: symbol.Name,
                BaseSymbol: symbol.BaseAsset.ToUpper(),
                QuoteSymbol: symbol.QuoteAsset.ToUpper()))
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

    private async Task<WebCallResult<HTXOrderBook>> GetOrderBookResultAsync(string unifiedPairSymbol)
    {
        var orderBookResult = await _htxRestClient
            .SpotApi
            .ExchangeData
            .GetOrderBookAsync(GetExchangePairSymbol(unifiedPairSymbol), mergeStep: 0)
            .ConfigureAwait(false);

        if (!orderBookResult.Success)
            throw new InvalidOperationException($"Failed to get order book for {nameof(HtxExchangeService)}");

        return orderBookResult;
    }
}
