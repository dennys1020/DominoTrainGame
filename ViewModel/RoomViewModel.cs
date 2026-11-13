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
using DominoTrainGame.Client.ViewModel.Validation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class RoomViewModel : ScreenViewModel, ISubscribingViewModel
{
    private const int OldestMessageIndex = 0;
    private const int MaximumVisibleMessages = 100;
    private const int MinimumPlayers = 2;
    private const string RoomLostMessageKey = "RoomLostAfterReconnectMessage";
    private const string ChatMessageEmptyMessageKey = "ChatMessageEmptyMessage";
    private const string ChatMessageTooLongMessageKey = "ChatMessageTooLongMessage";

    private readonly IRemoteGameServices _gameServices;
    private readonly ILobbyEvents _lobbyEvents;
    private readonly LocalizationViewModel _localization;
    private readonly Screen _previousScreen;
    private bool _canStartMatch;
    private bool _isHost;
    private bool _isReconnecting;
    private string _lobbyCode = string.Empty;
    private int _maximumPlayers;
    private string _messageText = string.Empty;
    private string _chatNotice = string.Empty;

    public RoomViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices,
        InviteFriendsViewModel inviteFriends,
        LocalizationViewModel localization) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _lobbyEvents = _gameServices.LobbyEvents;
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        InviteFriends = inviteFriends ?? throw new ArgumentNullException(nameof(inviteFriends));

        BackCommand = new AsyncRelayCommand(LeaveRoomAsync);
        ToggleReadyCommand = new AsyncRelayCommand(ToggleReadyAsync);
        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync);
        StartMatchCommand = new AsyncRelayCommand(StartMatchAsync);

        TryApplyCurrentLobby();
        _previousScreen = IsHost ? Screen.NewGame : Screen.JoinGame;

        _lobbyEvents.LobbyChanged += HandleLobbyChanged;
        _lobbyEvents.ConnectionStateChanged += HandleConnectionStateChanged;
        _lobbyEvents.LobbyMessageReceived += HandleLobbyMessageReceived;
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

    public string ChatNotice
    {
        get
        {
            return _chatNotice;
        }
        private set
        {
            bool hasChanged = SetProperty(ref _chatNotice, value);

            if (hasChanged)
            {
                OnPropertyChanged(nameof(HasChatNotice));
            }
        }
    }

    public bool HasChatNotice => !string.IsNullOrEmpty(ChatNotice);

    public int MaximumMessageLength => LobbyMessageInputValidator.MaximumMessageLength;

    public InviteFriendsViewModel InviteFriends { get; }

    public ObservableCollection<RoomPlayerViewModel> Players { get; } =
        new ObservableCollection<RoomPlayerViewModel>();

    public ObservableCollection<RoomChatMessageViewModel> Messages { get; } =
        new ObservableCollection<RoomChatMessageViewModel>();

    public AsyncRelayCommand BackCommand { get; }

    public AsyncRelayCommand ToggleReadyCommand { get; }

    public AsyncRelayCommand SendMessageCommand { get; }

    public AsyncRelayCommand StartMatchCommand { get; }

    public void Unsubscribe()
    {
        _lobbyEvents.LobbyChanged -= HandleLobbyChanged;
        _lobbyEvents.ConnectionStateChanged -= HandleConnectionStateChanged;
        _lobbyEvents.LobbyMessageReceived -= HandleLobbyMessageReceived;
        _lobbyEvents.MatchStarting -= HandleMatchStarting;
    }

    private bool TryApplyCurrentLobby()
    {
        LobbyResult currentLobby = _gameServices.CurrentLobby;
        bool hasLobby = currentLobby.Status == LobbyActionResult.Success;

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
                _gameServices.CurrentPlayerId,
                lobby.HostPlayerId);

            Players.Add(playerViewModel);
        }

        LobbyCode = lobby.Code;
        MaximumPlayers = lobby.MaximumPlayers;
        IsHost = lobby.HostPlayerId == _gameServices.CurrentPlayerId;
        RefreshStartAvailability();
    }

    private void HandleLobbyChanged(object sender, EventArgs eventArgs)
    {
        TryApplyCurrentLobby();
    }

    private void HandleLobbyMessageReceived(object sender, LobbyMessageReceivedEventArgs eventArgs)
    {
        RoomChatMessageViewModel message = new RoomChatMessageViewModel(eventArgs.Sender.Username, eventArgs.Text);

        Messages.Add(message);

        if (Messages.Count > MaximumVisibleMessages)
        {
            Messages.RemoveAt(OldestMessageIndex);
        }
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
        bool hasMinimumPlayers = Players.Count >= MinimumPlayers;
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
            case ConnectionState.Lost:
                IsReconnecting = false;
                ExitRoom(Screen.Login, CommonMessageKeys.SessionExpiredMessageKey);
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
        WindowService.ShowMessage(messageKey);
        WindowService.Navigate(destination);
    }

    private async Task LeaveRoomAsync()
    {
        LobbyActionResult result = await _gameServices.LeaveLobbyAsync();

        if (result == LobbyActionResult.Success)
        {
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
            WindowService.ShowMessage(GetInputMessageKey(inputStatus));
        }
    }

    private async Task SubmitMessageAsync()
    {
        string normalizedText = LobbyMessageInputValidator.Normalize(MessageText);
        LobbyActionResult result = await RunLobbyCommandAsync(
            () => _gameServices.SendLobbyMessageAsync(normalizedText));

        bool wasDelivered = result == LobbyActionResult.Success || result == LobbyActionResult.ChatWarning;

        if (wasDelivered)
        {
            MessageText = string.Empty;
        }

        ChatNotice = GetChatNotice(result);
    }

    private string GetChatNotice(LobbyActionResult result)
    {
        string chatNotice = IsChatSanction(result)
            ? _localization[LobbyResultMessageKeys.GetMessageKey(result)]
            : string.Empty;
        return chatNotice;
    }

    private async Task<LobbyActionResult> RunLobbyCommandAsync(Func<Task<LobbyActionResult>> command)
    {
        LobbyActionResult result = await RunWhileBusyAsync(command);
        bool isFailure = result != LobbyActionResult.Success && !IsChatSanction(result);

        if (isFailure)
        {
            WindowService.ShowMessage(LobbyResultMessageKeys.GetMessageKey(result));
        }

        return result;
    }

    private static bool IsChatSanction(LobbyActionResult result)
    {
        bool isChatSanction = result == LobbyActionResult.ChatWarning || result == LobbyActionResult.ChatBlocked;
        return isChatSanction;
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
