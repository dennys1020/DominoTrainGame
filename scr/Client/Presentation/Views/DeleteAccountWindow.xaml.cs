using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class DeleteAccountWindow : Window
{
    public DeleteAccountWindow()
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.DeleteAccount);
    }
}
