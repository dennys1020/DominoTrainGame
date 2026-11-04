using System;
using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Ports;

public sealed class MatchStartingNotice
{
    public MatchStartingNotice(IReadOnlyList<int> recipientPlayerIds, string code)
    {
        RecipientPlayerIds = recipientPlayerIds ?? throw new ArgumentNullException(nameof(recipientPlayerIds));
        Code = code ?? string.Empty;
    }

    public IReadOnlyList<int> RecipientPlayerIds { get; }

    public string Code { get; }
}
