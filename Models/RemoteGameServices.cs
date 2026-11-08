using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;
using DominoTrainGame.Client.Models.Networking;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteGameServices : IRemoteGameServices
{
    private readonly AccountRequests _accountRequests;
    private readonly LobbyChannel _lobbyChannel;
    private readonly ClientSession _session;
    private readonly GameClientCallback _callback;

    public RemoteGameServices(
        AccountRequests accountRequests,
        LobbyChannel lobbyChannel,
        ClientSession session,
        GameClientCallback callback)
    {
        _accountRequests = accountRequests ?? throw new ArgumentNullException(nameof(accountRequests));
        _lobbyChannel = lobbyChannel ?? throw new ArgumentNullException(nameof(lobbyChannel));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public ILobbyEvents LobbyEvents => _callback;

    public LobbyResult CurrentLobby => _session.CurrentLobby;

    public int CurrentPlayerId => _session.CurrentPlayerId;

    public string SessionToken => _session.SessionToken;

    public Task<ConnectionCheckResult> CheckConnectionAsync()
    {
        Task<ConnectionCheckResult> connectionCheck = _accountRequests.CheckConnectionAsync();
        return connectionCheck;
    }

    public Task<LoginResult> LogInAsync(string identifier, string password)
    {
        Task<LoginResult> login = _accountRequests.LogInAsync(identifier, password);
        return login;
    }

    public void LogOut()
    {
        _lobbyChannel.Close();
        _session.ClearLobby();
        _session.Forget();
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
}
