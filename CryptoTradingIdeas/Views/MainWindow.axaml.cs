using Avalonia.ReactiveUI;
using CryptoTradingIdeas.ViewModel.ViewModels;

namespace CryptoTradingIdeas.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}