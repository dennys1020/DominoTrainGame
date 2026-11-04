using System;

namespace DominoTrainGame.Server.Service.Implementation;

public sealed class SessionController
{
    public SessionController(LobbyController lobbyController, FriendController friendController)
    {
        LobbyController = lobbyController ?? throw new ArgumentNullException(nameof(lobbyController));
        FriendController = friendController ?? throw new ArgumentNullException(nameof(friendController));
    }

    public LobbyController LobbyController { get; }

    public FriendController FriendController { get; }
}
