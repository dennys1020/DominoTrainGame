using System.Windows;

namespace DominoTrainGame.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void CloseSettings(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
