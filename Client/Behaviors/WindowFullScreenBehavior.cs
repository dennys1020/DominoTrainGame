using System.Windows;
using System.Windows.Input;

namespace DominoTrainGame.Client.View.Behaviors;

public static class WindowFullScreenBehavior
{
    private const string IsFullScreenToggleEnabledPropertyName = "IsFullScreenToggleEnabled";
    private const string IsFullScreenActivePropertyName = "IsFullScreenActive";

    public static readonly DependencyProperty IsFullScreenToggleEnabledProperty;
    public static readonly DependencyProperty IsFullScreenActiveProperty;

    static WindowFullScreenBehavior()
    {
        IsFullScreenToggleEnabledProperty = DependencyProperty.RegisterAttached(
            IsFullScreenToggleEnabledPropertyName,
            typeof(bool),
            typeof(WindowFullScreenBehavior),
            new PropertyMetadata(false, OnIsFullScreenToggleEnabledChanged));

        IsFullScreenActiveProperty = DependencyProperty.RegisterAttached(
            IsFullScreenActivePropertyName,
            typeof(bool),
            typeof(WindowFullScreenBehavior),
            new PropertyMetadata(false, OnIsFullScreenActiveChanged));
    }

    public static bool GetIsFullScreenToggleEnabled(DependencyObject target)
    {
        bool isFullScreenToggleEnabled = (bool)target.GetValue(IsFullScreenToggleEnabledProperty);
        return isFullScreenToggleEnabled;
    }

    public static void SetIsFullScreenToggleEnabled(DependencyObject target, bool value)
    {
        target.SetValue(IsFullScreenToggleEnabledProperty, value);
    }

    public static bool GetIsFullScreenActive(DependencyObject target)
    {
        bool isFullScreenActive = (bool)target.GetValue(IsFullScreenActiveProperty);
        return isFullScreenActive;
    }

    public static void SetIsFullScreenActive(DependencyObject target, bool value)
    {
        target.SetValue(IsFullScreenActiveProperty, value);
    }

    private static void OnIsFullScreenToggleEnabledChanged(
        DependencyObject target,
        DependencyPropertyChangedEventArgs eventArgs)
    {
        if (target is Window window)
        {
            window.PreviewKeyDown -= OnWindowPreviewKeyDown;

            bool isFullScreenToggleEnabled = (bool)eventArgs.NewValue;

            if (isFullScreenToggleEnabled)
            {
                window.PreviewKeyDown += OnWindowPreviewKeyDown;
            }
        }
    }

    private static void OnIsFullScreenActiveChanged(
        DependencyObject target,
        DependencyPropertyChangedEventArgs eventArgs)
    {
        if (target is Window window)
        {
            bool isFullScreenActive = (bool)eventArgs.NewValue;

            if (isFullScreenActive)
            {
                ShowWindowInFullScreen(window);
            }
            else
            {
                RestoreWindowToWindowedMode(window);
            }
        }
    }

    private static void OnWindowPreviewKeyDown(object sender, KeyEventArgs eventArgs)
    {
        Window window = (Window)sender;
        bool isFullScreenActive = GetIsFullScreenActive(window);
        bool isFullScreenToggleKey = eventArgs.Key == Key.F11;
        bool isFullScreenExitKey = isFullScreenActive && eventArgs.Key == Key.Escape;

        if (isFullScreenToggleKey || isFullScreenExitKey)
        {
            SetIsFullScreenActive(window, !isFullScreenActive);
            eventArgs.Handled = true;
        }
    }

    private static void ShowWindowInFullScreen(Window window)
    {
        window.WindowState = WindowState.Normal;
        window.WindowStyle = WindowStyle.None;
        window.ResizeMode = ResizeMode.NoResize;
        window.WindowState = WindowState.Maximized;
    }

    private static void RestoreWindowToWindowedMode(Window window)
    {
        window.WindowStyle = WindowStyle.SingleBorderWindow;
        window.ResizeMode = ResizeMode.CanResize;
        window.WindowState = WindowState.Normal;
    }
}
