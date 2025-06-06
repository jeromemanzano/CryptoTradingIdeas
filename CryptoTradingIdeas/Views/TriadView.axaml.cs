using Avalonia.ReactiveUI;
using CryptoTradingIdeas.Core.Injection;
using CryptoTradingIdeas.ViewModel.ViewModels;

namespace CryptoTradingIdeas.Views;

public partial class TriadView : ReactiveUserControl<TriadViewModel>, ISingletonDependency
{
    public TriadView()
    {
        InitializeComponent();
    }
}