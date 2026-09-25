using System.Windows;
using System.Windows.Controls;

namespace DominoTrainGame.Views.Controls;

public partial class SettingsButton : UserControl
{
    public SettingsButton()
    {
        InitializeComponent();
    }

    private void OpenSettings(object sender, RoutedEventArgs e)
    {
        SettingsWindow settingsWindow = new SettingsWindow
        {
            Owner = Window.GetWindow(this)
        };
        settingsWindow.ShowDialog();
    }
}
