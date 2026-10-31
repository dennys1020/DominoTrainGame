using System;
using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class PlayerLeftNotice
{
    public PlayerLeftNotice(
        IReadOnlyList<int> recipientPlayerIds,
        int playerId,
        DominoTrainGame.Server.Application.Connection.LobbyRoster roster)
    {
        RecipientPlayerIds = recipientPlayerIds ?? throw new ArgumentNullException(nameof(recipientPlayerIds));
        PlayerId = playerId;
        Roster = roster ?? throw new ArgumentNullException(nameof(roster));
    }

    public IReadOnlyList<int> RecipientPlayerIds { get; }

    public int PlayerId { get; }

    public DominoTrainGame.Server.Application.Connection.LobbyRoster Roster { get; }

    public long Version => Roster.Version;

    public int HostPlayerId => Roster.HostPlayerId;
}
