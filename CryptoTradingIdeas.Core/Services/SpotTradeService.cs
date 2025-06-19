using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Interfaces.Services;

namespace CryptoTradingIdeas.Core.Services;

public class SpotTradeService : ISpotTradeService
{
    private readonly Dictionary<Exchanges, IExchangeService> _exchangeServices;

    public SpotTradeService(IEnumerable<IExchangeService> exchangeServices)
    {
        _exchangeServices = exchangeServices.ToDictionary(service => service.Exchange, service => service);
    }

    public async Task<decimal> GetMarketSellingPriceAsync(Exchanges exchange, string pairSymbols, decimal baseAssetAmount)
    {
        var exchangeService = _exchangeServices[exchange];

        var marketBids = await exchangeService.GetMarketBidsAsync(pairSymbols);

        var quoteAssetAmount = 0m;

        foreach (var bid in marketBids)
        {
            if (bid.Quantity >= baseAssetAmount)
            {
                quoteAssetAmount += baseAssetAmount * bid.Price;
                baseAssetAmount = 0m;
                break;
            }

            quoteAssetAmount += bid.Price * bid.Quantity;
            baseAssetAmount -= bid.Quantity;
        }

        if (baseAssetAmount > 0m)
        {
            throw new InvalidOperationException(
                $"{pairSymbols} - " +
                $"Not enough bid prices to cover the base asset amount. Only {quoteAssetAmount} quote " +
                $"asset amount purchased with {baseAssetAmount} base asset amount left.");
        }

        return quoteAssetAmount;
    }

    public async Task<decimal> GetMarketBuyingPriceAsync(Exchanges exchange, string pairSymbols, decimal quoteAssetAmount)
    {
        var exchangeService = _exchangeServices[exchange];
        var marketAsks = await exchangeService.GetMarketAsksAsync(pairSymbols);

        var baseAssetAmount = 0m;

        foreach (var ask in marketAsks)
        {
            var askPriceAmount = ask.Price * ask.Quantity;

            if (askPriceAmount >= quoteAssetAmount)
            {
                baseAssetAmount += quoteAssetAmount / ask.Price;
                quoteAssetAmount = 0m;
                break;
            }

            baseAssetAmount += ask.Quantity;
            quoteAssetAmount -= askPriceAmount;
        }

        if (quoteAssetAmount > 0m)
        {
            throw new InvalidOperationException(
                $"{pairSymbols} - " +
                $"Not enough ask prices to cover the quote asset amount. Only {baseAssetAmount} base " +
                $"asset amount can be bought with {quoteAssetAmount} quote asset amount left.");
        }

        return baseAssetAmount;
    }
}