using System.Reactive.Disposables;
using CryptoTradingIdeas.Core.Interfaces;
using DynamicData.Alias;
using ReactiveUI;

namespace CryptoTradingIdeas.ViewModel.ViewModels;

public class TriadViewModel : ViewModelBase
{
    public TriadViewModel(
        ISpotDataCacheManager spotDataCacheManager)
    {
        this.WhenActivated(disposable =>
        {
            spotDataCacheManager
                .SpotDataCache
                .Connect()
                .Subscribe()
                .DisposeWith(disposable);
        });
    }
}