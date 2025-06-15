using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;

namespace CryptoTradingIdeas.MarkupExtensions;

/// <summary>
/// A markup extension that provides binding to enum values.
/// </summary>
/// <typeparam name="TEnum">The enum type to bind to.</typeparam>
public class EnumBinding<TEnum> : MarkupExtension where TEnum : struct, Enum
{
    [Content]
    public IList<TEnum> Except { get; } = new List<TEnum>();

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return Enum.GetValues<TEnum>().Except(Except);
    }
}
