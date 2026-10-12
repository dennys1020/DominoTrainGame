using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class LogInWindow : Window
{
    public LogInWindow()
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.Login);
    }
}
