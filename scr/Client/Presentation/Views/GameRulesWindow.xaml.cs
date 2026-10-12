using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class GameRulesWindow : Window
{
    public GameRulesWindow()
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.Rules);
    }
}
