using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CryptoTradingIdeas.ViewModel.ViewModels;
using CryptoTradingIdeas.Views;
using Microsoft.Extensions.DependencyInjection;
using CryptoTradingIdeas.Core.Injection; // Reference used for source generated RegisterApplicationServices

namespace CryptoTradingIdeas;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();

            // Register services
            services.RegisterApplicationServices();

            _serviceProvider = services.BuildServiceProvider();

            desktop.MainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}