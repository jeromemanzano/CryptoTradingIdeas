using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.Core.Interfaces.Services;
using CryptoTradingIdeas.Core.Models.Dtos;
using CryptoTradingIdeas.Core.Models.Entities;
using DynamicData;
using ReactiveUI;
using Serilog;

namespace CryptoTradingIdeas.Core.Services;

public sealed class TriangularArbitrageService : ITriangularArbitrageService
{
    private const decimal StartingStableCoinAmount = 100m; // Example starting amount 100USD

    private readonly Dictionary<(Exchanges, string), ConversionPrices> _cryptoSymbolConversionPrices = new();
    private readonly ISpotTradeService _spotTradeService;
    private readonly CompositeDisposable _serviceDisposable = new();

    private readonly SourceCache<TriangularArbitrageOpportunity, (Exchanges, string, string, string)> _gainsCache =
        new(gain => (
            gain.Exchange,
            gain.FirstTransaction.PairSymbols,
            gain.SecondTransaction.PairSymbols,
            gain.ThirdTransaction.PairSymbols));

    /// <summary>
    /// Read-only collection of triangular arbitrage opportunities. This calculates potential gains based on
    /// last prices of the pairs.
    /// </summary>
    public ReadOnlyObservableCollection<TriangularArbitrageOpportunity> TriangularArbitrageOpportunities { get; }

    public TriangularArbitrageService(
        ISpotTradeService spotTradeService,
        IEntityCache<SpotData, (string, Exchanges)> spotDataCache)
    {
        _spotTradeService = spotTradeService;

        var opportunitiesChanged = spotDataCache
            .Connect()
            // Ignore pairs that starts with stable coin since they are supposed to be stable e.g. USDTUSDC
            .Filter(spotData => !SpotHelper.StableCoinSymbols.Contains(spotData.BaseSymbol))
            .ForEachChange(change => AddOrUpdateConversionRates(change.Current))
            .TransformMany(
                manySelector: GetPotentialGains,
                keySelector: gain => (
                    gain.Exchange,
                    gain.FirstTransaction.PairSymbols,
                    gain.SecondTransaction.PairSymbols,
                    gain.ThirdTransaction.PairSymbols));

        opportunitiesChanged
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out var opportunities)
            .Subscribe()
            .DisposeWith(_serviceDisposable);

        TriangularArbitrageOpportunities = opportunities;

        const decimal minimumLoggingGainThreshold = 0.01m;

