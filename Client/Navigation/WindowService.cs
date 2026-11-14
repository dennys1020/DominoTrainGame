using System;
using System.Windows;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.View.Navigation;

internal sealed class WindowService : IWindowService
{
    private const string MessageTitleKey = "GameTitle";

    private readonly Window _window;
    private readonly WindowFactory _windowFactory;
    private readonly LocalizationViewModel _localization;

    public WindowService(Window window, WindowFactory windowFactory, LocalizationViewModel localization)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _windowFactory = windowFactory ?? throw new ArgumentNullException(nameof(windowFactory));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
    }

    public void Navigate(Screen screen)
    {
        Window nextWindow = _windowFactory.Create(screen);
        ReplaceCurrentWindow(nextWindow);
    }

    public void ShowDialog(Screen screen)
    {
        Window dialog = _windowFactory.CreateDialog(screen);

        dialog.Owner = _window;
        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dialog.ShowInTaskbar = false;
        dialog.ShowDialog();
    }

    public void ShowMessage(string messageKey)
    {
        string title = _localization[MessageTitleKey];
        string message = _localization[messageKey];
        MessageBox.Show(_window, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public bool Confirm(string message)
    {
        string title = _localization[MessageTitleKey];
        MessageBoxResult answer = MessageBox.Show(
            _window,
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        bool isConfirmed = answer == MessageBoxResult.Yes;
        return isConfirmed;
    }

    public bool ConfirmMessage(string messageKey)
    {
        string message = _localization[messageKey];
        bool isConfirmed = Confirm(message);
        return isConfirmed;
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
