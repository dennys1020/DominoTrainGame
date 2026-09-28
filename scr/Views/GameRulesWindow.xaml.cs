using System.Windows;

namespace DominoTrainGame.Views;

public partial class GameRulesWindow : Window
{
    public GameRulesWindow()
    {
        InitializeComponent();
    }

    private void NavigateBack(object sender, RoutedEventArgs eventArgs)
    {
        Close();
    }
}