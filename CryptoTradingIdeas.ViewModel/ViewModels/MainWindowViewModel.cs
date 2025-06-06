using System.Reactive.Disposables;
using CryptoTradingIdeas.Core.Interfaces;
using ReactiveUI;

namespace CryptoTradingIdeas.ViewModel.ViewModels;

public class MainWindowViewModel : ReactiveObject, IHostScreen
{
    public RoutingState Router { get; } = new ();

    public MainWindowViewModel(
        IRoutableViewModelFactory routableViewModelFactory,
        ISpotDataCacheManager spotDataCacheManager)
    {
        Router.Navigate.Execute(routableViewModelFactory.Create<TriadViewModel>());

        this.WhenActivated(disposables =>
        {
            spotDataCacheManager.Initialize();
            spotDataCacheManager.DisposeWith(disposables);
        });
    }

    public ViewModelActivator Activator { get; } = new();
}