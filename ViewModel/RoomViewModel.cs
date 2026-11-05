using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;
using DominoTrainGame.Client.ViewModel.Validation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class RoomViewModel : ScreenViewModel
{
    private const long NextVersionOffset = 1;
    private const int OldestMessageIndex = 0;
    private const int MaximumVisibleMessages = 100;
    private const int MinimumPlayers = 2;
    private const string SessionLostMessageKey = "LobbySessionExpiredMessage";
    private const string RoomLostMessageKey = "RoomLostAfterReconnectMessage";
    private const string ChatMessageEmptyMessageKey = "ChatMessageEmptyMessage";
    private const string ChatMessageTooLongMessageKey = "ChatMessageTooLongMessage";

    private readonly IRemoteGameServices _gameServices;
    private readonly LocalizationViewModel _localization;
    private readonly Screen _previousScreen;
    private bool _canStartMatch;
    private bool _isBusy;
    private bool _isHost;
    private bool _isReconnecting;
    private string _lobbyCode = string.Empty;
    private int _maximumPlayers;
    private string _messageText = string.Empty;
    private long _version;

    public RoomViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices,
        LocalizationViewModel localization) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));

        BackCommand = new AsyncRelayCommand(LeaveRoomAsync);
        ToggleReadyCommand = new AsyncRelayCommand(ToggleReadyAsync);
        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync);
        StartMatchCommand = new AsyncRelayCommand(StartMatchAsync);

        TryApplyCurrentLobby();
        _previousScreen = IsHost ? Screen.NewGame : Screen.JoinGame;

        _gameServices.PlayerJoined += HandlePlayerJoined;
        _gameServices.PlayerLeft += HandlePlayerLeft;
        _gameServices.ConnectionStateChanged += HandleConnectionStateChanged;
        _gameServices.PlayerReadyChanged += HandlePlayerReadyChanged;
        _gameServices.LobbyMessageReceived += HandleLobbyMessageReceived;
        _gameServices.MatchStarting += HandleMatchStarting;
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

    public bool IsBusy
    {
        get
        {
            return _isBusy;
        }
        private set
        {
            SetProperty(ref _isBusy, value);
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

    public string MessageText
    {
        get
        {
            return _messageText;
        }
        set
        {
            SetProperty(ref _messageText, value);
        }
    }

    public int MaximumMessageLength => LobbyMessageInputValidator.MaximumMessageLength;

    public ObservableCollection<RoomPlayerViewModel> Players { get; } =
        new ObservableCollection<RoomPlayerViewModel>();

    public ObservableCollection<RoomChatMessageViewModel> Messages { get; } =
        new ObservableCollection<RoomChatMessageViewModel>();

    public AsyncRelayCommand BackCommand { get; }

    public AsyncRelayCommand ToggleReadyCommand { get; }

    public AsyncRelayCommand SendMessageCommand { get; }

    public AsyncRelayCommand StartMatchCommand { get; }

    private RoomRole CurrentRole => IsHost ? RoomRole.Host : RoomRole.Guest;

    private bool TryApplyCurrentLobby()
    {
        LobbyResult currentLobby = _gameServices.CurrentLobby;
        bool hasLobby = currentLobby.Status == LobbyResultStatus.Success;

        if (hasLobby)
        {
            ApplySnapshot(currentLobby);
        }

        return hasLobby;
    }

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
        RefreshStartAvailability();
    }

    private void HandlePlayerJoined(object sender, PlayerJoinedEventArgs e)
    {
        ApplyRosterChange(e.Version, () => AddPlayer(e.Player));
    }

    private void HandlePlayerLeft(object sender, PlayerLeftEventArgs e)
    {
        ApplyRosterChange(e.Version, () => RemovePlayer(e));
    }

    private void HandlePlayerReadyChanged(object sender, PlayerReadyChangedEventArgs e)
    {
        ApplyRosterChange(e.Version, () => UpdateReadyState(e));
    }

    private void HandleLobbyMessageReceived(object sender, LobbyMessageReceivedEventArgs e)
    {
        RoomChatMessageViewModel message = new RoomChatMessageViewModel(e.Sender.Username, e.Text);

        Messages.Add(message);

        if (Messages.Count > MaximumVisibleMessages)
        {
            Messages.RemoveAt(OldestMessageIndex);
        }
    }

    private void HandleMatchStarting(object sender, MatchStartingEventArgs e)
    {
        bool isCurrentLobby = string.Equals(e.Code, LobbyCode, StringComparison.Ordinal);

        if (isCurrentLobby)
        {
            UnsubscribeFromGameServices();
            WindowService.Navigate(Screen.MatchResults, CurrentRole);
        }
    }

    private void RefreshStartAvailability()
    {
        bool hasMinimumPlayers = Players.Count >= MinimumPlayers;
        bool areAllPlayersReady = Players.All(player => player.IsReady);
        CanStartMatch = IsHost && hasMinimumPlayers && areAllPlayersReady;
    }

    private void ApplyRosterChange(long version, Action applyChange)
    {
        bool isNewerVersion = version > _version;
        bool isNextVersion = version == _version + NextVersionOffset;

        if (isNewerVersion && isNextVersion)
        {
            applyChange();
            _version = version;
            RefreshStartAvailability();
        }
        else if (isNewerVersion)
        {
            TryApplyCurrentLobby();
        }
    }

    private void AddPlayer(LobbyPlayerResult player)
    {
        bool isAlreadyPresent = Players.Any(existingPlayer => existingPlayer.PlayerId == player.PlayerId);

        if (!isAlreadyPresent)
        {
            Players.Add(new RoomPlayerViewModel(player, _gameServices.CurrentPlayerId));
        }
    }

    private void RemovePlayer(PlayerLeftEventArgs departure)
    {
        List<RoomPlayerViewModel> departingPlayers = Players
            .Where(player => player.PlayerId == departure.PlayerId)
            .ToList();

        foreach (RoomPlayerViewModel departingPlayer in departingPlayers)
        {
            Players.Remove(departingPlayer);
        }

        foreach (RoomPlayerViewModel player in Players)
        {
            player.IsHost = player.PlayerId == departure.HostPlayerId;
        }

        IsHost = departure.HostPlayerId == _gameServices.CurrentPlayerId;
    }

    private void UpdateReadyState(PlayerReadyChangedEventArgs change)
    {
        foreach (RoomPlayerViewModel player in Players)
        {
            if (player.PlayerId == change.PlayerId)
            {
                player.IsReady = change.IsReady;
            }
        }
    }

    private void HandleConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
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
        bool wasRoomRestored = TryApplyCurrentLobby();

        if (!wasRoomRestored)
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
        _gameServices.PlayerJoined -= HandlePlayerJoined;
        _gameServices.PlayerLeft -= HandlePlayerLeft;
        _gameServices.ConnectionStateChanged -= HandleConnectionStateChanged;
        _gameServices.PlayerReadyChanged -= HandlePlayerReadyChanged;
        _gameServices.LobbyMessageReceived -= HandleLobbyMessageReceived;
        _gameServices.MatchStarting -= HandleMatchStarting;
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

    private async Task ToggleReadyAsync()
    {
        bool isLocalPlayerReady = Players.Any(player => player.IsCurrentPlayer && player.IsReady);
        bool requestedReadyState = !isLocalPlayerReady;
        await RunLobbyCommandAsync(() => _gameServices.SetReadyAsync(requestedReadyState));
    }

    private async Task StartMatchAsync()
    {
        await RunLobbyCommandAsync(() => _gameServices.StartMatchAsync());
    }

    private async Task SendMessageAsync()
    {
        LobbyMessageInputStatus inputStatus = LobbyMessageInputValidator.Validate(MessageText);

        if (inputStatus == LobbyMessageInputStatus.Valid)
        {
            await SubmitMessageAsync();
        }
        else
        {
            WindowService.ShowMessage(_localization[GetInputMessageKey(inputStatus)]);
        }
    }

    private async Task SubmitMessageAsync()
    {
        string normalizedText = LobbyMessageInputValidator.Normalize(MessageText);
        LobbyResultStatus result = await RunLobbyCommandAsync(
            () => _gameServices.SendLobbyMessageAsync(normalizedText));

        if (result == LobbyResultStatus.Success)
        {
            MessageText = string.Empty;
        }
    }

    private async Task<LobbyResultStatus> RunLobbyCommandAsync(Func<Task<LobbyResultStatus>> command)
    {
        LobbyResultStatus result;
        IsBusy = true;

        try
        {
            result = await command();
        }
        finally
        {
            IsBusy = false;
        }

        if (result != LobbyResultStatus.Success)
        {
            WindowService.ShowMessage(_localization[LobbyResultMessageKeys.GetMessageKey(result)]);
        }

        return result;
    }

    private static string GetInputMessageKey(LobbyMessageInputStatus inputStatus)
    {
        string messageKey;

        switch (inputStatus)
        {
            case LobbyMessageInputStatus.MessageTooLong:
                messageKey = ChatMessageTooLongMessageKey;
                break;
            default:
                messageKey = ChatMessageEmptyMessageKey;
                break;
        }

        return messageKey;
    }
}
