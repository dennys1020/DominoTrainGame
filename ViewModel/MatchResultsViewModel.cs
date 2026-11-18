using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class MatchResultsViewModel : ScreenViewModel
{
    private const string WinnerSeparator = ", ";

    private readonly IRemoteGameServices _gameServices;

    public MatchResultsViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices,
        IRemoteMatchServices matchServices) : base(windowService)
    {
        if (gameServices == null)
        {
            throw new ArgumentNullException(nameof(gameServices));
        }

        if (matchServices == null)
        {
            throw new ArgumentNullException(nameof(matchServices));
        }

        _gameServices = gameServices;
        bool isHost = gameServices.CurrentLobby.HostPlayerId == gameServices.CurrentPlayerId;
        Screen previousScreen = isHost ? Screen.NewGame : Screen.JoinGame;
        IReadOnlyList<RoundSummary> roundHistory = matchServices.RoundHistory;

        RoundLabels = roundHistory.Select(round => round.EnginePips).ToList();
        Players = CreatePlayerResults(matchServices.CurrentMatch.Players, roundHistory);
        WinnerUsernames = string.Join(WinnerSeparator, Players.Where(row => row.IsWinner).Select(row => row.Username));

        BackCommand = new RelayCommand(() => WindowService.Navigate(previousScreen));
        MainMenuCommand = new AsyncRelayCommand(ReturnToMainMenuAsync);
        RoomCommand = new RelayCommand(() => WindowService.Navigate(Screen.Room));
    }

    public IReadOnlyList<int> RoundLabels { get; }

    public int RoundCount => RoundLabels.Count;

    public IReadOnlyList<PlayerResultViewModel> Players { get; }

    public bool HasPlayers => Players.Any();

    public string WinnerUsernames { get; }

    public bool HasWinner => HasPlayers && RoundLabels.Any();

    public RelayCommand BackCommand { get; }

    public AsyncRelayCommand MainMenuCommand { get; }

    public RelayCommand RoomCommand { get; }

    private static IReadOnlyList<PlayerResultViewModel> CreatePlayerResults(
            IReadOnlyList<MatchPlayerSummary> players,
            IReadOnlyList<RoundSummary> roundHistory)
    {
        Dictionary<int, IReadOnlyList<int>> roundScoresByPlayerId = players.ToDictionary(
            player => player.Player.PlayerId,
            player => GetRoundScores(roundHistory, player.Player.PlayerId));
        int winningTotal = roundScoresByPlayerId.Values
            .Select(roundScores => roundScores.Sum())
            .DefaultIfEmpty()
            .Min();
        List<PlayerResultViewModel> playerResults = players
            .Select(player => CreatePlayerResult(player.Player, roundScoresByPlayerId, winningTotal))
            .ToList();

        return playerResults;
    }

    private static PlayerResultViewModel CreatePlayerResult(
        PlayerSummary player,
        IReadOnlyDictionary<int, IReadOnlyList<int>> roundScoresByPlayerId,
        int winningTotal)
    {
        PlayerResultViewModel playerResult = new PlayerResultViewModel(
            player,
            roundScoresByPlayerId[player.PlayerId],
            winningTotal);
        return playerResult;
    }

    private static IReadOnlyList<int> GetRoundScores(IReadOnlyList<RoundSummary> roundHistory, int playerId)
    {
        List<int> roundScores = roundHistory.Select(round => GetRoundScore(round, playerId)).ToList();
        return roundScores;
    }

    private static int GetRoundScore(RoundSummary round, int playerId)
    {
        int roundScore;
        round.ScoresByPlayerId.TryGetValue(playerId, out roundScore);
        return roundScore;
    }

    private async Task ReturnToMainMenuAsync()
    {
        bool isInLobby = _gameServices.CurrentLobby.Status == LobbyActionResult.Success;

        if (isInLobby)
        {
            await RunWhileBusyAsync(_gameServices.LeaveLobbyAsync);
        }

        WindowService.Navigate(Screen.Main);
    }
}
