using System.Collections.Generic;
using System.ServiceModel;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;

namespace DominoTrainGame.Server.Service.Contracts;

/// <summary>
/// Manages the friends and friend requests of the registered players.
/// </summary>
/// <remarks>
/// The channel keeps a session. <see cref="Connect"/> must be the first call, and the server uses
/// <see cref="ISocialClientCallback"/> to tell the player about status changes and new requests.
/// </remarks>
[ServiceContract(
    CallbackContract = typeof(ISocialClientCallback),
    SessionMode = SessionMode.Required)]
public interface IFriendService
{
    /// <summary>
    /// Binds the channel to the registered player that owns the session token.
    /// </summary>
    /// <param name="request">The session token returned by the login.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.InvalidSessionToken"/> when the token is unknown or expired,
    /// <see cref="GameServiceFaultCode.GuestNotAllowed"/> when the player is a guest, or
    /// <see cref="GameServiceFaultCode.AlreadyConnected"/> when the channel is already bound to a player.
    /// </exception>
    [OperationContract(IsInitiating = true)]
    [FaultContract(typeof(GameServiceFault))]
    void Connect(SessionTokenRequest request);

    /// <summary>
    /// Gets the friends of the player with the current status of each one.
    /// </summary>
    /// <returns>The friends ordered by username, or an empty list when the player has no friends.</returns>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player,
    /// <see cref="GameServiceFaultCode.GuestNotAllowed"/> when the player is a guest, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    List<FriendResponse> GetFriends();

    /// <summary>
    /// Gets the friend requests that the player received and has not answered.
    /// </summary>
    /// <returns>The pending requests, oldest first, or an empty list when there are none.</returns>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player,
    /// <see cref="GameServiceFaultCode.GuestNotAllowed"/> when the player is a guest, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    List<PendingFriendshipResponse> GetPendingRequests();

    /// <summary>
    /// Searches the registered players whose username contains a fragment.
    /// </summary>
    /// <param name="request">The fragment of the username.</param>
    /// <returns>A limited list of players ordered by username, or an empty list when none matches.</returns>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.InvalidInput"/> when the fragment is empty or too long,
    /// <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player,
    /// <see cref="GameServiceFaultCode.GuestNotAllowed"/> when the player is a guest, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    List<PlayerSearchResponse> SearchPlayers(PlayerSearchRequest request);

    /// <summary>
    /// Sends a friend request to a registered player.
    /// </summary>
    /// <param name="request">The username of the player that receives the request.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.InvalidInput"/> when the username is empty or too long,
    /// <see cref="GameServiceFaultCode.PlayerNotFound"/> when no registered player has that username,
    /// <see cref="GameServiceFaultCode.CannotTargetSelf"/> when the username belongs to the sender,
    /// <see cref="GameServiceFaultCode.AlreadyFriends"/> when the players are already friends,
    /// <see cref="GameServiceFaultCode.FriendRequestAlreadyExists"/> when a request between them is pending,
    /// <see cref="GameServiceFaultCode.PlayerBlocked"/> when one player blocked the other,
    /// <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player,
    /// <see cref="GameServiceFaultCode.GuestNotAllowed"/> when the player is a guest, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void SendFriendRequest(FriendUsernameRequest request);

    /// <summary>
    /// Accepts a friend request that the player received.
    /// </summary>
    /// <param name="request">The identifier of the request.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.FriendRequestNotFound"/> when the request is not pending or was not
    /// sent to the player, <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player,
    /// <see cref="GameServiceFaultCode.GuestNotAllowed"/> when the player is a guest, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void AcceptFriendRequest(FriendshipDecisionRequest request);

    /// <summary>
    /// Rejects a friend request that the player received.
    /// </summary>
    /// <param name="request">The identifier of the request.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.FriendRequestNotFound"/> when the request is not pending or was not
    /// sent to the player, <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player,
    /// <see cref="GameServiceFaultCode.GuestNotAllowed"/> when the player is a guest, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void RejectFriendRequest(FriendshipDecisionRequest request);

    /// <summary>
    /// Removes a friend of the player.
    /// </summary>
    /// <param name="request">The username of the friend to remove.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.InvalidInput"/> when the username is empty or too long,
    /// <see cref="GameServiceFaultCode.PlayerNotFound"/> when no registered player has that username,
    /// <see cref="GameServiceFaultCode.NotFriends"/> when the players are not friends,
    /// <see cref="GameServiceFaultCode.SessionNotFound"/> when the channel has no connected player,
    /// <see cref="GameServiceFaultCode.GuestNotAllowed"/> when the player is a guest, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract(IsInitiating = false)]
    [FaultContract(typeof(GameServiceFault))]
    void RemoveFriend(FriendUsernameRequest request);
}
