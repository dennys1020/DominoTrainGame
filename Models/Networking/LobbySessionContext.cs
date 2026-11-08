using System;
using DominoTrainGame.Client.Models.Callbacks;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class LobbySessionContext
{
    public LobbySessionContext(ClientSession session, GameClientCallback callback)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public ClientSession Session { get; }

    public GameClientCallback Callback { get; }
}
