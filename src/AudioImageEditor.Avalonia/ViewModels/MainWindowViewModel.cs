using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using TagLib;
using File = TagLib.File;
using Picture = TagLib.Picture;

namespace AudioImageEditor.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly Window? _mainWindow;

    public MainWindowViewModel()
    {
        _mainWindow = null;
    }

    public MainWindowViewModel(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public Bitmap? AlbumArt { get; set; }

    public File? File { get; private set; }

    public string FileName { get; set; } = "No File Loaded";

    [RelayCommand]
    private async Task SaveImage()
    {
        if (_mainWindow is null || AlbumArt is null)
            return;

        // Save image to location
        IReadOnlyList<IStorageFolder> folderPicker = await _mainWindow.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions()
            {
                AllowMultiple = false,
                SuggestedStartLocation =
                    await _mainWindow.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Pictures)
            });

        // User canceled select folder prompt
        if (folderPicker.Count == 0)
            return;

        AlbumArt.Save(Path.Combine(folderPicker[0].Path.LocalPath, "cover.png"), 100);
    }

    [RelayCommand]
    private async Task SelectImage()
    {
        if (_mainWindow is null || File is null)
            return;

        IReadOnlyList<IStorageFile> folderPicker = await _mainWindow.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions()
            {
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Supported File Types")
                    {
                        Patterns = new[]
                        {
                            "*.bmp", "*.jpeg", "*.jpg", "*.pbm", "*.pgm", "*.ppm", "*.pnm", "*.pcx", "*.png", "*.tiff", "*.dng"
                        }
                    }
                },
                SuggestedStartLocation =
                    await _mainWindow.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Pictures)
            });

        // User canceled select file prompt
        if (folderPicker.Count == 0)
            return;

        File.Tag.Pictures = new IPicture[]
        {
            new Picture(folderPicker[0].Path.LocalPath)
            {
                Type = PictureType.FrontCover
            }
        };

        File.Save();

        RefreshImage();
    }

    [RelayCommand]
    private void RemoveImage()
    {
        if (File is null)
            return;

        File.Tag.Pictures = null;
        File.Save();

        RefreshImage();
    }

    [RelayCommand]
    private async Task SelectFile()
    {
        if (_mainWindow is null)
            return;

        // Open file picker
        FilePickerOpenOptions options = new FilePickerOpenOptions()
        {
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType("Supported File Types")
                {
                    Patterns = new[]
                    {
                        "*.aa", "*.aax", "*.aac", "*.aiff", "*.ape", "*.dsf", "*.flac", "*.m4a", "*.m4b", "*.m4p",
                        "*.mp3", "*.mpc", "*.mpp", "*.ogg", "*.oga", "*.wav", "*.wma", "*.wv", "*.webm"
                    }
                }
            },
            SuggestedStartLocation = await _mainWindow.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Music)
        };

        IReadOnlyList<IStorageFile> folderPicker = await _mainWindow.StorageProvider.OpenFilePickerAsync(options);

        // User canceled select file prompt
        if (folderPicker.Count == 0)
            return;

        FileName = Path.GetFileNameWithoutExtension(folderPicker[0].Path.LocalPath);
        OnPropertyChanged(nameof(FileName));

        // Load file
        File = File.Create(folderPicker[0].Path.LocalPath);
        OnPropertyChanged(nameof(File));

        RefreshImage();
    }

    private void RefreshImage()
    {
        if (File is null)
            return;

        if (File.Tag.Pictures.Length == 0)
            AlbumArt = null;
        else
        {
            using (MemoryStream stream = new MemoryStream(File.Tag.Pictures[0].Data.Data))
            {
                try
                {
                    AlbumArt = new Bitmap(stream);
                }
                catch (ArgumentException)
                {
                    AlbumArt = null;
                }
            }
        }

        OnPropertyChanged(nameof(AlbumArt));
    }
}
