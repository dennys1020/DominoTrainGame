using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;
using DominoTrainGame.Client.Models.Networking;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteGameServices : IRemoteGameServices
{
    private readonly LobbyChannel _lobbyChannel;
    private readonly ClientSession _session;
    private readonly GameClientCallback _callback;

    public RemoteGameServices(LobbyChannel lobbyChannel, ClientSession session, GameClientCallback callback)
    {
        _lobbyChannel = lobbyChannel ?? throw new ArgumentNullException(nameof(lobbyChannel));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public ILobbyEvents LobbyEvents => _callback;

    public LobbyResult CurrentLobby => _session.CurrentLobby;

    public int CurrentPlayerId => _session.CurrentPlayerId;

    public string SessionToken => _session.SessionToken;

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

    public async Task<LobbyActionResult> LeaveLobbyAsync()
    {
        LobbyActionResult resultStatus = await _lobbyChannel.SendCommandAsync(SendLeaveLobbyAsync)
            .ConfigureAwait(false);

        _session.ClearLobby();
        return resultStatus;
    }

    public Task<LobbyActionResult> SetReadyAsync(bool isReady)
    {
        SetReadyRequest request = new SetReadyRequest
        {
            IsReady = isReady
        };
        LobbyCommand<SetReadyRequest> readyCommand = new LobbyCommand<SetReadyRequest>(request, SendReadyStateAsync);
        Task<LobbyActionResult> command = _lobbyChannel.SendCommandAsync(readyCommand.SendAsync);
        return command;
    }

    public Task<LobbyActionResult> SelectDominoSetAsync(int highestDouble)
    {
        SelectDominoSetRequest request = new SelectDominoSetRequest
        {
            HighestDouble = highestDouble
        };
        LobbyCommand<SelectDominoSetRequest> selectionCommand = new LobbyCommand<SelectDominoSetRequest>(
            request,
            SendDominoSetSelectionAsync);
        Task<LobbyActionResult> command = _lobbyChannel.SendCommandAsync(selectionCommand.SendAsync);
        return command;
    }

    public Task<LobbyActionResult> SendLobbyMessageAsync(string text)
    {
        SendLobbyMessageRequest request = new SendLobbyMessageRequest
        {
            Text = text
        };
        LobbyCommand<SendLobbyMessageRequest> messageCommand = new LobbyCommand<SendLobbyMessageRequest>(
            request,
            SendMessageRequestAsync);
        Task<LobbyActionResult> command = _lobbyChannel.SendCommandAsync(messageCommand.SendAsync);
        return command;
    }

    public Task<LobbyActionResult> StartMatchAsync()
    {
        Task<LobbyActionResult> command = _lobbyChannel.SendCommandAsync(SendStartMatchAsync);
        return command;
    }

    public Task<LobbyActionResult> InviteFriendAsync(string username)
    {
        FriendUsernameRequest request = new FriendUsernameRequest
        {
            Username = username
        };
        LobbyCommand<FriendUsernameRequest> invitationCommand = new LobbyCommand<FriendUsernameRequest>(
            request,
            SendInvitationAsync);
        Task<LobbyActionResult> command = _lobbyChannel.SendCommandAsync(invitationCommand.SendAsync);
        return command;
    }

    private static Task SendLeaveLobbyAsync(LobbyServiceClient lobbyClient)
    {
        Task leaving = lobbyClient.LeaveLobbyAsync();
        return leaving;
    }

    private static Task SendStartMatchAsync(LobbyServiceClient lobbyClient)
    {
        Task starting = lobbyClient.StartMatchAsync();
        return starting;
    }

    private static Task SendReadyStateAsync(LobbyServiceClient lobbyClient, SetReadyRequest request)
    {
        Task sending = lobbyClient.SetReadyAsync(request);
        return sending;
    }

    private static Task SendDominoSetSelectionAsync(LobbyServiceClient lobbyClient, SelectDominoSetRequest request)
    {
        Task sending = lobbyClient.SelectDominoSetAsync(request);
        return sending;
    }

    private static Task SendMessageRequestAsync(LobbyServiceClient lobbyClient, SendLobbyMessageRequest request)
    {
        Task sending = lobbyClient.SendLobbyMessageAsync(request);
        return sending;
    }

    private static Task SendInvitationAsync(LobbyServiceClient lobbyClient, FriendUsernameRequest request)
    {
        Task sending = lobbyClient.InviteFriendAsync(request);
        return sending;
    }

    private sealed class LobbyCommand<TRequest>
    {
        private readonly TRequest _request;
        private readonly Func<LobbyServiceClient, TRequest, Task> _send;

        public LobbyCommand(TRequest request, Func<LobbyServiceClient, TRequest, Task> send)
        {
            _request = request;
            _send = send ?? throw new ArgumentNullException(nameof(send));
        }

        public Task SendAsync(LobbyServiceClient lobbyClient)
        {
            Task sending = _send(lobbyClient, _request);
            return sending;
        }
    }
}
