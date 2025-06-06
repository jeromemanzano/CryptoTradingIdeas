using CryptoTradingIdeas.Core.Injection;
using CryptoTradingIdeas.Core.Models;

namespace CryptoTradingIdeas.Core.Interfaces;

/// <summary>
/// Represents a service for interacting with cryptocurrency exchanges and streaming market data.
/// This interface provides functionality for real-time spot market data streaming.
/// </summary>
public interface IExchangeService : IDisposable, ISingletonDependency
{
    /// <summary>
    /// Gets an observable stream of the latest spot market data.
    /// </summary>
    /// <remarks>
    /// This stream provides real-time updates of spot market data from the exchange after
    /// <see cref="StartStreamingAsync"/> runs to completion
    /// </remarks>
    IObservable<SpotData> SpotLatestDataStream { get; }

    /// <summary>
    /// Starts streaming market data from the exchange.
    /// </summary>
    /// <returns>A task representing the asynchronous operation of starting the data stream.</returns>
    /// <remarks>
    /// This method should be called to initiate the market data streaming process.
    /// The streaming will continue until the service is disposed.
    /// </remarks>
    Task StartStreamingAsync();
}