using System;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.State;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class ChatSanctionUseCases : IChatSanctionUseCases
{
    private const int NoPreviousSanctions = 0;
    private const int OnePreviousSanction = 1;
    private const int FirstChatMuteMinutes = 2;
    private const int WarningThreshold = 7;
    private const int SanctionThreshold = 10;
    private const int SecondChatMuteMinutes = 15;
    private const int GameBanMinutes = 30;

    private static readonly ITraceLog _logger;

    private readonly ISanctionRepository _sanctionRepository;
    private readonly ChatInfractionRegistry _infractionRegistry;
    private readonly IClock _clock;

    static ChatSanctionUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(ChatSanctionUseCases));
    }

    public ChatSanctionUseCases(
        ISanctionRepository sanctionRepository,
        ChatInfractionRegistry infractionRegistry,
        IClock clock)
    {
        _sanctionRepository = sanctionRepository ?? throw new ArgumentNullException(nameof(sanctionRepository));
        _infractionRegistry = infractionRegistry ?? throw new ArgumentNullException(nameof(infractionRegistry));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public ChatPermissionStatus CheckChatPermission(int playerId)
    {
        bool isSanctioned = _sanctionRepository.TryFindActiveSanction(
            playerId,
            _clock.UtcNow,
            out PlayerSanction sanction);
        ChatPermissionStatus status = ChatPermissionStatus.Allowed;

        if (isSanctioned)
        {
            status = ToPermissionStatus(sanction.Type);
        }

        return status;
    }

    public ChatInfractionStatus RegisterInfraction(int playerId)
    {
        int infractionCount = _infractionRegistry.Add(playerId);
        ChatInfractionStatus status = ChatInfractionStatus.Counted;

        if (infractionCount >= SanctionThreshold)
        {
            status = ApplySanction(playerId);
        }
        else if (infractionCount >= WarningThreshold)
        {
            status = ChatInfractionStatus.Warned;
        }

        _logger.WarnFormat(
            "A chat infraction was registered. PlayerId: {0}. Count: {1}. Status: {2}.",
            playerId,
            infractionCount,
            status);
        return status;
    }

    public bool IsBannedFromPlaying(int playerId)
    {
        bool isSanctioned = _sanctionRepository.TryFindActiveSanction(
            playerId,
            _clock.UtcNow,
            out PlayerSanction sanction);
        bool isBanned = isSanctioned && sanction.Type == SanctionType.GameBan;
        return isBanned;
    }

    private static ChatPermissionStatus ToPermissionStatus(SanctionType sanctionType)
    {
        ChatPermissionStatus status;

        switch (sanctionType)
        {
            case SanctionType.GameBan:
                status = ChatPermissionStatus.GameBanned;
                break;
            default:
                status = ChatPermissionStatus.ChatBlocked;
                break;
        }

        return status;
    }

    private static PlayerSanction CreateSanction(int previousSanctions, DateTime startedAt)
    {
        PlayerSanction sanction;

        if (previousSanctions == NoPreviousSanctions)
        {
            sanction = new PlayerSanction(SanctionType.ChatMute, startedAt.AddMinutes(FirstChatMuteMinutes));
        }
        else if (previousSanctions == OnePreviousSanction)
        {
            sanction = new PlayerSanction(SanctionType.ChatMute, startedAt.AddMinutes(SecondChatMuteMinutes));
        }
        else
        {
            sanction = new PlayerSanction(SanctionType.GameBan, startedAt.AddMinutes(GameBanMinutes));
        }

        return sanction;
    }

    private ChatInfractionStatus ApplySanction(int playerId)
    {
        DateTime startedAt = _clock.UtcNow;
        int previousSanctions = _sanctionRepository.CountSanctions(playerId);
        PlayerSanction sanction = CreateSanction(previousSanctions, startedAt);

        _sanctionRepository.AddSanction(new SanctionRecord(playerId, sanction, startedAt));
        _infractionRegistry.Reset(playerId);
        _logger.WarnFormat(
            "A player was sanctioned for offensive chat messages. PlayerId: {0}. Type: {1}.",
            playerId,
            sanction.Type);

        ChatInfractionStatus status = sanction.Type == SanctionType.GameBan
            ? ChatInfractionStatus.GameBanned
            : ChatInfractionStatus.ChatBlocked;
        return status;
    }
}
