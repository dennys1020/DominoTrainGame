using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

/// <summary>
/// Sends invitations to join a lobby to the friends of a player.
/// </summary>
public interface IInvitationUseCases
{
    /// <summary>
    /// Invites a connected friend to the lobby where the inviter is.
    /// </summary>
    /// <param name="inviter">The player that sends the invitation.</param>
    /// <param name="friendUsername">The username of the friend to invite.</param>
    /// <returns>The result of the invitation, or the reason it was rejected.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="inviter"/> is null.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// Thrown when the inviter is registered and <paramref name="friendUsername"/> is empty or longer than the
    /// maximum username length.
    /// </exception>
    InviteFriendStatus InviteFriend(PlayerIdentity inviter, string friendUsername);
}
