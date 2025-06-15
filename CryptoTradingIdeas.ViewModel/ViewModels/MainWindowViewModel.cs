using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CryptoTradingIdeas.Core.Interfaces.Services;
using ReactiveUI;

namespace CryptoTradingIdeas.ViewModel.ViewModels;

public class MainWindowViewModel : ReactiveObject, IHostScreen
{
    public RoutingState Router { get; } = new ();

    public MainWindowViewModel(
        IEnumerable<IExchangeService> exchangeServices,
        IRoutableViewModelFactory routableViewModelFactory)
    {
        Router.Navigate.Execute(routableViewModelFactory.Create<TriangularArbitrageViewModel>());

        this.WhenActivated(disposables =>
        {
            exchangeServices
                .Select(service => service.StartStreamingAsync().ToObservable())
                .Merge(10)
                .Subscribe()
                .DisposeWith(disposables);
        });
    }

    public ViewModelActivator Activator { get; } = new();
}