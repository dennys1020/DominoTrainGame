using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class ProfileWindow : Window
{
    public ProfileWindow()
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.Profile);
    }
}
