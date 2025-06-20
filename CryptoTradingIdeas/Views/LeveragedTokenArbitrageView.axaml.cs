using Avalonia.ReactiveUI;
using CryptoTradingIdeas.Core.Interfaces.Services;
using CryptoTradingIdeas.ViewModel.ViewModels;

namespace CryptoTradingIdeas.Views;

public partial class LeveragedTokenArbitrageView :
    ReactiveUserControl<LeveragedTokenArbitrageViewModel>,
    ISingletonViewFor<LeveragedTokenArbitrageViewModel>
{
    public LeveragedTokenArbitrageView()
    {
        InitializeComponent();
    }
} 