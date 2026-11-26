using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;
using DominoTrainGame.Shared.Common;

namespace DominoTrainGame.Client.ViewModel;

public sealed class MatchViewModel : ScreenViewModel, ISubscribingViewModel
{
    private const string LeaveConfirmationMessageKey = "MatchLeaveConfirmationMessage";
    private const string MatchAbandonedMessageKey = "MatchAbandonedMessage";
    private const int RoundNoticeSeconds = 3;
    private static readonly TimeSpan _roundNoticeDuration = TimeSpan.FromSeconds(RoundNoticeSeconds);

    private readonly IRemoteGameServices _gameServices;
    private readonly IRemoteMatchServices _matchServices;
    private readonly IMatchEvents _matchEvents;
    private readonly ILobbyEvents _lobbyEvents;
    private readonly AsyncParameterRelayCommand<string> _closeMatchCommand;
    private readonly AsyncRelayCommand _showRoundNoticeCommand;

    private int _enginePips;
    private int _boneyardCount;
    private int _lastRoundEnginePips;
    private bool _isLocalPlayerTurn;
    private bool _hasRoundSummary;
    private bool _isRoundNoticeVisible;
    private bool _isReconnecting;
    private string _turnPlayerUsername = string.Empty;
    private string _lastRoundWinnerUsername = string.Empty;

