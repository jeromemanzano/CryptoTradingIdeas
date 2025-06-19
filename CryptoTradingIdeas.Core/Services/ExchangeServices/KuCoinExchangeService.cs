using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CryptoExchange.Net.Objects;
using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.Core.Interfaces.Services;
using CryptoTradingIdeas.Core.Models.Entities;
using Kucoin.Net.Clients;
using Kucoin.Net.Objects.Models.Spot;

namespace CryptoTradingIdeas.Core.Services.ExchangeServices;

public sealed class KuCoinExchangeService(
    IEntityCache<SpotData, (string, Exchanges)> spotDataCache,
    IEntityCache<LeveragedToken, (string, string, Exchanges)> leveragedTokenCache)
    : ExchangeServiceBase(spotDataCache, leveragedTokenCache), IExchangeService
{
    private readonly KucoinRestClient _kuCoinRestClient = new();

    public override Exchanges Exchange => Exchanges.KuCoin;

    protected override IObservable<SpotData> GetSpotDataStream()
    {
        return GetRefreshTimerObservable()
            .Select(_ => _kuCoinRestClient.SpotApi.ExchangeData.GetTickersAsync().ToObservable())
            .Concat() // Follows sequential order
            .Where(result => result is { Success: true, Data.Data.Length: > 0 })
            .SelectMany(result =>
            {
                var timestamp = DateTime.UtcNow;
                return result
                    .Data
                    .Data
                    // Checks if symbol is actively trading
                    .Where(kuCoinTick =>IsTradePairActivelyTrading(kuCoinTick.Symbol))
                    .Select(kuCoinTick =>
                    {
                        var (baseSymbol, quoteSymbol) =  GetBaseAndQuoteSymbols(kuCoinTick.Symbol);

                        return new SpotData
                        {
                            BaseSymbol = baseSymbol,
                            QuoteSymbol = quoteSymbol,
                            BidPrice = kuCoinTick.BestBidPrice ?? decimal.Zero,
                            AskPrice = kuCoinTick.BestAskPrice ?? decimal.Zero,
                            LatestPrice = kuCoinTick.LastPrice ?? decimal.Zero,
                            Exchange = Exchanges.KuCoin,
                            LastUpdateTimestamp = timestamp
                        };
                    })
                    // We're returning stable coin pairs first to simplify the logic for Triangular Arbitrage.
                    .OrderByDescending(spotData => SpotHelper.StableCoinSymbols.Contains(spotData.QuoteSymbol));
            });
    }

    protected override async Task<IReadOnlyCollection<PairSymbol>> GetExchangePairSymbolsAsync()
    {
        var exchangeInfoResult = await _kuCoinRestClient
            .SpotApi
            .ExchangeData
            .GetSymbolsAsync()
            .ConfigureAwait(false);

        if (!exchangeInfoResult.Success)
            throw new InvalidOperationException($"Failed to initialize {nameof(KuCoinExchangeService)}");

        return exchangeInfoResult
            .Data
            .Where(symbol => symbol.EnableTrading)
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

    private async Task<WebCallResult<KucoinOrderBook>> GetOrderBookResultAsync(string unifiedPairSymbol)
    {
        var orderBookResult = await _kuCoinRestClient
            .SpotApi
            .ExchangeData
            .GetAggregatedPartialOrderBookAsync(GetExchangePairSymbol(unifiedPairSymbol), limit: 20)
            .ConfigureAwait(false);

        if (!orderBookResult.Success)
            throw new InvalidOperationException($"Failed to get order book for {nameof(KuCoinExchangeService)}");

        return orderBookResult;
    }
}