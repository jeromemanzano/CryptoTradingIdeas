using CryptoTradingIdeas.Core.Injection;
using ReactiveUI;

namespace CryptoTradingIdeas.ViewModel.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen, ISingletonDependency
{
    public RoutingState Router { get; } = new ();

    public MainWindowViewModel()
    {
        Router.Navigate.Execute(new TriadViewModel());
    }
}