    public MatchViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices,
        IRemoteMatchServices matchServices) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _matchServices = matchServices ?? throw new ArgumentNullException(nameof(matchServices));
        _matchEvents = _matchServices.MatchEvents;
        _lobbyEvents = _gameServices.LobbyEvents;

        DrawTileCommand = new AsyncRelayCommand(DrawTileAsync);
        PassTurnCommand = new AsyncRelayCommand(PassTurnAsync);
        LeaveMatchCommand = new AsyncRelayCommand(LeaveMatchAsync);
        _closeMatchCommand = new AsyncParameterRelayCommand<string>(CloseMatchAsync);
        _showRoundNoticeCommand = new AsyncRelayCommand(ShowRoundNoticeAsync);

        ApplyCurrentMatch();
        ApplyCurrentHand();

        _matchEvents.MatchChanged += HandleMatchChanged;
        _matchEvents.HandChanged += HandleHandChanged;
        _matchEvents.RoundEnded += HandleRoundEnded;
        _matchEvents.MatchFinished += HandleMatchFinished;
        _matchEvents.MatchAbandoned += HandleMatchAbandoned;
        _matchEvents.MatchLost += HandleMatchLost;
        _lobbyEvents.ConnectionStateChanged += HandleConnectionStateChanged;
    }

    public ObservableCollection<TileSummary> HandTiles { get; } = new ObservableCollection<TileSummary>();

    public ObservableCollection<TrainViewModel> Trains { get; } = new ObservableCollection<TrainViewModel>();

    public ObservableCollection<MatchPlayerViewModel> Players { get; } =
        new ObservableCollection<MatchPlayerViewModel>();

    public int EnginePips
    {
        get
        {
            return _enginePips;
        }
        private set
        {
            SetProperty(ref _enginePips, value);
        }
    }

    public int BoneyardCount
    {
        get
        {
            return _boneyardCount;
        }
        private set
        {
            SetProperty(ref _boneyardCount, value);
        }
    }

    public bool IsLocalPlayerTurn
    {
        get
        {
            return _isLocalPlayerTurn;
        }
        private set
        {
            SetProperty(ref _isLocalPlayerTurn, value);
        }
    }

    public string TurnPlayerUsername
    {
        get
        {
            return _turnPlayerUsername;
        }
        private set
        {
            SetProperty(ref _turnPlayerUsername, value);
        }
    }

    public int LastRoundEnginePips
    {
        get
        {
            return _lastRoundEnginePips;
        }
        private set
        {
            SetProperty(ref _lastRoundEnginePips, value);
        }
    }

    public string LastRoundWinnerUsername
    {
        get
        {
            return _lastRoundWinnerUsername;
        }
        private set
        {
            SetProperty(ref _lastRoundWinnerUsername, value ?? string.Empty);
        }
    }

    public bool HasRoundSummary
    {
        get
        {
            return _hasRoundSummary;
        }
        private set
        {
            SetProperty(ref _hasRoundSummary, value);
        }
    }

    public bool IsRoundNoticeVisible
    {
        get
        {
            return _isRoundNoticeVisible;
        }
        private set
        {
            SetProperty(ref _isRoundNoticeVisible, value);
        }
    }

    public bool IsReconnecting
    {
        get
        {
            return _isReconnecting;
        }
        private set
        {
            SetProperty(ref _isReconnecting, value);
        }
    }

    public AsyncRelayCommand DrawTileCommand { get; }

    public AsyncRelayCommand PassTurnCommand { get; }

    public AsyncRelayCommand LeaveMatchCommand { get; }

    public void Unsubscribe()
    {
        _matchEvents.MatchChanged -= HandleMatchChanged;
        _matchEvents.HandChanged -= HandleHandChanged;
        _matchEvents.RoundEnded -= HandleRoundEnded;
        _matchEvents.MatchFinished -= HandleMatchFinished;
        _matchEvents.MatchAbandoned -= HandleMatchAbandoned;
        _matchEvents.MatchLost -= HandleMatchLost;
        _lobbyEvents.ConnectionStateChanged -= HandleConnectionStateChanged;
    }

    private void ApplyCurrentMatch()
    {
        MatchSnapshot currentMatch = _matchServices.CurrentMatch;
        bool hasMatch = !currentMatch.IsEmpty;

        if (hasMatch)
        {
            ApplySnapshot(currentMatch);
        }
    }

    private void ApplyCurrentHand()
    {
        PlayerHand currentHand = _matchServices.CurrentHand;
        bool hasHand = !currentHand.IsEmpty;

        if (hasHand)
        {
            ReplaceHand(currentHand.Tiles);
        }
    }

    private void HandleMatchChanged(object sender, EventArgs eventArgs)
    {
        ApplyCurrentMatch();
    }

    private void HandleHandChanged(object sender, EventArgs eventArgs)
    {
        ApplyCurrentHand();
    }

    private void HandleRoundEnded(object sender, EventArgs eventArgs)
    {
        IReadOnlyList<RoundSummary> roundHistory = _matchServices.RoundHistory;

        if (roundHistory.Any())
        {
            ShowRoundSummary(roundHistory.Last());
        }
    }

    private void HandleMatchFinished(object sender, EventArgs eventArgs)
    {
        WindowService.Navigate(Screen.MatchResults);
    }

    private void HandleMatchAbandoned(object sender, EventArgs eventArgs)
    {
        RequestMatchClose(MatchAbandonedMessageKey);
    }

    private void HandleMatchLost(object sender, EventArgs eventArgs)
    {
        RequestMatchClose(MatchMessageKeys.GetMessageKey(MatchMoveResult.MatchNotFound));
    }

    private void HandleConnectionStateChanged(object sender, ConnectionStateChangedEventArgs eventArgs)
    {
        IsReconnecting = eventArgs.State == ConnectionState.Reconnecting;
    }

    private void RequestMatchClose(string messageKey)
    {
        if (_closeMatchCommand.CanExecute(messageKey))
        {
            _closeMatchCommand.Execute(messageKey);
        }
    }

    private void ShowRoundSummary(RoundSummary roundSummary)
    {
        LastRoundEnginePips = roundSummary.EnginePips;
        LastRoundWinnerUsername = UsernameList.Join(FindRoundWinnerUsernames(roundSummary));
        HasRoundSummary = true;

        if (_showRoundNoticeCommand.CanExecute(null))
        {
            _showRoundNoticeCommand.Execute(null);
        }
    }

    private List<string> FindRoundWinnerUsernames(RoundSummary roundSummary)
    {
        int lowestRoundScore = roundSummary.ScoresByPlayerId.Values.DefaultIfEmpty().Min();
        List<string> winnerUsernames = new List<string>();

        foreach (KeyValuePair<int, int> playerScore in roundSummary.ScoresByPlayerId)
        {
            if (playerScore.Value == lowestRoundScore)
            {
                winnerUsernames.Add(FindUsername(_matchServices.CurrentMatch.Players, playerScore.Key));
            }
        }

        return winnerUsernames;
    }

    private async Task ShowRoundNoticeAsync()
    {
        IsRoundNoticeVisible = true;
        await Task.Delay(_roundNoticeDuration);
        IsRoundNoticeVisible = false;
    }

    private async Task CloseMatchAsync(string messageKey)
    {
        WindowService.ShowMessage(messageKey);
        await RunWhileBusyAsync(_gameServices.LeaveLobbyAsync);
        WindowService.Navigate(Screen.Main);
    }

    private void ApplySnapshot(MatchSnapshot matchSnapshot)
    {
        EnginePips = matchSnapshot.EnginePips;
        BoneyardCount = matchSnapshot.BoneyardCount;
        IsLocalPlayerTurn = matchSnapshot.CurrentPlayerId == _gameServices.CurrentPlayerId;
        TurnPlayerUsername = FindUsername(matchSnapshot.Players, matchSnapshot.CurrentPlayerId);

        ReplaceTrains(matchSnapshot.Trains);
        ReplacePlayers(matchSnapshot.Players, matchSnapshot.CurrentPlayerId);
    }

    private void ReplaceHand(IReadOnlyList<TileSummary> tiles)
    {
        HandTiles.Clear();

        foreach (TileSummary tile in tiles)
        {
            HandTiles.Add(tile);
        }
    }

    private void ReplaceTrains(IReadOnlyList<TrainSummary> trains)
    {
        Trains.Clear();

        foreach (TrainSummary train in trains)
        {
            bool isLocalPlayerTrain = IsLocalPlayerTrain(train);
            bool canReceiveTile = IsLocalPlayerTurn && IsTrainAvailable(train);
            ICommand dropTileCommand = CreateDropTileCommand(train.TrainId);
            TrainViewModel trainViewModel = new TrainViewModel(
                train,
                isLocalPlayerTrain,
                canReceiveTile,
                dropTileCommand);

            if (isLocalPlayerTrain)
            {
                Trains.Insert(CollectionIndex.First, trainViewModel);
            }
            else
            {
                Trains.Add(trainViewModel);
            }
        }
    }

    private void ReplacePlayers(IReadOnlyList<MatchPlayerSummary> players, int turnPlayerId)
    {
        Players.Clear();

        foreach (MatchPlayerSummary player in players)
        {
            Players.Add(new MatchPlayerViewModel(player, turnPlayerId));
        }
    }

    private bool IsLocalPlayerTrain(TrainSummary train)
    {
        bool isLocalPlayerTrain = !train.IsMexicanTrain && train.Owner.PlayerId == _gameServices.CurrentPlayerId;
        return isLocalPlayerTrain;
    }

    private bool IsTrainAvailable(TrainSummary train)
    {
        bool isAvailable = IsLocalPlayerTrain(train) || train.IsMexicanTrain || train.IsOpen;
        return isAvailable;
    }

    private ICommand CreateDropTileCommand(int trainId)
    {
        ICommand dropTileCommand = new AsyncParameterRelayCommand<int>(tileId => PlayTileAsync(tileId, trainId));
        return dropTileCommand;
    }

    private Task PlayTileAsync(int tileId, int trainId)
    {
        Task playTileTask = RunMoveAsync(() => _matchServices.PlayTileAsync(tileId, trainId));
        return playTileTask;
    }

    private Task DrawTileAsync()
    {
        Task drawTileTask = RunMoveAsync(() => _matchServices.DrawTileAsync());
        return drawTileTask;
    }

    private Task PassTurnAsync()
    {
        Task passTurnTask = RunMoveAsync(() => _matchServices.PassTurnAsync());
        return passTurnTask;
    }

    private async Task RunMoveAsync(Func<Task<MatchMoveResult>> sendMove)
    {
        MatchMoveResult moveResult = await RunWhileBusyAsync(sendMove);

        if (moveResult != MatchMoveResult.Success)
        {
            WindowService.ShowMessageError(MatchMessageKeys.GetMessageKey(moveResult));
        }
    }

    private async Task LeaveMatchAsync()
    {
        bool isLeaveConfirmed = WindowService.ConfirmMessage(LeaveConfirmationMessageKey);

        if (isLeaveConfirmed)
        {
            await RunWhileBusyAsync(_matchServices.LeaveMatchAsync);
            await RunWhileBusyAsync(_gameServices.LeaveLobbyAsync);
            WindowService.Navigate(Screen.Main);
        }
    }

    private static string FindUsername(IReadOnlyList<MatchPlayerSummary> players, int playerId)
    {
        string username = players
            .Where(player => player.Player.PlayerId == playerId)
            .Select(player => player.Player.Username)
            .FirstOrDefault();
        return username ?? string.Empty;
    }
}
