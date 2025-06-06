using CryptoTradingIdeas.Core.Injection;
using ReactiveUI;

namespace CryptoTradingIdeas.ViewModel.ViewModels;

public abstract class ViewModelBase :
    ReactiveObject,
    IActivatableViewModel,
    IRoutableViewModel,
    ITransientDependency
{
    public string UrlPathSegment => Guid.NewGuid().ToString();

    public ViewModelActivator Activator { get; } = new();

    public IScreen HostScreen { get; }
}