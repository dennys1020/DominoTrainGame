using System;
using System.Windows;
using DominoTrainGame.ViewModels.Localization;
using DominoTrainGame.ViewModels.Navigation;

namespace DominoTrainGame.Navigation;

internal sealed class WindowService : IWindowService
{
    private const string MessageTitleKey = "GameTitle";
    private readonly Window _window;

    public WindowService(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }

    public void Navigate(Screen screen)
    {
        Window nextWindow = WindowFactory.Create(screen);
        ReplaceCurrentWindow(nextWindow);
    }

    public void Navigate(Screen screen, RoomRole roomRole)
    {
        Window nextWindow = WindowFactory.Create(screen, roomRole);
        ReplaceCurrentWindow(nextWindow);
    }

    public void ShowDialog(Screen screen)
    {
        Window dialog = WindowFactory.Create(screen);

        dialog.Owner = _window;
        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dialog.ShowInTaskbar = false;
        dialog.ShowDialog();
    }

    public void ShowMessage(string message)
    {
        string title = LocalizationViewModel.Instance[MessageTitleKey];
        MessageBox.Show(_window, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void Close()
    {
        _window.Close();
    }

    private void ReplaceCurrentWindow(Window nextWindow)
    {
        nextWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        nextWindow.WindowState = _window.WindowState;
        Application.Current.MainWindow = nextWindow;
        nextWindow.Show();
        _window.Close();
    }
}
