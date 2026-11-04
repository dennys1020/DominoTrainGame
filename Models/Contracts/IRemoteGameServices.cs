using System;
using System.Threading.Tasks;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IRemoteGameServices
{
    Task<ConnectionCheckResult> CheckConnectionAsync();

    Task<LoginResult> LogInAsync(string identifier, string password);

    Task<LobbyResult> CreateLobbyAsync();

    Task<LobbyResult> JoinLobbyAsync(string code);

    Task<LobbyResultStatus> LeaveLobbyAsync();

    Task<LobbyResultStatus> SetReadyAsync(bool isReady);

    event EventHandler<PlayerJoinedEventArgs> PlayerJoined;

    event EventHandler<PlayerLeftEventArgs> PlayerLeft;

    event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

    event EventHandler<PlayerReadyChangedEventArgs> PlayerReadyChanged;

    LobbyResult CurrentLobby { get; }

    int CurrentPlayerId { get; }

    Task<SignUpResult> SignUpAsync(string username, string email, string password);
}
