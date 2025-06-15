using System.Collections.ObjectModel;
using CryptoTradingIdeas.Core.Injection;
using CryptoTradingIdeas.Core.Models.Dtos;

namespace CryptoTradingIdeas.Core.Interfaces.Services;

/// <summary>
/// Service interface for handling triangular arbitrage operations in cryptocurrency trading.
/// Triangular arbitrage involves taking advantage of price differences between three different cryptocurrencies
/// on the same exchange to make a profit.
/// </summary>
public interface ITriangularArbitrageService : ISingletonDependency, IDisposable
{
    /// <summary>
    /// Gets a read-only observable collection of triangular arbitrage opportunities and their potential gains.
    /// </summary>
    ReadOnlyObservableCollection<TriangularArbitrageOpportunity> TriangularArbitrageOpportunities { get; }
}