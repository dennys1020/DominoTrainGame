namespace DominoTrainGame.Server.Application.Domain;

public sealed class SocialLinks
{
    static SocialLinks()
    {
        Empty = new SocialLinks(string.Empty, string.Empty, string.Empty);
    }

    public SocialLinks(string instagramUsername, string facebookUsername, string twitterUsername)
    {
        InstagramUsername = instagramUsername ?? string.Empty;
        FacebookUsername = facebookUsername ?? string.Empty;
        TwitterUsername = twitterUsername ?? string.Empty;
    }

    public static SocialLinks Empty { get; }

    public string InstagramUsername { get; }

    public string FacebookUsername { get; }

    public string TwitterUsername { get; }

    public bool IsEmpty => ReferenceEquals(this, Empty);
}
