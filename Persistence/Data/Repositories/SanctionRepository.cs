using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Persistence.Data.Repositories;

public sealed class SanctionRepository : ISanctionRepository
{
    private const string ChatMuteBanType = "ChatMute";
    private const string GameBanType = "GameBan";
    private const string ChatMuteReason = "Repeated offensive language in the lobby chat.";
    private const string GameBanReason = "Repeated chat sanctions for offensive language.";

    private readonly DatabaseExecutor _databaseExecutor;

    public SanctionRepository(DatabaseExecutor databaseExecutor)
    {
        _databaseExecutor = databaseExecutor ?? throw new ArgumentNullException(nameof(databaseExecutor));
    }

    public PlayerSanction FindActiveSanction(int playerId, DateTime now)
    {
        List<Ban> activeBans = _databaseExecutor.Execute(context => FindActiveBans(context, playerId, now));
        PlayerSanction sanction = activeBans.Any() ? ToSanction(activeBans) : PlayerSanction.Empty;

        return sanction;
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
        List<Ban> activeBans = (from ban in context.Bans
                                where ban.PlayerId == playerId
                                    && (ban.ExpiresAt == null || ban.ExpiresAt > now)
                                select ban).ToList();
        return activeBans;
    }

    private static int CountChatSanctions(DominoGameDBEntities context, int playerId)
    {
        int sanctionCount = (from ban in context.Bans
                             where ban.PlayerId == playerId
                                 && (ban.BanType == ChatMuteBanType || ban.BanType == GameBanType)
                             select ban).Count();
        return sanctionCount;
    }

    private static bool InsertBan(DominoGameDBEntities context, SanctionRecord sanctionRecord)
    {
        string banType = ToBanType(sanctionRecord.Sanction.Type);
        Ban ban = new Ban
        {
            PlayerId = sanctionRecord.PlayerId,
            Reason = ToReason(banType),
            BanType = banType,
            StartedAt = sanctionRecord.StartedAt,
            ExpiresAt = sanctionRecord.Sanction.ExpiresAt
        };

        context.Bans.Add(ban);
        bool wasSaved = ContextChanges.Save(context);
        return wasSaved;
    }

    private static string ToBanType(SanctionType sanctionType)
    {
        string banType = sanctionType == SanctionType.GameBan ? GameBanType : ChatMuteBanType;
        return banType;
    }

    private static string ToReason(string banType)
    {
        string reason = banType == GameBanType ? GameBanReason : ChatMuteReason;
        return reason;
    }

    private static PlayerSanction ToSanction(List<Ban> activeBans)
    {
        DateTime expiresAt = DateTime.MinValue;
        SanctionType sanctionType = SanctionType.ChatMute;

        foreach (Ban activeBan in activeBans)
        {
            DateTime banExpiresAt = activeBan.ExpiresAt ?? DateTime.MaxValue;
            expiresAt = banExpiresAt > expiresAt ? banExpiresAt : expiresAt;
            sanctionType = activeBan.BanType == GameBanType ? SanctionType.GameBan : sanctionType;
        }

        PlayerSanction sanction = new PlayerSanction(sanctionType, expiresAt);
        return sanction;
    }
}
