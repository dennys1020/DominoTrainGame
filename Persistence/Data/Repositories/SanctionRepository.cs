using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Persistence.Data.Repositories;

public sealed class SanctionRepository : ISanctionRepository
{
    private const int NoSavedRows = 0;
    private const string TemporaryBanType = "Temporal";
    private const string ChatSanctionReason = "Repeated offensive language in the lobby chat.";

    private readonly DatabaseExecutor _databaseExecutor;

    public SanctionRepository(DatabaseExecutor databaseExecutor)
    {
        _databaseExecutor = databaseExecutor ?? throw new ArgumentNullException(nameof(databaseExecutor));
    }

    public bool TryFindActiveSanction(int playerId, DateTime now, out PlayerSanction sanction)
    {
        List<Ban> activeBans = _databaseExecutor.Execute(context => FindActiveBans(context, playerId, now));
        bool isSanctioned = activeBans.Any();

        sanction = isSanctioned ? ToChatSanction(activeBans) : PlayerSanction.Empty;
        return isSanctioned;
    }

    public int CountSanctions(int playerId)
    {
        int sanctionCount = _databaseExecutor.Execute(context => CountChatSanctions(context, playerId));
        return sanctionCount;
    }

    public bool AddSanction(SanctionRecord sanctionRecord)
    {
        if (sanctionRecord == null)
        {
            throw new ArgumentNullException(nameof(sanctionRecord));
        }

        bool wasSaved = _databaseExecutor.Execute(context => InsertBan(context, sanctionRecord));
        return wasSaved;
    }

    private static List<Ban> FindActiveBans(DominoGameDBEntities context, int playerId, DateTime now)
    {
        List<Ban> activeBans = context.Bans
            .Where(ban => ban.PlayerId == playerId)
            .Where(ban => ban.ExpiresAt == null || ban.ExpiresAt > now)
            .ToList();
        return activeBans;
    }

    private static int CountChatSanctions(DominoGameDBEntities context, int playerId)
    {
        int sanctionCount = context.Bans
            .Where(ban => ban.PlayerId == playerId)
            .Count(ban => ban.Reason == ChatSanctionReason);
        return sanctionCount;
    }

    private static bool InsertBan(DominoGameDBEntities context, SanctionRecord sanctionRecord)
    {
        Ban ban = new Ban
        {
            PlayerId = sanctionRecord.PlayerId,
            Reason = ChatSanctionReason,
            BanType = TemporaryBanType,
            StartedAt = sanctionRecord.StartedAt,
            ExpiresAt = sanctionRecord.Sanction.ExpiresAt
        };

        context.Bans.Add(ban);
        bool wasSaved = context.SaveChanges() > NoSavedRows;
        return wasSaved;
    }

    private static PlayerSanction ToChatSanction(List<Ban> activeBans)
    {
        DateTime expiresAt = activeBans.Max(ban => ban.ExpiresAt ?? DateTime.MaxValue);
        PlayerSanction sanction = new PlayerSanction(SanctionType.ChatMute, expiresAt);
        return sanction;
    }
}
