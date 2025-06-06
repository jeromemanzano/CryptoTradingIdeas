using ReactiveUI;

namespace CryptoTradingIdeas.ViewModel.ViewModels;

public class TriadViewModel : ViewModelBase, IRoutableViewModel
{
    public string? UrlPathSegment { get; } = nameof(TriadViewModel);
    public IScreen HostScreen { get; }
}