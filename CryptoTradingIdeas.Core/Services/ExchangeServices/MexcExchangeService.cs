using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CryptoExchange.Net.Objects;
using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.Core.Interfaces.Services;
using CryptoTradingIdeas.Core.Models.Entities;
using Mexc.Net.Clients;
using Mexc.Net.Enums;
using Mexc.Net.Objects.Models.Spot;

namespace CryptoTradingIdeas.Core.Services.ExchangeServices;

public sealed class MexcExchangeService
    (IEntityCache<SpotData, (string, Exchanges)> spotDataCache)
    : ExchangeServiceBase(spotDataCache), IExchangeService
{
    private readonly MexcRestClient _mexcRestClient = new();

    public Exchanges Exchange => Exchanges.Mexc;

    protected override IObservable<SpotData> GetSpotDataStream()
    {
        return GetRefreshTimerObservable()
            .Select(_ => _mexcRestClient.SpotApi.ExchangeData.GetTickersAsync().ToObservable())
            .Concat() // Follows sequential order
            .Where(result => result is { Success: true, Data.Length: > 0 })
            .SelectMany(result =>
            {
                var timestamp = DateTime.UtcNow;
                return result
                    .Data
                    // Checks if symbol is actively trading
                    .Where(mexcTick => IsTradePairActivelyTrading(mexcTick.Symbol))
                    .Select(mexcTick =>
                    {
                        var (baseSymbol, quoteSymbol) = GetBaseAndQuoteSymbols(mexcTick.Symbol);

                        return new SpotData
                        {
                            BaseSymbol = baseSymbol,
                            QuoteSymbol = quoteSymbol,
                            BidPrice = mexcTick.BestBidPrice,
                            AskPrice = mexcTick.BestAskPrice,
                            LatestPrice = mexcTick.LastPrice,
                            Exchange = Exchanges.Mexc,
                            LastUpdateTimestamp = timestamp
                        };
                    })
                    // We're returning stable coin pairs first to simplify the logic for Triangular Arbitrage.
                    .OrderByDescending(spotData => SpotHelper.StableCoinSymbols.Contains(spotData.QuoteSymbol));
            });
    }

    protected override async Task<IReadOnlyCollection<PairSymbol>> GetExchangePairSymbolsAsync()
    {
        var exchangeInfoResult = await _mexcRestClient
            .SpotApi
            .ExchangeData
            .GetExchangeInfoAsync()
            .ConfigureAwait(false);

        if (!exchangeInfoResult.Success)
            throw new InvalidOperationException($"Failed to initialize {nameof(MexcExchangeService)}");

        return exchangeInfoResult
            .Data
            .Symbols
            .Where(symbol => symbol.TradeSidesEnabled is TradeSidesStatus.AllEnabled)
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

    private async Task<WebCallResult<MexcOrderBook>> GetOrderBookResultAsync(string unifiedPairSymbol)
    {
        var orderBookResult = await _mexcRestClient
            .SpotApi
            .ExchangeData
            .GetOrderBookAsync(GetExchangePairSymbol(unifiedPairSymbol), limit: 20)
            .ConfigureAwait(false);

        if (!orderBookResult.Success)
            throw new InvalidOperationException($"Failed to get order book for {nameof(MexcExchangeService)}");

        return orderBookResult;
    }
}