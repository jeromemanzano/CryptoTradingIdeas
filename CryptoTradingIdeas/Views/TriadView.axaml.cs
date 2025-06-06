using Avalonia.ReactiveUI;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.ViewModel.ViewModels;

namespace CryptoTradingIdeas.Views;

public partial class TriadView : ReactiveUserControl<TriadViewModel>, ISingletonViewFor<TriadViewModel>
{
    public TriadView()
    {
        InitializeComponent();
    }
}