using CryptoTradingIdeas.Core.Enums;
using CryptoTradingIdeas.Core.Injection;

namespace CryptoTradingIdeas.Core.Interfaces.Services;

/// <summary>
/// Represents a service for interacting with cryptocurrency exchanges and streaming market data.
/// This interface provides functionality for real-time spot market data streaming.
/// </summary>
public interface IExchangeService : IDisposable, ISingletonDependency
{
    /// <summary>
    /// The exchange this service interacts with.
    /// </summary>
    Exchanges Exchange { get; }

    /// <summary>
    /// Starts streaming market data from the exchange.
    /// </summary>
    /// <remarks>
    /// This method should be called to initiate the market data streaming process.
    /// The streaming will continue until the service is disposed.
    /// </remarks>
    Task StartStreamingAsync();

    /// <summary>
    /// Gets the current market ask orders for a specified trading pair.
    /// </summary>
    /// <param name="unifiedPairSymbol">The unified trading pair symbol (e.g., "BTCUSDT").</param>
    /// <returns>
    /// An array of tuples containing the price and quantity of ask orders, sorted by price in ascending order.
    /// </returns>
    Task<(decimal Price, decimal Quantity)[]> GetMarketAsksAsync(string unifiedPairSymbol);

    /// <summary>
    /// Gets the current market bid orders for a specified trading pair.
    /// </summary>
    /// <param name="unifiedPairSymbol">The unified trading pair symbol (e.g., "BTCUSDT").</param>
    /// <returns>
    /// An array of tuples containing the price and quantity of bid orders, sorted by price in descending order.
    /// </returns>
    Task<(decimal Price, decimal Quantity)[]> GetMarketBidsAsync(string unifiedPairSymbol);
}