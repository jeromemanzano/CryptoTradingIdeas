using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Injection;

namespace CryptoTradingIdeas.Core.Interfaces.Services;

public interface ISpotTradeService : ISingletonDependency
{
    Task<decimal> GetMarketSellingPriceAsync(Exchanges exchange, string pairSymbols, decimal baseAssetAmount);

    Task<decimal> GetMarketBuyingPriceAsync(Exchanges exchange, string pairSymbols, decimal quoteAssetAmount);
}