using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class ChangePasswordWindow : Window
{
    public ChangePasswordWindow()
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.ChangePassword);
    }
}
