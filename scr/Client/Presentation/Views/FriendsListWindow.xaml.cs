using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class FriendsListWindow : Window
{
    public FriendsListWindow()
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.Friends);
    }
}
