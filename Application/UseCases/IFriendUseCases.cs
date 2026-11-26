using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

/// <summary>
/// Manages the friends and friend requests of the registered players.
/// </summary>
public interface IFriendUseCases
{
    /// <summary>
    /// Gets the friends of a player with the current status of each one.
    /// </summary>
    /// <param name="playerId">The player whose friends are read.</param>
    /// <returns>The friends ordered by username, or an empty list when the player has no friends.</returns>
    IReadOnlyList<FriendPresence> GetFriends(int playerId);

    /// <summary>
    /// Gets the friend requests that a player received and has not answered.
    /// </summary>
    /// <param name="playerId">The player that received the requests.</param>
    /// <returns>The pending requests, oldest first, or an empty list when there are none.</returns>
    IReadOnlyList<PendingFriendshipSummary> GetPendingRequests(int playerId);

    /// <summary>
    /// Searches the registered players whose username contains a fragment.
    /// </summary>
    /// <param name="playerId">The player that searches, who is left out of the results.</param>
    /// <param name="usernameFragment">The fragment to search, with or without surrounding spaces.</param>
    /// <returns>A limited list of players ordered by username, or an empty list when none matches.</returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when the username is empty or longer than the maximum username length.
    /// </exception>
    IReadOnlyList<PlayerIdentity> SearchPlayers(int playerId, string usernameFragment);

    /// <summary>
    /// Sends a friend request and tells the player that receives it.
    /// </summary>
    /// <param name="requesterPlayerId">The player that sends the request.</param>
    /// <param name="addresseeUsername">The username of the player that receives the request.</param>
    /// <returns>The result of the request, or the reason it was rejected.</returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when the username is empty or longer than the maximum username length.
    /// </exception>
    SendFriendRequestStatus SendFriendRequest(int requesterPlayerId, string addresseeUsername);

    /// <summary>
    /// Accepts a friend request and tells the player that sent it.
    /// </summary>
    /// <param name="playerId">The player that received the request.</param>
    /// <param name="requestId">The request to accept.</param>
    /// <returns>
    /// <c>true</c> when the request was accepted; <c>false</c> when it is not pending or was not sent to the
    /// player.
    /// </returns>
    bool AcceptFriendRequest(int playerId, int requestId);

    /// <summary>
    /// Rejects a friend request.
    /// </summary>
    /// <param name="playerId">The player that received the request.</param>
    /// <param name="requestId">The request to reject.</param>
    /// <returns>
    /// <c>true</c> when the request was rejected; <c>false</c> when it is not pending or was not sent to the
    /// player.
    /// </returns>
    bool RejectFriendRequest(int playerId, int requestId);

    /// <summary>
    /// Removes a friend and tells the removed player.
    /// </summary>
    /// <param name="playerId">The player that removes the friend.</param>
    /// <param name="friendUsername">The username of the friend to remove.</param>
    /// <returns>The result of the removal, or the reason it was rejected.</returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when the username is empty or longer than the maximum username length.
    /// </exception>
    RemoveFriendStatus RemoveFriend(int playerId, string friendUsername);

    /// <summary>
    /// Sends the current status of a player to the friends of that player.
    /// </summary>
    /// <remarks>
    /// When the database is unavailable the error is logged and the status is not sent, so the operation
    /// that changed the status is not undone.
    /// </remarks>
    /// <param name="playerId">The player whose status changed.</param>
    void PublishStatus(int playerId);
}
