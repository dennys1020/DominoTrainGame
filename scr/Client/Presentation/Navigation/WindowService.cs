using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Navigation;

internal sealed class WindowService : IWindowService
{
    private readonly Window _owner;
    public WindowService(Window owner) { _owner = owner; }
    public bool IsVisible => _owner.IsVisible;

    public void Navigate(Screen screen, object context = null, bool preservePosition = true)
    {
        Window next = ClientComposition.CreateWindow(screen, context);
        if (preservePosition) { WindowNavigation.Navigate(_owner, next); }
        else
        {
            Application.Current.MainWindow = next;
            next.Show();
            _owner.Close();
        }
    }

    public bool? ShowDialog(Screen screen, object context = null)
    {
        Window dialog = ClientComposition.CreateWindow(screen, context);
        dialog.Owner = _owner;
        if (screen == Screen.Rules)
        {
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowInTaskbar = false;
        }
        return dialog.ShowDialog();
    }

    public void ShowMessage(string message) => MessageBox.Show(message);

    public void Close(bool? dialogResult = null)
    {
        if (dialogResult.HasValue) { _owner.DialogResult = dialogResult; }
        else { _owner.Close(); }
    }
}
