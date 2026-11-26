using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Reads and changes the friendships and friend requests between registered players.
/// </summary>
public interface IFriendshipRepository
{
    /// <summary>
    /// Finds a registered player by username.
    /// </summary>
    /// <param name="username">The normalized username of the player.</param>
    /// <returns>
    /// The player, or <see cref="PlayerIdentity.Empty"/> when no registered player has that username.
    /// </returns>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    PlayerIdentity FindRegisteredPlayer(string username);

    /// <summary>
    /// Gets the players that accepted a friendship with a player, ordered by username.
    /// </summary>
    /// <param name="playerId">The player whose friends are read.</param>
    /// <returns>The friends of the player, or an empty list when the player has no friends.</returns>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    IReadOnlyList<PlayerIdentity> GetFriends(int playerId);

    /// <summary>
    /// Gets the friend requests that a player received and has not answered, oldest first.
    /// </summary>
    /// <param name="playerId">The player that received the requests.</param>
    /// <returns>The pending requests, or an empty list when there are none.</returns>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    IReadOnlyList<PendingFriendshipSummary> GetPendingRequests(int playerId);

    /// <summary>
    /// Searches the registered players whose username contains a fragment.
    /// </summary>
    /// <param name="playerId">The player that searches, who is left out of the results.</param>
    /// <param name="usernameFragment">The normalized fragment of the username.</param>
    /// <returns>A limited list of players ordered by username, or an empty list when none matches.</returns>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    IReadOnlyList<PlayerIdentity> SearchRegisteredPlayers(int playerId, string usernameFragment);

    /// <summary>
    /// Gets the state of the friendship between two players, no matter who sent the request.
    /// </summary>
    /// <param name="playerId">One of the players.</param>
    /// <param name="friendPlayerId">The other player.</param>
    /// <returns>The state of the friendship, or <see cref="FriendshipState.None"/> when there is no request.</returns>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    FriendshipState GetFriendshipState(int playerId, int friendPlayerId);

    /// <summary>
    /// Creates a pending friend request, or reopens a request between the two players that was rejected.
    /// </summary>
    /// <param name="requesterPlayerId">The player that sends the request.</param>
    /// <param name="addresseePlayerId">The player that receives the request.</param>
    /// <returns>
    /// <c>true</c> when the request was created or reopened; <c>false</c> when the players already have a
    /// request that was not rejected.
    /// </returns>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    /// <exception cref="Errors.DataIntegrityException">
    /// Thrown when the changes violate a database constraint.
    /// </exception>
    bool CreateRequest(int requesterPlayerId, int addresseePlayerId);

    /// <summary>
    /// Accepts a pending friend request.
    /// </summary>
    /// <param name="requestId">The request to accept.</param>
    /// <param name="addresseePlayerId">The player that received the request.</param>
    /// <returns>
    /// The player that sent the request, or an empty list when the request is not pending or was not sent to
    /// <paramref name="addresseePlayerId"/>.
    /// </returns>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    /// <exception cref="Errors.DataIntegrityException">
    /// Thrown when the changes violate a database constraint.
    /// </exception>
    IReadOnlyList<int> AcceptRequest(int requestId, int addresseePlayerId);

    /// <summary>
    /// Rejects a pending friend request.
    /// </summary>
    /// <param name="requestId">The request to reject.</param>
    /// <param name="addresseePlayerId">The player that received the request.</param>
    /// <returns>
    /// <c>true</c> when the request was rejected; <c>false</c> when it is not pending or was not sent to
    /// <paramref name="addresseePlayerId"/>.
    /// </returns>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    /// <exception cref="Errors.DataIntegrityException">
    /// Thrown when the changes violate a database constraint.
    /// </exception>
    bool RejectRequest(int requestId, int addresseePlayerId);

    /// <summary>
    /// Deletes the accepted friendship between two players.
    /// </summary>
    /// <param name="playerId">The player that removes the friend.</param>
    /// <param name="friendPlayerId">The friend to remove.</param>
    /// <returns><c>true</c> when the friendship was deleted; <c>false</c> when the players are not friends.</returns>
    /// <exception cref="Errors.DatabaseUnavailableException">
    /// Thrown when the database cannot be reached.
    /// </exception>
    /// <exception cref="Errors.DataIntegrityException">
    /// Thrown when the changes violate a database constraint.
    /// </exception>
    bool RemoveFriendship(int playerId, int friendPlayerId);
}
