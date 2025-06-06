using CryptoTradingIdeas.Core.Injection;
using ReactiveUI;

namespace CryptoTradingIdeas.Core.Interfaces;

public interface IHostScreen : IActivatableViewModel, IScreen, ISingletonDependency
{
}