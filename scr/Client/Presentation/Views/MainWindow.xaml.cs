using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.Main);
    }
}
