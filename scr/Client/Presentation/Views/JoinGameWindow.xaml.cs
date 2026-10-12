using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class JoinGameWindow : Window
{
    public JoinGameWindow()
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.JoinGame);
    }
}
