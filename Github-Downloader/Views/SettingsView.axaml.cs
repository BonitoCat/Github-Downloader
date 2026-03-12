using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using FileLib;
using Github_Downloader_lib;
using Github_Downloader.Enums;
using Github_Downloader.ViewModels;
using LoggerLib;

namespace Github_Downloader.Views;

public partial class SettingsView : UserControl
{
    private readonly MainViewModel _mainViewModel;
    
    public SettingsView()
    {
        InitializeComponent();
        _mainViewModel = ((App)Application.Current!).MainViewModel;
    }

    private async void BtnExport_OnClick(object? sender, RoutedEventArgs e)
    {
        Logger.LogI("Exporting repo.json");
        
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }
        
        IReadOnlyList<IStorageFolder> folders = await topLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Select folder",
                AllowMultiple = false
            });

        if (folders.Count <= 0) return;
        
        string path = folders[0].Path.LocalPath;

        FileManager.ExportRepoConfig(path);
    }

    private async void BtnImport_OnClick(object? sender, RoutedEventArgs e)
    {
        Logger.LogI("Selecting file");

        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Select file",
                AllowMultiple = false,
            });

        if (files.Count == 0) return;

        string path = files[0].Path.LocalPath;

        FileManager.ImportRepoConfig(path);
    }

    private void ImgBack_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _mainViewModel.SwitchPage(ViewNames.Home);
    }
}