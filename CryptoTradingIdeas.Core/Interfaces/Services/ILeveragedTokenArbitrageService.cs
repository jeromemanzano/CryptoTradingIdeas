using System.Collections.ObjectModel;
using CryptoTradingIdeas.Core.Injection;
using CryptoTradingIdeas.Core.Models.Dtos;

namespace CryptoTradingIdeas.Core.Interfaces.Services;

/// <summary>
/// Service interface for managing and updating leveraged token arbitrage opportunities.
/// </summary>
public interface ILeveragedTokenArbitrageService : ISingletonDependency, IDisposable
{
    /// <summary>
    /// Starts the process of updating leveraged token arbitrage opportunities.
    /// </summary>
    void StartUpdatingOpportunities();

    /// <summary>
    /// Gets a read-only observable collection of current leveraged token arbitrage opportunities.
    /// </summary>
    ReadOnlyObservableCollection<LeveragedTokenArbitrageOpportunity> LeveragedTokenArbitrageOpportunities { get; }
}