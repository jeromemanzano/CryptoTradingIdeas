﻿using Avalonia;
using System;
using ReactiveUI.Avalonia.Splat;
using CryptoTradingIdeas.Core.Injection; // Reference used for source generated RegisterApplicationServices

namespace CryptoTradingIdeas;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUIWithMicrosoftDependencyResolver(services =>
            {
                // Register services
                services.RegisterApplicationServices();
            })
            .WithInterFont()
            .LogToTrace();
}