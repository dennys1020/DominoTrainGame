using System.Windows;

namespace DominoTrainGame.Navigation;

internal static class WindowNavigation
{
    public static void Navigate(Window currentWindow, Window nextWindow)
    {
        nextWindow.WindowStartupLocation = WindowStartupLocation.Manual;
        nextWindow.Left = currentWindow.Left;
        nextWindow.Top = currentWindow.Top;
        nextWindow.WindowState = currentWindow.WindowState;
        Application.Current.MainWindow = nextWindow;
        nextWindow.Show();
        currentWindow.Close();
    }
}
