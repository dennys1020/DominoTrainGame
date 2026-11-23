using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Guards;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class PlayerProfileSummary
{
    private static readonly ITraceLog _logger;

    static PlayerProfileSummary()
    {
        _logger = TraceLogManager.GetLogger(typeof(PlayerProfileSummary));
        Empty = new PlayerProfileSummary(PlayerIdentity.Empty, ProfileDetails.Empty, SocialLinks.Empty);
    }

    public PlayerProfileSummary(PlayerIdentity player, ProfileDetails details, SocialLinks socialLinks)
    {
        Player = ArgumentGuard.NotNull(player, nameof(player), _logger);
        Details = ArgumentGuard.NotNull(details, nameof(details), _logger);
        SocialLinks = ArgumentGuard.NotNull(socialLinks, nameof(socialLinks), _logger);
    }

    public static PlayerProfileSummary Empty { get; }

    public PlayerIdentity Player { get; }

    public ProfileDetails Details { get; }

    public SocialLinks SocialLinks { get; }

    public bool IsEmpty => ReferenceEquals(this, Empty);
}
