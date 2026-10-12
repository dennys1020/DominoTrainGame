using System;
using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class LeaderBoardWindow : Window
{
    public LeaderBoardWindow()
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.Leaderboard);
    }

    private async void OnLoaded(object sender, RoutedEventArgs args) =>
        await ((LeaderboardViewModel)DataContext).RefreshCommand.ExecuteAsync();

    private void OnClosed(object sender, EventArgs args) => ((LeaderboardViewModel)DataContext).Dispose();
}
