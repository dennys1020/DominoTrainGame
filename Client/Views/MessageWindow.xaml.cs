using System.Windows;
using System.Windows.Input;

namespace DominoTrainGame.Client.View.Views;

public sealed partial class MessageWindow : Window
{
    public MessageWindow()
    {
        InitializeComponent();
    }

    private void OnYesButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void OnWindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}
