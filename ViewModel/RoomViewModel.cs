using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;
using DominoTrainGame.Shared.Rules;

namespace DominoTrainGame.Client.ViewModel;

public sealed class RoomViewModel : ScreenViewModel, ISubscribingViewModel
{
    private const int MinimumPlayersToStart = 2;
    private const string RoomLostAfterReconnectMessageKey = "RoomLostAfterReconnectMessage";

    private readonly IRemoteGameServices _gameServices;
    private readonly ILobbyEvents _lobbyEvents;
    private readonly Screen _previousScreen;
    private bool _canStartMatch;
    private bool _isHost;
    private bool _isReconnecting;
    private string _lobbyCode = string.Empty;
    private int _maximumPlayers;
    private int _highestDouble = MatchRules.DoubleTwelvePips;

    public RoomViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices,
        InviteFriendsViewModel inviteFriends,
        LobbyChatViewModel chat) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _lobbyEvents = _gameServices.LobbyEvents;
        InviteFriends = inviteFriends ?? throw new ArgumentNullException(nameof(inviteFriends));
        Chat = chat ?? throw new ArgumentNullException(nameof(chat));

        BackCommand = new AsyncRelayCommand(LeaveRoomAsync);
        ToggleReadyCommand = new AsyncRelayCommand(ToggleReadyAsync);
        StartMatchCommand = new AsyncRelayCommand(StartMatchAsync);
        SelectDominoSetCommand = new AsyncParameterRelayCommand<int>(SelectDominoSetAsync);

        TryApplyCurrentLobby();
        _previousScreen = IsHost ? Screen.NewGame : Screen.JoinGame;

        _lobbyEvents.LobbyChanged += HandleLobbyChanged;
        _lobbyEvents.ConnectionStateChanged += HandleConnectionStateChanged;
        _lobbyEvents.MatchStarting += HandleMatchStarting;
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

    public bool IsHost
    {
        get
        {
            return _isHost;
        }
        private set
        {
            SetProperty(ref _isHost, value);
        }
    }

    public bool CanStartMatch
    {
        get
        {
            return _canStartMatch;
        }
        private set
        {
            SetProperty(ref _canStartMatch, value);
        }
    }

    public string LobbyCode
    {
        get
        {
            return _lobbyCode;
        }
        private set
        {
            SetProperty(ref _lobbyCode, value);
        }
    }

    public int MaximumPlayers
    {
        get
        {
            return _maximumPlayers;
        }
        private set
        {
            SetProperty(ref _maximumPlayers, value);
        }
    }

    public int HighestDouble
    {
        get
        {
            return _highestDouble;
        }
        private set
        {
            bool hasChanged = SetProperty(ref _highestDouble, value);

            if (hasChanged)
            {
                RaiseDominoSetSelectionChanged();
            }
        }
    }

    public bool IsDoubleSixSelected => HighestDouble == MatchRules.DoubleSixPips;

    public bool IsDoubleNineSelected => HighestDouble == MatchRules.DoubleNinePips;

    public bool IsDoubleTwelveSelected => HighestDouble == MatchRules.DoubleTwelvePips;

    public InviteFriendsViewModel InviteFriends { get; }

    public LobbyChatViewModel Chat { get; }

    public ObservableCollection<RoomPlayerViewModel> Players { get; } =
        new ObservableCollection<RoomPlayerViewModel>();

    public AsyncRelayCommand BackCommand { get; }

    public AsyncRelayCommand ToggleReadyCommand { get; }

    public AsyncRelayCommand StartMatchCommand { get; }

    public AsyncParameterRelayCommand<int> SelectDominoSetCommand { get; }

    public void Unsubscribe()
    {
        _lobbyEvents.LobbyChanged -= HandleLobbyChanged;
        _lobbyEvents.ConnectionStateChanged -= HandleConnectionStateChanged;
        _lobbyEvents.MatchStarting -= HandleMatchStarting;
        Chat.Unsubscribe();
    }

    private bool TryApplyCurrentLobby()
    {
        LobbyResult currentLobby = _gameServices.CurrentLobby;
        bool hasLobby = currentLobby.Status == LobbyActionResult.Success;

        if (hasLobby)
        {
            ApplyLobby(currentLobby);
        }

        return hasLobby;
    }

    private void ApplyLobby(LobbyResult lobby)
    {
        Players.Clear();

        foreach (LobbyPlayerResult player in lobby.Players)
        {
            RoomPlayerViewModel playerViewModel = new RoomPlayerViewModel(
                player,
                _gameServices.CurrentPlayerId,
                lobby.HostPlayerId);

            Players.Add(playerViewModel);
        }

        LobbyCode = lobby.Code;
        HighestDouble = lobby.HighestDouble;
        MaximumPlayers = lobby.MaximumPlayers;
        IsHost = lobby.HostPlayerId == _gameServices.CurrentPlayerId;
        RefreshStartAvailability();
    }

    private void HandleLobbyChanged(object sender, EventArgs eventArgs)
    {
        TryApplyCurrentLobby();
    }

    private void HandleMatchStarting(object sender, MatchStartingEventArgs eventArgs)
    {
        bool isCurrentLobby = string.Equals(eventArgs.Code, LobbyCode, StringComparison.Ordinal);

        if (isCurrentLobby)
        {
            WindowService.Navigate(Screen.Match);
        }
    }

    private void RefreshStartAvailability()
    {
        bool hasMinimumPlayers = Players.Count >= MinimumPlayersToStart;
        bool areAllPlayersReady = Players.All(player => player.IsReady);
        CanStartMatch = IsHost && hasMinimumPlayers && areAllPlayersReady;
    }

    private void HandleConnectionStateChanged(object sender, ConnectionStateChangedEventArgs eventArgs)
    {
        switch (eventArgs.State)
        {
            case ConnectionState.Reconnecting:
                IsReconnecting = true;
                break;
            case ConnectionState.Reconnected:
                IsReconnecting = false;
                RestoreRoomAfterReconnect();
                break;
            default:
                IsReconnecting = false;
                break;
        }
    }

    private void RestoreRoomAfterReconnect()
    {
        bool wasRoomRestored = TryApplyCurrentLobby();

        if (!wasRoomRestored)
        {
            ExitRoom(_previousScreen, RoomLostAfterReconnectMessageKey);
        }
    }

    private void ExitRoom(Screen destination, string messageKey)
    {
        WindowService.ShowMessage(messageKey);
        WindowService.Navigate(destination);
    }

    private async Task LeaveRoomAsync()
    {
        LobbyActionResult leaveResult = await _gameServices.LeaveLobbyAsync();

        if (leaveResult == LobbyActionResult.Success)
        {
            WindowService.Navigate(_previousScreen);
        }
    }

    private async Task ToggleReadyAsync()
    {
        bool isLocalPlayerReady = Players.Any(player => player.IsCurrentPlayer && player.IsReady);
        bool requestedReadyState = !isLocalPlayerReady;
        await RunLobbyRequestAsync(() => _gameServices.SetReadyAsync(requestedReadyState));
    }

    private async Task StartMatchAsync()
    {
        await RunLobbyRequestAsync(() => _gameServices.StartMatchAsync());
    }

    private async Task SelectDominoSetAsync(int highestDouble)
    {
        bool isNewSelection = highestDouble != HighestDouble;

        if (isNewSelection)
        {
            LobbyActionResult selectionResult = await RunWhileBusyAsync(
                _gameServices.SelectDominoSetAsync,
                highestDouble);
            ShowRequestFailure(selectionResult);
        }

        RaiseDominoSetSelectionChanged();
    }

    private void RaiseDominoSetSelectionChanged()
    {
        OnPropertyChanged(nameof(IsDoubleSixSelected));
        OnPropertyChanged(nameof(IsDoubleNineSelected));
        OnPropertyChanged(nameof(IsDoubleTwelveSelected));
    }

    private async Task RunLobbyRequestAsync(Func<Task<LobbyActionResult>> sendRequest)
    {
        LobbyActionResult requestResult = await RunWhileBusyAsync(sendRequest);
        ShowRequestFailure(requestResult);
    }

    private void ShowRequestFailure(LobbyActionResult requestResult)
    {
        if (requestResult != LobbyActionResult.Success)
        {
            WindowService.ShowMessageError(LobbyResultMessageKeys.GetMessageKey(requestResult));
        }
    }

}
