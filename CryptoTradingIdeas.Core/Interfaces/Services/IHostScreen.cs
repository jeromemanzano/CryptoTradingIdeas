using CryptoTradingIdeas.Core.Injection;
using ReactiveUI;

namespace CryptoTradingIdeas.Core.Interfaces.Services;

public interface IHostScreen : IActivatableViewModel, IScreen, ISingletonDependency
{
}