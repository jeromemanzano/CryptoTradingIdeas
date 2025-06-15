using CryptoTradingIdeas.Core.Injection;

namespace CryptoTradingIdeas.Core.Interfaces.Services;

public interface ISingletonViewFor<T> : ISingletonDependency, IViewFor<T> where T : class
{
}