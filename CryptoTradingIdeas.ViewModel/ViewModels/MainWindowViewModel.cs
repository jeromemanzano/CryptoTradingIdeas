using ReactiveUI;

namespace CryptoTradingIdeas.ViewModel.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; } = new ();

    public MainWindowViewModel()
    {
        Router.Navigate.Execute(new TriadViewModel());
    }
}