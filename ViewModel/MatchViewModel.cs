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

namespace DominoTrainGame.Client.ViewModel;

public sealed class MatchViewModel : ScreenViewModel, ISubscribingViewModel
{
    private readonly IRemoteGameServices _gameServices;
    private readonly IRemoteMatchServices _matchServices;
    private readonly IMatchEvents _matchEvents;
    private int _enginePips;
    private int _boneyardCount;
    private bool _isMyTurn;
    private string _turnUsername = string.Empty;

    public MatchViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices,
        IRemoteMatchServices matchServices) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _matchServices = matchServices ?? throw new ArgumentNullException(nameof(matchServices));
        _matchEvents = _matchServices.MatchEvents;

        DrawTileCommand = new AsyncRelayCommand(DrawTileAsync);
        PassTurnCommand = new AsyncRelayCommand(PassTurnAsync);
        LeaveMatchCommand = new AsyncRelayCommand(LeaveMatchAsync);

        ApplyCurrentMatch();
        ApplyCurrentHand();

        _matchEvents.MatchChanged += HandleMatchChanged;
        _matchEvents.HandChanged += HandleHandChanged;
    }

    public ObservableCollection<TileSummary> Hand { get; } = new ObservableCollection<TileSummary>();

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

    public bool IsMyTurn
    {
        get
        {
            return _isMyTurn;
        }
        private set
        {
            SetProperty(ref _isMyTurn, value);
        }
    }

    public string TurnUsername
    {
        get
        {
            return _turnUsername;
        }
        private set
        {
            SetProperty(ref _turnUsername, value);
        }
    }

    public AsyncRelayCommand DrawTileCommand { get; }

    public AsyncRelayCommand PassTurnCommand { get; }

    public AsyncRelayCommand LeaveMatchCommand { get; }

    public void Unsubscribe()
    {
        _matchEvents.MatchChanged -= HandleMatchChanged;
        _matchEvents.HandChanged -= HandleHandChanged;
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

    private void HandleMatchChanged(object sender, EventArgs e)
    {
        ApplyCurrentMatch();
    }

    private void HandleHandChanged(object sender, EventArgs e)
    {
        ApplyCurrentHand();
    }

    private void ApplySnapshot(MatchSnapshot matchSnapshot)
    {
        EnginePips = matchSnapshot.EnginePips;
        BoneyardCount = matchSnapshot.BoneyardCount;
        IsMyTurn = matchSnapshot.CurrentPlayerId == _gameServices.CurrentPlayerId;
        TurnUsername = FindUsername(matchSnapshot.Players, matchSnapshot.CurrentPlayerId);

        ReplaceTrains(matchSnapshot.Trains);
        ReplacePlayers(matchSnapshot.Players, matchSnapshot.CurrentPlayerId);
    }

    private void ReplaceHand(IReadOnlyList<TileSummary> tiles)
    {
        Hand.Clear();

        foreach (TileSummary tile in tiles)
        {
            Hand.Add(tile);
        }
    }

    private void ReplaceTrains(IReadOnlyList<TrainSummary> trains)
    {
        Trains.Clear();

        foreach (TrainSummary train in trains)
        {
            bool canReceiveTile = IsMyTurn && IsTrainAvailable(train);
            ICommand dropTileCommand = CreateDropTileCommand(train.TrainId);

            Trains.Add(new TrainViewModel(train, canReceiveTile, dropTileCommand));
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

    private bool IsTrainAvailable(TrainSummary train)
    {
        bool isOwnTrain = !train.IsMexicanTrain && train.Owner.PlayerId == _gameServices.CurrentPlayerId;
        bool isAvailable = isOwnTrain || train.IsMexicanTrain || train.IsOpen;
        return isAvailable;
    }

    private ICommand CreateDropTileCommand(int trainId)
    {
        ICommand dropTileCommand = new AsyncParameterRelayCommand<int>(tileId => PlayTileAsync(tileId, trainId));
        return dropTileCommand;
    }

    private Task PlayTileAsync(int tileId, int trainId)
    {
        Task playing = RunMoveAsync(() => _matchServices.PlayTileAsync(tileId, trainId));
        return playing;
    }

    private Task DrawTileAsync()
    {
        Task drawing = RunMoveAsync(() => _matchServices.DrawTileAsync());
        return drawing;
    }

    private Task PassTurnAsync()
    {
        Task passing = RunMoveAsync(() => _matchServices.PassTurnAsync());
        return passing;
    }

    private async Task RunMoveAsync(Func<Task<MatchMoveResult>> sendMove)
    {
        MatchMoveResult matchResult = await RunWhileBusyAsync(sendMove);

        if (matchResult != MatchMoveResult.Success)
        {
            WindowService.ShowMessage(MatchMessageKeys.GetMessageKey(matchResult));
        }
    }

    private async Task LeaveMatchAsync()
    {
        await RunWhileBusyAsync(() => _gameServices.LeaveLobbyAsync());
        WindowService.Navigate(Screen.Main);
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
