using System.Collections.ObjectModel;
using CryptoTradingIdeas.Core.Interfaces.Services;
using CryptoTradingIdeas.Core.Models.Dtos;

namespace CryptoTradingIdeas.ViewModel.ViewModels;

public class LeveragedTokenArbitrageViewModel : ViewModelBase, ITabViewModel
{
    private readonly ILeveragedTokenArbitrageService _leveragedTokenArbitrageService;

    public string TabName => "Leveraged Token Arbitrage";
    public ReadOnlyObservableCollection<LeveragedTokenArbitrageOpportunity> LeveragedTokenArbitrageOpportunities =>
        _leveragedTokenArbitrageService.LeveragedTokenArbitrageOpportunities;

    public LeveragedTokenArbitrageViewModel(
        ILeveragedTokenArbitrageService leveragedTokenArbitrageService)
    {
        _leveragedTokenArbitrageService = leveragedTokenArbitrageService;
    }

}