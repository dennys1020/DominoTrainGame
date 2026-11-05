using System;
using System.Threading.Tasks;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IRemoteGameServices
{
    event EventHandler<PlayerJoinedEventArgs> PlayerJoined;

    event EventHandler<PlayerLeftEventArgs> PlayerLeft;

    event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

    event EventHandler<PlayerReadyChangedEventArgs> PlayerReadyChanged;

    event EventHandler<LobbyMessageReceivedEventArgs> LobbyMessageReceived;

    event EventHandler<MatchStartingEventArgs> MatchStarting;

    event EventHandler<LobbyInvitationReceivedEventArgs> LobbyInvitationReceived;

    LobbyResult CurrentLobby { get; }

    int CurrentPlayerId { get; }

    string SessionToken { get; }

    Task<ConnectionCheckResult> CheckConnectionAsync();

    Task<LoginResult> LogInAsync(string identifier, string password);

    Task<LobbyResult> CreateLobbyAsync();

    Task<LobbyResult> JoinLobbyAsync(string code);

    Task<LobbyResultStatus> LeaveLobbyAsync();

    Task<LobbyResultStatus> SetReadyAsync(bool isReady);

    Task<LobbyResultStatus> SendLobbyMessageAsync(string text);

    Task<LobbyResultStatus> StartMatchAsync();

    Task<LobbyResultStatus> InviteFriendAsync(string username);

    Task<SignUpResult> SignUpAsync(string username, string email, string password);
}
