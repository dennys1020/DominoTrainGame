using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class LeaderBoardWindow : Window
{
    private readonly LeaderboardViewModel _viewModel = new LeaderboardViewModel();
    private readonly CancellationTokenSource _loadCancellation = new CancellationTokenSource();

    public LeaderBoardWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private async void OnLoaded(object sender, RoutedEventArgs eventArgs)
    {
        await LoadLeaderboardAsync();
    }

    private async void RefreshLeaderboard(object sender, RoutedEventArgs eventArgs)
    {
        await LoadLeaderboardAsync();
    }

    private async Task LoadLeaderboardAsync()
    {
        CancellationToken cancellationToken = _loadCancellation.Token;
        try
        {
            await _viewModel.LoadAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The window has closed, so there is no remaining result to display.
        }
    }

    private void OnClosed(object sender, EventArgs eventArgs)
    {
        _loadCancellation.Cancel();
        _loadCancellation.Dispose();
    }
}
