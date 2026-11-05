using System;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public interface ISanctionRepository
{
    bool TryFindActiveSanction(int playerId, DateTime now, out PlayerSanction sanction);

    int CountSanctions(int playerId);

    bool AddSanction(SanctionRecord sanctionRecord);
}
