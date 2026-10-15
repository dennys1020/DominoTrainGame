using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class RoomWindow : Window
{
    public RoomWindow()
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.Room);
    }
}
