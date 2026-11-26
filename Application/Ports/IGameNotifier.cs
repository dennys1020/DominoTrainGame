namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Sends the lobby and match events to the connected players.
/// </summary>
/// <remarks>
/// A recipient that is not connected, or whose channel fails, is skipped without stopping the others.
/// </remarks>
public interface IGameNotifier
{
    /// <summary>
    /// Tells the members of a lobby that a player joined.
    /// </summary>
    /// <param name="notice">The new member, the roster version and the recipients.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="notice"/> is null.
    /// </exception>
    void NotifyPlayerJoined(PlayerJoinedNotice notice);

    /// <summary>
    /// Tells the members of a lobby that a player left.
    /// </summary>
    /// <param name="notice">The player that left, the remaining roster and the recipients.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="notice"/> is null.
    /// </exception>
    void NotifyPlayerLeft(PlayerLeftNotice notice);

    /// <summary>
    /// Tells the members of a lobby that a player changed the ready state.
    /// </summary>
    /// <param name="notice">The player, the new ready state, the roster version and the recipients.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="notice"/> is null.
    /// </exception>
    void NotifyPlayerReadyChanged(PlayerReadyChangedNotice notice);

    /// <summary>
    /// Tells the members of a lobby that the host selected another domino set.
    /// </summary>
    /// <param name="notice">The selected domino set, the roster version and the recipients.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="notice"/> is null.
    /// </exception>
    void NotifyDominoSetChanged(DominoSetChangedNotice notice);

    /// <summary>
    /// Sends a chat message to the members of a lobby.
    /// </summary>
    /// <param name="notice">The sender, the filtered text and the recipients.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="notice"/> is null.
    /// </exception>
    void NotifyLobbyMessage(LobbyMessageNotice notice);

    /// <summary>
    /// Tells the members of a lobby that the host started the match.
    /// </summary>
    /// <param name="notice">The code of the match and the recipients.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="notice"/> is null.
    /// </exception>
    void NotifyMatchStarting(MatchStartingNotice notice);

    /// <summary>
    /// Sends the public state of the board, such as the trains, the turn and the scores.
    /// </summary>
    /// <param name="notice">The state of the board and the recipients.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="notice"/> is null.
    /// </exception>
    void NotifyMatchUpdated(MatchUpdatedNotice notice);

    /// <summary>
    /// Sends the tiles of a hand only to the player that owns it.
    /// </summary>
    /// <param name="notice">The tiles of the hand and its owner.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="notice"/> is null.
    /// </exception>
    void NotifyHandUpdated(HandUpdatedNotice notice);

    /// <summary>
    /// Tells the players of a match that a round ended and how many points each player got.
    /// </summary>
    /// <param name="notice">The engine of the round, the reason it ended, the scores and the recipients.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="notice"/> is null.
    /// </exception>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown when the end reason of <paramref name="notice"/> does not describe a finished round.
    /// </exception>
    void NotifyRoundEnded(RoundEndedNotice notice);

    /// <summary>
    /// Tells the players of a match that the match ended.
    /// </summary>
    /// <param name="notice">The reason the match ended and the recipients.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="notice"/> is null.
    /// </exception>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown when the end reason of <paramref name="notice"/> does not describe a finished match.
    /// </exception>
    void NotifyMatchEnded(MatchEndedNotice notice);

    /// <summary>
    /// Sends a lobby invitation to a friend.
    /// </summary>
    /// <param name="notice">The player that invites, the lobby code and the invited friend.</param>
    /// <returns><c>true</c> when the friend is connected to the lobby channel; otherwise, <c>false</c>.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="notice"/> is null.
    /// </exception>
    bool TryNotifyLobbyInvitation(LobbyInvitationNotice notice);
}
