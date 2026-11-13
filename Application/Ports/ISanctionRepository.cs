using System;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public interface ISanctionRepository
{
    PlayerSanction FindActiveSanction(int playerId, DateTime now);

    int CountSanctions(int playerId);

    bool AddSanction(SanctionRecord sanctionRecord);
}
