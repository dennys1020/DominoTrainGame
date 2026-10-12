using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class RecoverPasswordWindow : Window
{
    public RecoverPasswordWindow()
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.RecoverPassword);
    }
}
