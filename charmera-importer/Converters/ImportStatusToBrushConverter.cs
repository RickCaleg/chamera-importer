using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using charmera_importer.Models;

namespace charmera_importer.Converters;

// Colors mirror the status palette defined in Styles/Theme.axaml.
public sealed class ImportStatusToBrushConverter : IValueConverter
{
    public static readonly ImportStatusToBrushConverter Instance = new();

    private static readonly IBrush ImportedBrush = new SolidColorBrush(Color.Parse("#16A34A"));
    private static readonly IBrush DuplicateBrush = new SolidColorBrush(Color.Parse("#D97706"));
    private static readonly IBrush ErrorBrush = new SolidColorBrush(Color.Parse("#DC2626"));
    private static readonly IBrush PendingBrush = new SolidColorBrush(Color.Parse("#9CA3AF"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            ImportStatus.Imported => ImportedBrush,
            ImportStatus.Duplicate => DuplicateBrush,
            ImportStatus.Error => ErrorBrush,
            _ => PendingBrush,
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
