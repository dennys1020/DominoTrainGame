using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class MatchResultsWindow : Window
{
    public MatchResultsWindow()
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.MatchResults);
    }
}
