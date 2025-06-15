using System;
using ReactiveUI;
using Splat;

namespace CryptoTradingIdeas;

public class AppViewLocator : IViewLocator
{
    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        var viewType = typeof(Core.Interfaces.Services.IViewFor<>).MakeGenericType(viewModel.GetType());

        return Locator.Current.GetService(viewType) as IViewFor;
    }
}