        opportunitiesChanged
            .Filter(gain => gain.PotentialGain > minimumLoggingGainThreshold)
            .TransformAsync(CalculateActualGainAsync)
            .Filter(tuple => tuple.Profit > decimal.Zero)
            .Transform(tuple =>
                new SuccessfulTriangularArbitrageTrade(tuple.Opportunity, tuple.Profit, StartingStableCoinAmount))
            .Flatten()
            .Subscribe(trade =>
            {
                Log.Information(
                    $"[Triangular Arbitrage] {trade.Current.Profit} USD profit from trading {trade.Current.Sequence}.");
            })
            .DisposeWith(_serviceDisposable);
    }

    private IReadOnlyCollection<TriangularArbitrageOpportunity> GetPotentialGains(SpotData spotData)
    {
        // Data is organized so stable coin rates are added first to _cryptoSymbolConversionRates to simplify logic.
        // This makes time complexity of this method linear O(n) where n is number of stable coins
        if (SpotHelper.StableCoinSymbols.Contains(spotData.QuoteSymbol))
            return [];

        if (!_cryptoSymbolConversionPrices.TryGetValue((spotData.Exchange, spotData.BaseSymbol), out var baseSymbolConversionRates) ||
            !_cryptoSymbolConversionPrices.TryGetValue((spotData.Exchange, spotData.QuoteSymbol), out var quoteSymbolConversionRates))
        {
            return [];
        }

        // Base and Quote symbols are not stable coin which means we just have to find stable coin rates for both
        // base and quote symbols
        return SpotHelper
            .StableCoinSymbols
            .Where(stableCoinSymbol => baseSymbolConversionRates.AskPrices.ContainsKey(stableCoinSymbol) &&
                                       quoteSymbolConversionRates.AskPrices.ContainsKey(stableCoinSymbol))
            .SelectMany(stableCoinSymbol =>
            {
                return new TriangularArbitrageOpportunity[]
                {
                    // Forward Base-Stable -> Base-Quote -> Quote-Stable
                    new(spotData.Exchange,
                        // 1. We'll buy ETH from USDT using ETHUSDT ask price
                        firstTransaction: new TradeTransaction
                        {
                            PairSymbols = $"{spotData.BaseSymbol}{stableCoinSymbol}",
                            Type = SpotTradeType.Buy,
                            Price = baseSymbolConversionRates.AskPrices[stableCoinSymbol]
                        },
                        // 2. We'll sell ETH to BTC using ETHBTC bid price
                        secondTransaction: new TradeTransaction
                        {
                            PairSymbols = $"{spotData.BaseSymbol}{spotData.QuoteSymbol}",
                            Type = SpotTradeType.Sell,
                            Price = spotData.BidPrice
                        },
                        // 3. We'll sell BTC to USDT using BTCUSDT bid price
                        thirdTransaction: new TradeTransaction
                        {
                            PairSymbols = $"{spotData.QuoteSymbol}{stableCoinSymbol}",
                            Type = SpotTradeType.Sell,
                            Price = quoteSymbolConversionRates.BidPrices[stableCoinSymbol]
                        }),
                    // Reverse Quote-Stable -> Base-Quote -> Base-Stable
                    new(spotData.Exchange,
                        // 1. We'll buy BTC from USDT using BTCUSDT ask price
                        firstTransaction: new TradeTransaction
                        {
                            PairSymbols = $"{spotData.QuoteSymbol}{stableCoinSymbol}",
                            Type = SpotTradeType.Buy,
                            Price = quoteSymbolConversionRates.AskPrices[stableCoinSymbol]
                        },
                        // 2. We'll buy ETH from BTC using ETHBTC ask price
                        secondTransaction: new TradeTransaction
                        {
                            PairSymbols = $"{spotData.BaseSymbol}{spotData.QuoteSymbol}",
                            Type = SpotTradeType.Buy,
                            Price = spotData.AskPrice
                        },
                        // 3. We'll sell ETH to USDT using ETHUSDT bid price
                        thirdTransaction: new TradeTransaction
                        {
                            PairSymbols = $"{spotData.BaseSymbol}{stableCoinSymbol}",
                            Type = SpotTradeType.Sell,
                            Price = baseSymbolConversionRates.BidPrices[stableCoinSymbol]
                        })
                };
            })
            .ToArray();
    }

    private void AddOrUpdateConversionRates(SpotData spotData)
    {
        var key = (spotData.Exchange, spotData.BaseSymbol);
        if (!_cryptoSymbolConversionPrices.TryGetValue(key, out var conversionRates))
        {
            conversionRates = new ConversionPrices();
            _cryptoSymbolConversionPrices[key] = conversionRates;
        }

        conversionRates.AskPrices[spotData.QuoteSymbol] = spotData.AskPrice;
        conversionRates.BidPrices[spotData.QuoteSymbol] = spotData.BidPrice;
    }

    private async Task<(TriangularArbitrageOpportunity Opportunity, decimal Profit)> CalculateActualGainAsync(
        TriangularArbitrageOpportunity opportunity)
    {
        var firstConversionAmount = await GetTransactionResultAsync(
            opportunity.Exchange,
            opportunity.FirstTransaction,
            StartingStableCoinAmount);

        var secondConversionAmount = await GetTransactionResultAsync(
            opportunity.Exchange,
            opportunity.SecondTransaction,
            firstConversionAmount);

        var finalConversionAmount = await GetTransactionResultAsync(
            opportunity.Exchange,
            opportunity.ThirdTransaction,
            secondConversionAmount);

        return (opportunity, finalConversionAmount - StartingStableCoinAmount);
    }

    private async Task<decimal> GetTransactionResultAsync(Exchanges exchange, TradeTransaction transaction, decimal amount)
    {
        return transaction.Type is SpotTradeType.Buy
            ? await _spotTradeService.GetMarketBuyingPriceAsync(exchange, transaction.PairSymbols, amount)
            : await _spotTradeService.GetMarketSellingPriceAsync(exchange, transaction.PairSymbols, amount);
    }

    private class ConversionPrices
    {
        public Dictionary<string, decimal> AskPrices { get; } = [];
        public Dictionary<string, decimal> BidPrices { get; } = [];
    }

    public void Dispose()
    {
        _serviceDisposable.Dispose();
        _gainsCache.Dispose();
    }
}