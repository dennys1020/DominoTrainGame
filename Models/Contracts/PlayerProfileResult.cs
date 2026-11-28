using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class PlayerProfileResult
{
    public PlayerProfileResult(ProfileActionResult status, PlayerProfile profile)
    {
        Status = status;
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));
    }

    public ProfileActionResult Status { get; }

    public PlayerProfile Profile { get; }

    public static PlayerProfileResult CreateFailure(ProfileActionResult status)
    {
        PlayerProfileResult result = new PlayerProfileResult(status, PlayerProfile.Empty);
        return result;
    }
}
