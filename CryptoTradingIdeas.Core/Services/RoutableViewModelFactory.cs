using CryptoTradingIdeas.Core.Interfaces.Services;
using ReactiveUI;
using Splat;

namespace CryptoTradingIdeas.Core.Services;

public class RoutableViewModelFactory : IRoutableViewModelFactory
{
    public TViewModel Create<TViewModel>() where TViewModel : IRoutableViewModel
    {
        return Locator.Current.GetService<TViewModel>()!;
    }
}