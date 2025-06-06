using System;
using CryptoTradingIdeas.ViewModel.ViewModels;
using CryptoTradingIdeas.Views;
using ReactiveUI;

namespace CryptoTradingIdeas;

public class AppViewLocator : IViewLocator
{
    public IViewFor ResolveView<T>(T? viewModel, string? contract = null) => viewModel switch
    {
        TriadViewModel context => new TriadView { DataContext = context },
        _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
    };
}
