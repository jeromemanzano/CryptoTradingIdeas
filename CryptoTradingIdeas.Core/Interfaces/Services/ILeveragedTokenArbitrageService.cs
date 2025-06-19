using CryptoTradingIdeas.Core.Injection;

namespace CryptoTradingIdeas.Core.Interfaces.Services;

public interface ILeveragedTokenArbitrageService : ISingletonDependency, IDisposable
{
    void StartUpdatingOpportunities();
}