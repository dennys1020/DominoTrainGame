using System;
using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.View.Navigation;

internal sealed class RemoteServices
{
    public RemoteServices(
        IRemoteGameServices gameServices,
        IRemoteSocialServices socialServices,
        IRemoteMatchServices matchServices)
    {
        GameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        SocialServices = socialServices ?? throw new ArgumentNullException(nameof(socialServices));
        MatchServices = matchServices ?? throw new ArgumentNullException(nameof(matchServices));
    }

    public IRemoteGameServices GameServices { get; }

    public IRemoteSocialServices SocialServices { get; }

    public IRemoteMatchServices MatchServices { get; }
}
