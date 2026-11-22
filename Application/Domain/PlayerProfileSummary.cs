using System;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class PlayerProfileSummary
{
    static PlayerProfileSummary()
    {
        Empty = new PlayerProfileSummary(PlayerIdentity.Empty, ProfileDetails.Empty, SocialLinks.Empty);
    }

    public PlayerProfileSummary(PlayerIdentity player, ProfileDetails details, SocialLinks socialLinks)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Details = details ?? throw new ArgumentNullException(nameof(details));
        SocialLinks = socialLinks ?? throw new ArgumentNullException(nameof(socialLinks));
    }

    public static PlayerProfileSummary Empty { get; }

    public PlayerIdentity Player { get; }

    public ProfileDetails Details { get; }

    public SocialLinks SocialLinks { get; }

    public bool IsEmpty => ReferenceEquals(this, Empty);
}
