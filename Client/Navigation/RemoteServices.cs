using System;
using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.View.Navigation;

internal sealed class RemoteServices
{
    public RemoteServices(
        IRemoteAuthenticationServices authenticationServices,
        IRemoteGameServices gameServices,
        IRemoteSocialServices socialServices,
        IRemoteMatchServices matchServices,
        IRemoteProfileServices profileServices)
    {
        AuthenticationServices = authenticationServices
            ?? throw new ArgumentNullException(nameof(authenticationServices));
        GameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        SocialServices = socialServices ?? throw new ArgumentNullException(nameof(socialServices));
        MatchServices = matchServices ?? throw new ArgumentNullException(nameof(matchServices));
        ProfileServices = profileServices ?? throw new ArgumentNullException(nameof(profileServices));
    }

    public IRemoteAuthenticationServices AuthenticationServices { get; }

    public IRemoteGameServices GameServices { get; }

    public IRemoteSocialServices SocialServices { get; }

    public IRemoteMatchServices MatchServices { get; }

    public IRemoteProfileServices ProfileServices { get; }
}
