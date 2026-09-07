using System;
using Avalonia.Data;

namespace charmera_importer.Localization;

// Lets XAML write Text="{loc:Loc DeviceLabel}" instead of a verbose
// {Binding Source={x:Static loc:LocalizedStrings.Instance}, Path=DeviceLabel} —
// resolves to a OneWay Binding against the singleton, so it updates automatically
// whenever LocalizedStrings.Apply() switches languages.
public sealed class LocExtension
{
    public LocExtension()
    {
    }

    public LocExtension(string path)
    {
        Path = path;
    }

    public string Path { get; set; } = string.Empty;

    public object ProvideValue(IServiceProvider serviceProvider) => new Binding(Path)
    {
        Source = LocalizedStrings.Instance,
        Mode = BindingMode.OneWay,
    };
}
