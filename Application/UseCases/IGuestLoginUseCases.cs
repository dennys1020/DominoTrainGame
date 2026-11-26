using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.UseCases;

/// <summary>
/// Lets a person play without an account.
/// </summary>
public interface IGuestLoginUseCases
{
    /// <summary>
    /// Creates a guest player with a free generated username.
    /// </summary>
    /// <remarks>
    /// A guest can play matches, but cannot add friends, send invitations or have a profile.
    /// </remarks>
    /// <returns>
    /// The new guest, or <see cref="PlayerIdentity.Empty"/> when no free username was found after a few
    /// attempts.
    /// </returns>
    PlayerIdentity LogInAsGuest();
}
