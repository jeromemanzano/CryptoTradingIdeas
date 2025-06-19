using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CryptoExchange.Net.Objects;
using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.Core.Interfaces.Services;
using CryptoTradingIdeas.Core.Models.Entities;
using GateIo.Net.Clients;
using GateIo.Net.Objects.Models;
using GateIo.Net.Enums;

namespace CryptoTradingIdeas.Core.Services.ExchangeServices;

public sealed class GateIoExchangeService(
    IEntityCache<SpotData, (string, Exchanges)> spotDataCache,
    IEntityCache<LeveragedToken, (string, string, Exchanges)> leveragedTokenCache)
    : ExchangeServiceBase(spotDataCache, leveragedTokenCache), IExchangeService
{
    private readonly GateIoRestClient _gateIoRestClient = new();

    public override Exchanges Exchange => Exchanges.GateIo;

    protected override IObservable<SpotData> GetSpotDataStream()
    {
        return GetRefreshTimerObservable()
            .Select(_ => _gateIoRestClient.SpotApi.ExchangeData.GetTickersAsync().ToObservable())
            .Concat() // Follows sequential order
            .Where(result => result is { Success: true, Data.Length: > 0 })
            .SelectMany(result =>
            {
                var timestamp = DateTime.UtcNow;
                return result
                    .Data
                    // Checks if symbol is actively trading
                    .Where(binanceTick => IsTradePairActivelyTrading(binanceTick.Symbol))
                    .Select(gateIoTicker =>
                    {
                        var (baseSymbol, quoteSymbol) = GetBaseAndQuoteSymbols(gateIoTicker.Symbol);

                        return new SpotData
                        {
                            BaseSymbol = baseSymbol,
                            QuoteSymbol = quoteSymbol,
                            BidPrice = gateIoTicker.BestBidPrice ?? decimal.Zero,
                            AskPrice = gateIoTicker.BestAskPrice ?? decimal.Zero,
                            LatestPrice = gateIoTicker.LastPrice,
                            Exchange = Exchanges.GateIo,
                            LastUpdateTimestamp = timestamp
                        };
                    })
                    // We're returning stable coin pairs first to simplify the logic for Triangular Arbitrage.
                    .OrderByDescending(spotData => SpotHelper.StableCoinSymbols.Contains(spotData.QuoteSymbol));
            });
    }

    protected override async Task<IReadOnlyCollection<PairSymbol>> GetExchangePairSymbolsAsync()
    {
        var exchangeInfoResult = await _gateIoRestClient
            .SpotApi
            .ExchangeData
            .GetSymbolsAsync()
            .ConfigureAwait(false);

        if (!exchangeInfoResult.Success)
            throw new InvalidOperationException($"Failed to initialize {nameof(GateIoExchangeService)}");

        return exchangeInfoResult
            .Data
            .Where(symbol => symbol.TradeStatus is SymbolStatus.Tradable)
            .Select(symbol => new PairSymbol(
                ExchangePairSymbol: symbol.Name,
                BaseSymbol: symbol.BaseAsset,
                QuoteSymbol: symbol.QuoteAsset))
            .ToArray();
    }

    public async Task<(decimal Price, decimal Quantity)[]> GetMarketAsksAsync(string unifiedPairSymbols)
    {
        var orderBookResult = await GetOrderBookResultAsync(unifiedPairSymbols);

        return orderBookResult.Data.Asks.Select(entry => (entry.Price, entry.Quantity)).ToArray();
    }

    public async Task<(decimal Price, decimal Quantity)[]> GetMarketBidsAsync(string unifiedPairSymbol)
    {
        var orderBookResult = await GetOrderBookResultAsync(unifiedPairSymbol);

        return orderBookResult.Data.Bids.Select(entry => (entry.Price, entry.Quantity)).ToArray();
    }

    private async Task<WebCallResult<GateIoOrderBook>> GetOrderBookResultAsync(string unifiedPairSymbol)
    {
        var orderBookResult = await _gateIoRestClient
            .SpotApi
            .ExchangeData
            .GetOrderBookAsync(GetExchangePairSymbol(unifiedPairSymbol))
            .ConfigureAwait(false);

        if (!orderBookResult.Success)
            throw new InvalidOperationException($"Failed to get order for {nameof(GateIoExchangeService)}");

        return orderBookResult;
    }
}
