namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Sends the friend events to the players connected to the social channel.
/// </summary>
/// <remarks>
/// A recipient that is not connected, or whose channel fails, is skipped without stopping the others.
/// </remarks>
public interface ISocialNotifier
{
    /// <summary>
    /// Tells the friends of a player that the status of the player changed, such as going online or into a lobby.
    /// </summary>
    /// <param name="notice">The player, the new status and the friends that receive it.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="notice"/> is null.
    /// </exception>
    void NotifyFriendStatusChanged(FriendStatusNotice notice);

    /// <summary>
    /// Tells a player to reload the friend list and the pending requests, because one of them changed.
    /// </summary>
    /// <param name="playerId">The player that receives the event.</param>
    void NotifyFriendsChanged(int playerId);
}
