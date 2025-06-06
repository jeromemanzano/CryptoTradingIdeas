using CryptoTradingIdeas.Core.Interfaces;
using ReactiveUI;

namespace CryptoTradingIdeas.Core.Services;

public class RoutableViewModelFactory : IRoutableViewModelFactory
{
    public TViewModel Create<TViewModel>() where TViewModel : IRoutableViewModel
    {
        throw new NotImplementedException();
    }
}