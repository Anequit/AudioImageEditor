using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AudioImageEditor.Avalonia.ViewModels;
using AudioImageEditor.Avalonia.Views;

namespace AudioImageEditor.Avalonia;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();

            desktop.MainWindow.DataContext = new MainWindowViewModel(desktop.MainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }
}