using System.Windows;
using System.Windows.Controls;
using DominoTrainGame.Navigation;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class RoomWindow : Window
{
    private const string LocalPlayerId = "local-player";

    public RoomWindow() : this(false)
    {
    }

    public RoomWindow(bool isHost) : this(new RoomViewModel(), isHost)
    {
        Room.Players.Add(new RoomPlayerViewModel(LocalPlayerId, string.Empty)
        {
            IsCurrentPlayer = true,
            IsHost = isHost
        });
    }

    public RoomWindow(RoomViewModel room, bool isHost)
    {
        Room = room;
        IsHost = isHost;
        InitializeComponent();
    }

    public RoomViewModel Room { 
        get; 
    }

    public bool IsHost { 
        get; 
    }

    private void MarkReady(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is RoomPlayerViewModel player &&
            player.IsCurrentPlayer && Room.Players.Contains(player))
        {
            player.IsReady = true;
        }
    }

    private void NavigateBack(object sender, RoutedEventArgs e)
    {
        WindowNavigation.Navigate(this, new NewGameWindow());
    }

    private void OpenRules(object sender, RoutedEventArgs e)
    {
        GameRulesWindow rulesWindow = new GameRulesWindow
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false
        };
        rulesWindow.ShowDialog();
    }

    private void OpenResults(object sender, RoutedEventArgs e)
    {
        if (IsHost)
        {
            WindowNavigation.Navigate(this, new MatchResultsWindow(Room, IsHost));
        }
    }
}
