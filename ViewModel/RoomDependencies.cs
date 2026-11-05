using System;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;

namespace DominoTrainGame.Client.ViewModel;

public sealed class RoomDependencies
{
    public RoomDependencies(
        IRemoteGameServices gameServices,
        IRemoteSocialServices socialServices,
        LocalizationViewModel localization)
    {
        GameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        SocialServices = socialServices ?? throw new ArgumentNullException(nameof(socialServices));
        Localization = localization ?? throw new ArgumentNullException(nameof(localization));
    }

    public IRemoteGameServices GameServices { get; }

    public IRemoteSocialServices SocialServices { get; }

    public LocalizationViewModel Localization { get; }
}
