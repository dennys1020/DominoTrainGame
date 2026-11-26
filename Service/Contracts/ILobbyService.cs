using System.ServiceModel;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;

namespace DominoTrainGame.Server.Service.Contracts;

/// <summary>
/// Manages the lobbies, the lobby chat and the invitations, and starts the matches.
/// </summary>
/// <remarks>
/// The channel keeps a session. <see cref="Connect"/> must be the first call, and the server uses
/// <see cref="IGameClientCallback"/> to send the lobby and match events. When the channel closes during a
/// match, the server keeps the seat of the player for a grace period.
/// </remarks>
[ServiceContract(
    CallbackContract = typeof(IGameClientCallback),
    SessionMode = SessionMode.Required)]
public interface ILobbyService
{
    /// <summary>
    /// Binds the channel to the player that owns the session token and closes any older channel of that player.
    /// </summary>
    /// <param name="request">The session token returned by the login.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.InvalidSessionToken"/> when the token is unknown or expired, or
    /// <see cref="GameServiceFaultCode.AlreadyConnected"/> when the channel is already bound to a player.
    /// </exception>
    [OperationContract(IsInitiating = true)]
    [FaultContract(typeof(GameServiceFault))]
    void Connect(SessionTokenRequest request);

    /// <summary>
    /// Creates a lobby where the player is the host.
    /// </summary>
    /// <returns>The new lobby with its code and roster.</returns>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.LobbyConflict"/> when the player is already in a lobby,
    /// <see cref="GameServiceFaultCode.PlayerBanned"/> when the player has an active game ban,
    /// <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    LobbyResponse CreateLobby();

    /// <summary>
    /// Adds the player to the lobby with the given code.
    /// </summary>
    /// <param name="request">The code of the lobby, in any letter case.</param>
    /// <returns>The lobby with its current roster.</returns>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.InvalidLobbyRequest"/> when the code does not have the lobby code
    /// format, <see cref="GameServiceFaultCode.LobbyNotFound"/> when no lobby has that code,
    /// <see cref="GameServiceFaultCode.LobbyFull"/> when the lobby has no free seats,
    /// <see cref="GameServiceFaultCode.LobbyConflict"/> when the player is already in a lobby,
    /// <see cref="GameServiceFaultCode.PlayerBanned"/> when the player has an active game ban,
    /// <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    LobbyResponse JoinLobby(JoinLobbyRequest request);

    /// <summary>
    /// Removes the player from the lobby. Nothing happens when the player is not in a lobby.
    /// </summary>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void LeaveLobby();

    /// <summary>
    /// Changes the ready state of the player.
    /// </summary>
    /// <param name="request">The new ready state.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.LobbyNotFound"/> when the player is not in a lobby, or
    /// <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void SetReady(SetReadyRequest request);

    /// <summary>
    /// Changes the domino set of the lobby.
    /// </summary>
    /// <param name="request">The highest double of the set: six, nine or twelve.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.InvalidInput"/> when the highest double is not six, nine or twelve,
    /// <see cref="GameServiceFaultCode.LobbyNotFound"/> when the player is not in a lobby,
    /// <see cref="GameServiceFaultCode.NotLobbyHost"/> when the player is not the host,
    /// <see cref="GameServiceFaultCode.DominoSetTooSmall"/> when the set does not have enough tiles for the players in
    /// the lobby, or <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void SelectDominoSet(SelectDominoSetRequest request);

    /// <summary>
    /// Sends a chat message to the members of the lobby after the offensive words are censored.
    /// </summary>
    /// <param name="request">The text of the message.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.InvalidInput"/> when the text is empty or too long,
    /// <see cref="GameServiceFaultCode.ChatWarning"/> when the message was sent but the player is close to a sanction,
    /// <see cref="GameServiceFaultCode.ChatBlocked"/> when the player has an active chat sanction or just received one,
    /// <see cref="GameServiceFaultCode.LobbyNotFound"/> when the player is not in a lobby,
    /// <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void SendLobbyMessage(SendLobbyMessageRequest request);

    /// <summary>
    /// Starts the match of the lobby.
    /// </summary>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.LobbyNotFound"/> when the player is not in a lobby,
    /// <see cref="GameServiceFaultCode.NotLobbyHost"/> when the player is not the host,
    /// <see cref="GameServiceFaultCode.PlayersNotReady"/> when there are not enough players or a player is not ready,
    /// <see cref="GameServiceFaultCode.MatchAlreadyStarted"/> when the match of the lobby already started, or
    /// <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void StartMatch();

    /// <summary>
    /// Invites a connected friend to the lobby of the player.
    /// </summary>
    /// <param name="request">The username of the friend to invite.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.GuestNotAllowed"/> when the player is a guest,
    /// <see cref="GameServiceFaultCode.InvalidInput"/> when the username is empty or too long,
    /// <see cref="GameServiceFaultCode.LobbyNotFound"/> when the player is not in a lobby,
    /// <see cref="GameServiceFaultCode.LobbyFull"/> when the lobby has no free seats,
    /// <see cref="GameServiceFaultCode.PlayerNotFound"/> when no registered player has that username,
    /// <see cref="GameServiceFaultCode.NotFriends"/> when the players are not friends,
    /// <see cref="GameServiceFaultCode.PlayerOffline"/> when the friend is not connected,
    /// <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void InviteFriend(FriendUsernameRequest request);

    /// <summary>
    /// Returns the player to the match that was kept during the grace period and sends its current state.
    /// </summary>
    /// <param name="request">The code of the match that the player had.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.InvalidInput"/> when the code is empty,
    /// <see cref="GameServiceFaultCode.MatchNotFound"/> when the player has no kept seat in that match, or
    /// <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void Reconnect(ReconnectRequest request);

    /// <summary>
    /// Revokes the session token of the player before the client closes the channel.
    /// </summary>
    [OperationContract(IsInitiating = false, IsOneWay = true)]
    void LogOut();
}
