using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using charmera_importer.Localization;
using charmera_importer.ViewModels;

namespace charmera_importer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnBrowseDestinationClicked(object? sender, RoutedEventArgs e)
    {
        var storageProvider = StorageProvider;
        if (storageProvider is null || DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = LocalizedStrings.Instance.FolderPickerTitle,
            AllowMultiple = false,
        });

        var selected = folders.FirstOrDefault();
        if (selected?.TryGetLocalPath() is { } localPath)
        {
            viewModel.DestinationRootPath = localPath;
        }
    }
}
