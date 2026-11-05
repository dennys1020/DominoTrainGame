using System;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;
using DominoTrainGame.Client.Models.Networking;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteGameServices : IRemoteGameServices
{
    private readonly AccountRequests _accountRequests;
    private readonly LobbyChannel _lobbyChannel;
    private readonly ClientSession _session;
    private readonly LobbyEventRelay _lobbyEvents;

    public RemoteGameServices(
        AccountRequests accountRequests,
        LobbyChannel lobbyChannel,
        LobbySessionContext sessionContext)
    {
        if (sessionContext == null)
        {
            throw new ArgumentNullException(nameof(sessionContext));
        }

        _accountRequests = accountRequests ?? throw new ArgumentNullException(nameof(accountRequests));
        _lobbyChannel = lobbyChannel ?? throw new ArgumentNullException(nameof(lobbyChannel));
        _session = sessionContext.Session;
        _lobbyEvents = sessionContext.Events;
    }

    public event EventHandler<PlayerJoinedEventArgs> PlayerJoined
    {
        add
        {
            _lobbyEvents.PlayerJoined += value;
        }
        remove
        {
            _lobbyEvents.PlayerJoined -= value;
        }
    }

    public event EventHandler<PlayerLeftEventArgs> PlayerLeft
    {
        add
        {
            _lobbyEvents.PlayerLeft += value;
        }
        remove
        {
            _lobbyEvents.PlayerLeft -= value;
        }
    }

    public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged
    {
        add
        {
            _lobbyEvents.ConnectionStateChanged += value;
        }
        remove
        {
            _lobbyEvents.ConnectionStateChanged -= value;
        }
    }

    public event EventHandler<PlayerReadyChangedEventArgs> PlayerReadyChanged
    {
        add
        {
            _lobbyEvents.PlayerReadyChanged += value;
        }
        remove
        {
            _lobbyEvents.PlayerReadyChanged -= value;
        }
    }

    public event EventHandler<LobbyMessageReceivedEventArgs> LobbyMessageReceived
    {
        add
        {
            _lobbyEvents.LobbyMessageReceived += value;
        }
        remove
        {
            _lobbyEvents.LobbyMessageReceived -= value;
        }
    }

    public event EventHandler<MatchStartingEventArgs> MatchStarting
    {
        add
        {
            _lobbyEvents.MatchStarting += value;
        }
        remove
        {
            _lobbyEvents.MatchStarting -= value;
        }
    }

    public event EventHandler<LobbyInvitationReceivedEventArgs> LobbyInvitationReceived
    {
        add
        {
            _lobbyEvents.LobbyInvitationReceived += value;
        }
        remove
        {
            _lobbyEvents.LobbyInvitationReceived -= value;
        }
    }

    public LobbyResult CurrentLobby => _session.CurrentLobby;

    public int CurrentPlayerId => _session.CurrentPlayerId;

    public string SessionToken => _session.SessionToken;

    public Task<ConnectionCheckResult> CheckConnectionAsync()
    {
        Task<ConnectionCheckResult> connectionCheck = _accountRequests.CheckConnectionAsync();
        return connectionCheck;
    }

    public async Task<LoginResult> LogInAsync(string identifier, string password)
    {
        SynchronizationContext callerContext = SynchronizationContext.Current;
        LoginResult loginResult = await _accountRequests.RunLogInAsync(
            () => AuthenticateAsync(identifier, password, callerContext));
        bool isLoggedIn = loginResult == LoginResult.Success;

        if (!isLoggedIn)
        {
            _session.Forget();
        }

        return loginResult;
    }

    public Task<LobbyResult> CreateLobbyAsync()
    {
        Task<LobbyResult> creation = _lobbyChannel.CreateLobbyAsync();
        return creation;
    }

    public Task<LobbyResult> JoinLobbyAsync(string code)
    {
        Task<LobbyResult> joining = _lobbyChannel.JoinLobbyAsync(code);
        return joining;
    }

    public async Task<LobbyResultStatus> LeaveLobbyAsync()
    {
        LobbyResultStatus resultStatus = await _lobbyChannel.SendCommandAsync(
            lobbyClient => lobbyClient.LeaveLobbyAsync()).ConfigureAwait(false);

        _session.ClearLobby();
        return resultStatus;
    }

    public Task<LobbyResultStatus> SetReadyAsync(bool isReady)
    {
        SetReadyRequest request = new SetReadyRequest
        {
            IsReady = isReady
        };
        Task<LobbyResultStatus> command = _lobbyChannel.SendCommandAsync(
            lobbyClient => lobbyClient.SetReadyAsync(request));
        return command;
    }

    public Task<LobbyResultStatus> SendLobbyMessageAsync(string text)
    {
        SendLobbyMessageRequest request = new SendLobbyMessageRequest
        {
            Text = text
        };
        Task<LobbyResultStatus> command = _lobbyChannel.SendCommandAsync(
            lobbyClient => lobbyClient.SendLobbyMessageAsync(request));
        return command;
    }

    public Task<LobbyResultStatus> StartMatchAsync()
    {
        Task<LobbyResultStatus> command = _lobbyChannel.SendCommandAsync(
            lobbyClient => lobbyClient.StartMatchAsync());
        return command;
    }

    public Task<LobbyResultStatus> InviteFriendAsync(string username)
    {
        FriendUsernameRequest request = new FriendUsernameRequest
        {
            Username = username
        };
        Task<LobbyResultStatus> command = _lobbyChannel.SendCommandAsync(
            lobbyClient => lobbyClient.InviteFriendAsync(request));
        return command;
    }

    public Task<SignUpResult> SignUpAsync(string username, string email, string password)
    {
        Task<SignUpResult> signUp = _accountRequests.SignUpAsync(username, email, password);
        return signUp;
    }

    private async Task<LoginResult> AuthenticateAsync(
        string identifier,
        string password,
        SynchronizationContext callerContext)
    {
        LoginResult loginResult = LoginResult.InvalidCredentials;
        LoginResponse response = await _accountRequests.SendLogInAsync(identifier, password);
        bool isAuthenticated = response?.IsAuthenticated ?? false;

        if (isAuthenticated)
        {
            _session.Store(response.PlayerId, response.SessionToken, callerContext);
            await _lobbyChannel.OpenAsync(response.SessionToken);
            loginResult = LoginResult.Success;
        }

        return loginResult;
    }
}
