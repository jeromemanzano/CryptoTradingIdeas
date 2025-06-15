using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CryptoExchange.Net.Objects;
using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.Core.Interfaces.Services;
using CryptoTradingIdeas.Core.Models.Entities;
using OKX.Net.Clients;
using OKX.Net.Enums;
using OKX.Net.Objects.Market;

namespace CryptoTradingIdeas.Core.Services.ExchangeServices;

public sealed class OkxExchangeService
    (IEntityCache<SpotData, (string, Exchanges)> spotDataCache)
    : ExchangeServiceBase(spotDataCache), IExchangeService
{
    private readonly OKXRestClient _okxClient = new();

    public Exchanges Exchange => Exchanges.Okx;

    protected override IObservable<SpotData> GetSpotDataStream()
    {
        return GetRefreshTimerObservable()
            .Select(_ => _okxClient.UnifiedApi.ExchangeData.GetTickersAsync(InstrumentType.Spot).ToObservable())
            .Concat() // Follows sequential order
            .Where(result => result is { Success: true, Data.Length: > 0 })
            .SelectMany(result =>
            {
                var timestamp = DateTime.UtcNow;
                return result
                    .Data
                    // Checks if symbol is actively trading
                    .Where(okxTick => IsTradePairActivelyTrading(okxTick.Symbol))
                    .Select(okxTick =>
                    {
                        var (baseSymbol, quoteSymbol) = GetBaseAndQuoteSymbols(okxTick.Symbol);

                        return new SpotData
                        {
                            BaseSymbol = baseSymbol,
                            QuoteSymbol = quoteSymbol,
                            BidPrice = okxTick.BestBidPrice ?? decimal.Zero,
                            AskPrice = okxTick.BestAskPrice ?? decimal.Zero,
                            LatestPrice = okxTick.LastPrice ?? decimal.Zero,
                            Exchange = Exchanges.Okx,
                            LastUpdateTimestamp = timestamp
                        };
                    })
                    // We're returning stable coin pairs first to simplify the logic for Triangular Arbitrage.
                    .OrderByDescending(spotData => SpotHelper.StableCoinSymbols.Contains(spotData.QuoteSymbol));
            });
    }

    protected override async Task<IReadOnlyCollection<PairSymbol>> GetExchangePairSymbolsAsync()
    {
        var exchangeInfoResult = await _okxClient
            .UnifiedApi
            .ExchangeData
            .GetSymbolsAsync(InstrumentType.Spot)
            .ConfigureAwait(false);

        if (!exchangeInfoResult.Success)
            throw new InvalidOperationException($"Failed to initialize {nameof(OkxExchangeService)}");

        return exchangeInfoResult
            .Data
            .Where(symbol => symbol.RuleType is SymbolRuleType.Normal)
            .Select(symbol => new PairSymbol(
                ExchangePairSymbol: symbol.Symbol,
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

    private async Task<WebCallResult<OKXOrderBook>> GetOrderBookResultAsync(string unifiedPairSymbol)
    {
        var orderBookResult = await _okxClient
            .UnifiedApi
            .ExchangeData
            .GetOrderBookAsync(GetExchangePairSymbol(unifiedPairSymbol), depth: 10)
            .ConfigureAwait(false);

        if (!orderBookResult.Success)
            throw new InvalidOperationException($"Failed to get order book for {nameof(OkxExchangeService)}");

        return orderBookResult;
    }
}
