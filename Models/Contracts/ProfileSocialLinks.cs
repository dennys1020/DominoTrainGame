namespace DominoTrainGame.Client.Models.Contracts;

public sealed class ProfileSocialLinks
{
    static ProfileSocialLinks()
    {
        Empty = new ProfileSocialLinks(string.Empty, string.Empty, string.Empty);
    }

    public ProfileSocialLinks(string instagramUsername, string facebookUsername, string twitterUsername)
    {
        InstagramUsername = instagramUsername ?? string.Empty;
        FacebookUsername = facebookUsername ?? string.Empty;
        TwitterUsername = twitterUsername ?? string.Empty;
    }

    public static ProfileSocialLinks Empty { get; }

    public string InstagramUsername { get; }

    public string FacebookUsername { get; }

    public string TwitterUsername { get; }
}
