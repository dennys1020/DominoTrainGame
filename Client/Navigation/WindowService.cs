using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using log4net.Ext.Trace;
using Microsoft.Win32;
using DominoTrainGame.Client.View.Behaviors;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.View.Navigation;

internal sealed class WindowService : IWindowService
{
    private const string AvatarImageFilterKey = "ProfileAvatarFileFilter";
    private const string WebPageUnavailableMessageKey = "WebPageUnavailableMessage";

    private static readonly ITraceLog _logger;

    private readonly Window _window;
    private readonly WindowFactory _windowFactory;
    private readonly LocalizationViewModel _localization;

    static WindowService()
    {
        _logger = TraceLogManager.GetLogger(typeof(WindowService));
    }

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
        ShowOwnedDialog(dialog);
    }

    public void ShowPlayerProfile(string username)
    {
        Window profileDialog = _windowFactory.CreatePlayerProfileDialog(username);
        ShowOwnedDialog(profileDialog);
    }

    public void ShowMessage(string messageKey)
    {
        string message = _localization[messageKey];
        ShowMessageWindow(message, MessageKind.Information);
    }

    public void ShowMessageError(string messageKey)
    {
        string message = _localization[messageKey];
        ShowMessageWindow(message, MessageKind.Error);
    }

    public bool Confirm(string message)
    {
        bool isConfirmed = ShowMessageWindow(message, MessageKind.Confirmation);
        return isConfirmed;
    }

    public bool ConfirmMessage(string messageKey)
    {
        string message = _localization[messageKey];
        bool isConfirmed = Confirm(message);
        return isConfirmed;
    }

    public byte[] SelectAvatarImageFile()
    {
        OpenFileDialog imageFileDialog = new OpenFileDialog
        {
            Filter = _localization[AvatarImageFilterKey],
            CheckFileExists = true,
            Multiselect = false
        };
        bool isFileSelected = imageFileDialog.ShowDialog(_window).GetValueOrDefault();
        byte[] imageBytes = isFileSelected ? ReadAvatarImageFile(imageFileDialog.FileName) : Array.Empty<byte>();

        return imageBytes;
    }

    public void OpenWebPage(string address)
    {
        try
        {
            ProcessStartInfo browserStart = new ProcessStartInfo(address)
            {
                UseShellExecute = true
            };
            Process.Start(browserStart)?.Dispose();
        }
        catch (Win32Exception exception)
        {
            _logger.Warn("The default browser could not open a social network page.", exception);
            ShowMessageError(WebPageUnavailableMessageKey);
        }
    }

    public void Close()
    {
        _window.Close();
    }

    private static byte[] ReadAvatarImageFile(string imageFilePath)
    {
        byte[] imageBytes = Array.Empty<byte>();

        try
        {
            imageBytes = File.ReadAllBytes(imageFilePath);
        }
        catch (IOException exception)
        {
            _logger.Warn("The selected avatar image could not be read.", exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.Warn("The selected avatar image could not be read because access was denied.", exception);
        }

        return imageBytes;
    }

    private void ShowOwnedDialog(Window dialog)
    {
        dialog.Owner = _window;
        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dialog.ShowInTaskbar = false;
        dialog.ShowDialog();
    }

    private bool ShowMessageWindow(string message, MessageKind kind)
    {
        Window messageWindow = _windowFactory.CreateMessageWindow(message, kind);
        bool canOwnMessageWindow = _window.IsLoaded;

        if (canOwnMessageWindow)
        {
            messageWindow.Owner = _window;
            messageWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            messageWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        bool isAccepted = messageWindow.ShowDialog().GetValueOrDefault();
        return isAccepted;
    }

    private void ReplaceCurrentWindow(Window nextWindow)
    {
        bool isCurrentWindowFullScreen = WindowFullScreenBehavior.GetIsFullScreenActive(_window);

        nextWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowFullScreenBehavior.SetIsFullScreenActive(nextWindow, isCurrentWindowFullScreen);
        nextWindow.WindowState = _window.WindowState;
        Application.Current.MainWindow = nextWindow;
        nextWindow.Show();
        _window.Close();
    }
}
