using System;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class PlayerProfile
{
    static PlayerProfile()
    {
        Empty = new PlayerProfile(
            string.Empty,
            string.Empty,
            Array.Empty<byte>(),
            DateTime.MinValue,
            ProfileSocialLinks.Empty,
            ProfileStatistics.Empty);
    }

    public PlayerProfile(
        string username,
        string biography,
        byte[] avatarImage,
        DateTime memberSince,
        ProfileSocialLinks socialLinks,
        ProfileStatistics statistics)
    {
        Username = username ?? string.Empty;
        Biography = biography ?? string.Empty;
        AvatarImage = avatarImage ?? Array.Empty<byte>();
        MemberSince = memberSince;
        SocialLinks = socialLinks ?? throw new ArgumentNullException(nameof(socialLinks));
        Statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
    }

    public static PlayerProfile Empty { get; }

    public string Username { get; }

    public string Biography { get; }

    public byte[] AvatarImage { get; }

    public DateTime MemberSince { get; }

    public ProfileSocialLinks SocialLinks { get; }

    public ProfileStatistics Statistics { get; }
}
