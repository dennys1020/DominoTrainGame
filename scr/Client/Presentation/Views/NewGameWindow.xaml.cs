using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class NewGameWindow : Window
{
    public NewGameWindow()
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.NewGame);
    }
}
