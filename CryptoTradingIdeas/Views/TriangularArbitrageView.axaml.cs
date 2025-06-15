using Avalonia.ReactiveUI;
using CryptoTradingIdeas.Core.Interfaces.Services;
using CryptoTradingIdeas.ViewModel.ViewModels;

namespace CryptoTradingIdeas.Views;

public partial class TriangularArbitrageView :
    ReactiveUserControl<TriangularArbitrageViewModel>,
    ISingletonViewFor<TriangularArbitrageViewModel>
{
    public TriangularArbitrageView()
    {
        InitializeComponent();
    }
}