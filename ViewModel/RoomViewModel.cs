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

namespace DominoTrainGame.Client.ViewModel;

public sealed class RoomViewModel : ScreenViewModel
{
    private const long NextVersionOffset = 1;
    private const string SessionLostMessageKey = "LobbySessionExpiredMessage";
    private const string RoomLostMessageKey = "RoomLostAfterReconnectMessage";

    private readonly IRemoteGameServices _gameServices;
    private readonly LocalizationViewModel _localization;
    private readonly Screen _previousScreen;
    private bool _isHost;
    private bool _isReconnecting;
    private string _lobbyCode = string.Empty;
    private int _maximumPlayers;
    private long _version;

    public RoomViewModel(
        IWindowService windowService,
        RoomRole roomRole,
        IRemoteGameServices gameServices,
        LocalizationViewModel localization) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _isHost = roomRole == RoomRole.Host;
        _previousScreen = _isHost ? Screen.NewGame : Screen.JoinGame;

        BackCommand = new AsyncRelayCommand(LeaveRoomAsync);
        ResultsCommand = new RelayCommand(
            () => WindowService.Navigate(Screen.MatchResults, roomRole));

        LobbyResult currentLobby = _gameServices.CurrentLobby;

        if (currentLobby != null &&
            currentLobby.Status == LobbyResultStatus.Success)
        {
            ApplySnapshot(currentLobby);
        }

        _gameServices.PlayerJoined += OnPlayerJoined;
        _gameServices.PlayerLeft += OnPlayerLeft;
        _gameServices.ConnectionStateChanged += OnConnectionStateChanged;
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

    public ObservableCollection<RoomPlayerViewModel> Players { get; } =
        new ObservableCollection<RoomPlayerViewModel>();

    public AsyncRelayCommand BackCommand { get; }

    public RelayCommand ResultsCommand { get; }

    private void ApplySnapshot(LobbyResult lobby)
    {
        Players.Clear();

        foreach (LobbyPlayerResult player in lobby.Players)
        {
            RoomPlayerViewModel playerViewModel = new RoomPlayerViewModel(
                player,
                _gameServices.CurrentPlayerId);

            playerViewModel.IsHost = player.PlayerId == lobby.HostPlayerId;
            Players.Add(playerViewModel);
        }

        LobbyCode = lobby.Code;
        MaximumPlayers = lobby.MaximumPlayers;
        IsHost = lobby.HostPlayerId == _gameServices.CurrentPlayerId;
        _version = lobby.Version;
    }

    private void ReloadSnapshot()
    {
        LobbyResult currentLobby = _gameServices.CurrentLobby;

        if (currentLobby != null)
        {
            ApplySnapshot(currentLobby);
        }
    }

    private void OnPlayerJoined(
           object sender,
           PlayerJoinedEventArgs eventArgs)
    {
        bool isNewerVersion = eventArgs.Version > _version;
        bool isNextVersion =
            eventArgs.Version == _version + NextVersionOffset;

        if (isNewerVersion && isNextVersion)
        {
            bool isAlreadyPresent = Players.Any(
                player => player.PlayerId == eventArgs.Player.PlayerId);

            if (!isAlreadyPresent)
            {
                RoomPlayerViewModel playerViewModel = new RoomPlayerViewModel(
                    eventArgs.Player,
                    _gameServices.CurrentPlayerId);

                Players.Add(playerViewModel);
            }

            _version = eventArgs.Version;
        }
        else if (isNewerVersion)
        {
            ReloadSnapshot();
        }
    }

    private void OnPlayerLeft(
        object sender,
        PlayerLeftEventArgs eventArgs)
    {
        bool isNewerVersion = eventArgs.Version > _version;
        bool isNextVersion =
            eventArgs.Version == _version + NextVersionOffset;

        if (isNewerVersion && isNextVersion)
        {
            RoomPlayerViewModel departingPlayer = Players.FirstOrDefault(
                player => player.PlayerId == eventArgs.PlayerId);

            if (departingPlayer != null)
            {
                Players.Remove(departingPlayer);
            }

            foreach (RoomPlayerViewModel player in Players)
            {
                player.IsHost = player.PlayerId == eventArgs.HostPlayerId;
            }

            IsHost = eventArgs.HostPlayerId == _gameServices.CurrentPlayerId;
            _version = eventArgs.Version;
        }
        else if (isNewerVersion)
        {
            ReloadSnapshot();
        }
    }

    private void OnConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
    {
        switch (e.State)
        {
            case ConnectionState.Reconnecting:
                IsReconnecting = true;
                break;
            case ConnectionState.Reconnected:
                IsReconnecting = false;
                RestoreRoomAfterReconnect();
                break;
            case ConnectionState.Lost:
                IsReconnecting = false;
                ExitRoom(Screen.Login, SessionLostMessageKey);
                break;
            default:
                IsReconnecting = false;
                break;
        }
    }

    private void RestoreRoomAfterReconnect()
    {
        LobbyResult currentLobby = _gameServices.CurrentLobby;
        bool wasRoomRestored = currentLobby?.Status == LobbyResultStatus.Success;

        if (wasRoomRestored)
        {
            ApplySnapshot(currentLobby);
        }
        else
        {
            ExitRoom(_previousScreen, RoomLostMessageKey);
        }
    }

    private void ExitRoom(Screen destination, string messageKey)
    {
        UnsubscribeFromGameServices();
        WindowService.ShowMessage(_localization[messageKey]);
        WindowService.Navigate(destination);
    }

    private void UnsubscribeFromGameServices()
    {
        _gameServices.PlayerJoined -= OnPlayerJoined;
        _gameServices.PlayerLeft -= OnPlayerLeft;
        _gameServices.ConnectionStateChanged -= OnConnectionStateChanged;
    }

    private async Task LeaveRoomAsync()
    {
        LobbyResultStatus result = await _gameServices.LeaveLobbyAsync();

        if (result == LobbyResultStatus.Success)
        {
            UnsubscribeFromGameServices();
            WindowService.Navigate(_previousScreen);
        }
    }
}
