using CryptoTradingIdeas.Core.Injection;
using ReactiveUI;

namespace CryptoTradingIdeas.Core.Interfaces;

public interface IRoutableViewModelFactory : ISingletonDependency
{
    TViewModel Create<TViewModel>() where TViewModel : IRoutableViewModel;
}