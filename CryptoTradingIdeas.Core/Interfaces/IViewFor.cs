using CryptoTradingIdeas.Core.Injection;

namespace CryptoTradingIdeas.Core.Interfaces;

public interface ISingletonViewFor<T> : ISingletonDependency, IViewFor<T> where T : class
{
}

public interface IViewFor<T> : ReactiveUI.IViewFor<T> where T : class
{
}