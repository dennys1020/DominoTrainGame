using System;

namespace DominoTrainGame.Client.ViewModel.Navigation;

public sealed class ProfileOwner
{
    private static readonly ProfileOwner _signedInPlayer;

    static ProfileOwner()
    {
        _signedInPlayer = new ProfileOwner(string.Empty);
    }

    private ProfileOwner(string username)
    {
        Username = username;
    }

    public static ProfileOwner SignedInPlayer => _signedInPlayer;

    public string Username { get; }

    public bool IsSignedInPlayer => ReferenceEquals(this, _signedInPlayer);

    public static ProfileOwner CreateOtherPlayer(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("The username of the profile owner is required.", nameof(username));
        }

        ProfileOwner profileOwner = new ProfileOwner(username);
        return profileOwner;
    }
}
