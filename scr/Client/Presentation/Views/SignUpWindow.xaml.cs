using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class SignUpWindow : Window
{
    public SignUpWindow()
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.SignUp);
    }
}
