using System;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class LobbySessionContext
{
    public LobbySessionContext(ClientSession session, LobbyEventRelay events)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Events = events ?? throw new ArgumentNullException(nameof(events));
    }

    public ClientSession Session { get; }

    public LobbyEventRelay Events { get; }
}
