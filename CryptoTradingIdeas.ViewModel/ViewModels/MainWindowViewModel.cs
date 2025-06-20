using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CryptoTradingIdeas.Core.Interfaces.Services;
using ReactiveUI;

namespace CryptoTradingIdeas.ViewModel.ViewModels;

public class MainWindowViewModel : ReactiveObject, IHostScreen
{
    public RoutingState Router { get; } = new ();

    public IEnumerable<ITabViewModel> Tabs { get; }

    public MainWindowViewModel(
        IEnumerable<ITabViewModel> tabViewModels,
        IEnumerable<IExchangeService> exchangeServices,
        ILeveragedTokenArbitrageService leveragedTokenArbitrageService)
    {
        Tabs = tabViewModels;

        this.WhenActivated(disposables =>
        {
            exchangeServices
                .Select(service => service.StartStreamingAsync().ToObservable())
                .Merge(10)
                .Finally(leveragedTokenArbitrageService.StartUpdatingOpportunities)
                .Subscribe()
                .DisposeWith(disposables);
        });
    }

    public ViewModelActivator Activator { get; } = new();
}