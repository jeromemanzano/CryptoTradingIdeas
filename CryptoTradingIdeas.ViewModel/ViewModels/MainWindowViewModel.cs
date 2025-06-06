using CryptoTradingIdeas.Core.Injection;
using CryptoTradingIdeas.Core.Interfaces;
using ReactiveUI;

namespace CryptoTradingIdeas.ViewModel.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen, ISingletonDependency
{
    public RoutingState Router { get; } = new ();

    public MainWindowViewModel(IRoutableViewModelFactory routableViewModelFactory)
    {
        Router.Navigate.Execute(routableViewModelFactory.Create<TriadViewModel>());
    }
}