using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.Core.Interfaces.Services;
using CryptoTradingIdeas.Core.Models.Dtos;
using CryptoTradingIdeas.Core.Models.Entities;
using DynamicData;
using DynamicData.PLinq;
using ReactiveUI;
using Serilog;

namespace CryptoTradingIdeas.Core.Services;

public sealed class LeveragedTokenArbitrageService : ILeveragedTokenArbitrageService
{
    private const decimal InitialStableCoinAmount = 100m;
    private readonly ISpotTradeService _spotTradeService;
    private readonly IConnectableObservable<Unit> _updateOpportunities;
    private readonly CompositeDisposable _serviceDisposable = new();
    public ReadOnlyObservableCollection<LeveragedTokenArbitrageOpportunity> LeveragedTokenArbitrageOpportunities { get; }

    public LeveragedTokenArbitrageService(
        IEntityCache<SpotData, (string, Exchanges)> spotDataCache,
        IEntityCache<LeveragedToken, (string, string, Exchanges)> leveragedTokenCache,
        ISpotTradeService spotTradeService)
    {
        _spotTradeService = spotTradeService;

        leveragedTokenCache
            .Connect()
            .Transform(token => new LeveragedTokenArbitrageOpportunity
            {
                Exchange = token.Exchange,
                Multiplier = (int) char.GetNumericValue(token.BaseSymbol[^1]),
                BaseSymbol = token.BaseSymbol,
                QuoteSymbol = token.QuoteSymbol,
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .ForEachChange(opportunity => LogSuccessfulTrades(opportunity.Current))
            .Bind(out var leveragedTokenArbitrageOpportunities)
            .Subscribe()
            .DisposeWith(_serviceDisposable);

        LeveragedTokenArbitrageOpportunities = leveragedTokenArbitrageOpportunities;

        _updateOpportunities = Observable
            .Timer(dueTime: TimeSpan.Zero, TimeSpan.FromMinutes(1), RxApp.TaskpoolScheduler)
            .SelectMany(_ =>
                leveragedTokenArbitrageOpportunities
                    .Select(opportunity => UpdateValuesAsync(opportunity).ToObservable()))
            .Concat()
            .Publish();
    }

    private static void LogSuccessfulTrades(LeveragedTokenArbitrageOpportunity opportunity)
    {
        if (opportunity.Profit > decimal.Zero)
        {
            Log.Information(
                $"[Leveraged Token Arbitrage] {opportunity.Profit} USD profit from trading {opportunity.BaseSymbol}L/S in  {opportunity.Exchange}.");
        }
    }

    public void StartUpdatingOpportunities()
    {
        _updateOpportunities.Connect();
    }

    private async Task UpdateValuesAsync(LeveragedTokenArbitrageOpportunity opportunity)
    {
        await Task.WhenAll(UpdateLongValuesAsync(), UpdateShortValuesAsync());
        return;

        async Task UpdateLongValuesAsync()
        {
            if (opportunity.LongAmount == decimal.Zero)
            {
                opportunity.LongAmount = await _spotTradeService.GetMarketBuyingPriceAsync(
                    opportunity.Exchange,
                    opportunity.LongPairSymbol,
                    InitialStableCoinAmount);
            }

            opportunity.CurrentLongPrice = await _spotTradeService.GetMarketSellingPriceAsync(
                opportunity.Exchange,
                opportunity.LongPairSymbol,
                opportunity.LongAmount);
        }

        async Task UpdateShortValuesAsync()
        {
            if (opportunity.ShortAmount == decimal.Zero)
            {
                opportunity.ShortAmount = await _spotTradeService.GetMarketBuyingPriceAsync(
                    opportunity.Exchange,
                    opportunity.ShortPairSymbol,
                    InitialStableCoinAmount);
            }

            opportunity.CurrentShortPrice = await _spotTradeService.GetMarketSellingPriceAsync(
                opportunity.Exchange,
                opportunity.ShortPairSymbol,
                opportunity.ShortAmount);
        }
    }

    public void Dispose()
    {
        _serviceDisposable.Dispose();
    }
}
