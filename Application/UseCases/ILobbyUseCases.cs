using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

/// <summary>
/// Manages the lobbies where the players meet before a match.
/// </summary>
public interface ILobbyUseCases
{
    /// <summary>
    /// Creates a lobby with a new code, where the player is the host.
    /// </summary>
    /// <param name="host">The player that creates the lobby.</param>
    /// <returns>The result of the creation and the new lobby, or the reason it was rejected.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="host"/> is null.
    /// </exception>
    LobbyOutcome<CreateLobbyStatus> CreateLobby(PlayerIdentity host);

    /// <summary>
    /// Adds a player to a lobby and tells the other members.
    /// </summary>
    /// <param name="member">The player that joins.</param>
    /// <param name="code">The code of the lobby, in any letter case and with or without surrounding spaces.</param>
    /// <returns>The result of the join and the lobby, or the reason it was rejected.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="member"/> is null.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="code"/> does not have the format of a lobby code.
    /// </exception>
    LobbyOutcome<JoinLobbyStatus> JoinLobby(PlayerIdentity member, string code);

    /// <summary>
    /// Removes a player from the lobby and tells the members that stay. Nothing happens when the player is not
    /// in a lobby.
    /// </summary>
    /// <remarks>
    /// When the host leaves, the next member becomes the host, and the lobby is closed when nobody stays.
    /// </remarks>
    /// <param name="playerId">The player that leaves.</param>
    void LeaveLobby(int playerId);

    /// <summary>
    /// Changes the ready state of a player and tells the members of the lobby.
    /// </summary>
    /// <param name="playerId">The player whose state changes.</param>
    /// <param name="isReady"><c>true</c> when the player is ready to start; otherwise, <c>false</c>.</param>
    /// <returns><c>true</c> when the state changed; <c>false</c> when the player is not in a lobby.</returns>
    bool TrySetReady(int playerId, bool isReady);

    /// <summary>
    /// Changes the domino set of the lobby and tells the members.
    /// </summary>
    /// <param name="playerId">The player that selects the set, who must be the host.</param>
    /// <param name="highestDouble">The highest double of the set: six, nine or twelve.</param>
    /// <returns>The result of the selection, or the reason it was rejected.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown when <paramref name="highestDouble"/> is not six, nine or twelve.
    /// </exception>
    SelectDominoSetStatus SelectDominoSet(int playerId, int highestDouble);

    /// <summary>
    /// Sends a chat message to the members of the lobby after the offensive words are censored.
    /// </summary>
    /// <remarks>
    /// A censored message is still sent, and it counts as an infraction that can lead to a warning or a
    /// sanction.
    /// </remarks>
    /// <param name="playerId">The player that sends the message.</param>
    /// <param name="text">The text of the message, with or without surrounding spaces.</param>
    /// <returns>
    /// <see cref="SendMessageStatus.Success"/>, <see cref="SendMessageStatus.Warned"/> when the message was sent
    /// with a warning, or the reason it was rejected.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="text"/> is empty or longer than the maximum message length.
    /// </exception>
    SendMessageStatus SendMessage(int playerId, string text);

    /// <summary>
    /// Starts the match of a lobby and tells the members.
    /// </summary>
    /// <param name="playerId">The player that starts the match, who must be the host.</param>
    /// <returns>The result of the start, or the reason it was rejected.</returns>
    StartMatchStatus StartMatch(int playerId);
}
