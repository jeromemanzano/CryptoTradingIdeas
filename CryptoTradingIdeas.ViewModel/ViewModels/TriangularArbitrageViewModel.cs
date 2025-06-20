using System.Collections.ObjectModel;
using CryptoTradingIdeas.Core.Interfaces;
using CryptoTradingIdeas.Core.Interfaces.Services;
using CryptoTradingIdeas.Core.Models.Dtos;

namespace CryptoTradingIdeas.ViewModel.ViewModels;

public class TriangularArbitrageViewModel : ViewModelBase, ITabViewModel
{
    private readonly ITriangularArbitrageService _triangularArbitrageService;

    public string TabName => "Triangular Arbitrage";

    public ReadOnlyObservableCollection<TriangularArbitrageOpportunity> TriangularArbitrageOpportunities =>
        _triangularArbitrageService.TriangularArbitrageOpportunities;

    public TriangularArbitrageViewModel(
        ITriangularArbitrageService triangularArbitrageService)
    {
        _triangularArbitrageService = triangularArbitrageService;
    }
}
