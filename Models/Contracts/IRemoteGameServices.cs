using System.Threading.Tasks;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IRemoteGameServices
{
    ILobbyEvents LobbyEvents { get; }

    LobbyResult CurrentLobby { get; }

    int CurrentPlayerId { get; }

    string SessionToken { get; }

    Task<ConnectionCheckResult> CheckConnectionAsync();

    Task<LoginResult> LogInAsync(string identifier, string password);

    void LogOut();

    Task<LobbyResult> CreateLobbyAsync();

    Task<LobbyResult> JoinLobbyAsync(string code);

    Task<LobbyActionResult> LeaveLobbyAsync();

    Task<LobbyActionResult> SetReadyAsync(bool isReady);

    Task<LobbyActionResult> SendLobbyMessageAsync(string text);

    Task<LobbyActionResult> StartMatchAsync();

    Task<LobbyActionResult> InviteFriendAsync(string username);

    Task<SignUpResult> SignUpAsync(string username, string email, string password);
}
