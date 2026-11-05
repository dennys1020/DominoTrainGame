using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class FriendStatusUseCases : IFriendStatusUseCases
{
    private static readonly ITraceLog _logger;

    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IPresenceUseCases _presenceUseCases;
    private readonly ISocialNotifier _socialNotifier;

    static FriendStatusUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(FriendStatusUseCases));
    }

    public FriendStatusUseCases(
        IFriendshipRepository friendshipRepository,
        IPresenceUseCases presenceUseCases,
        ISocialNotifier socialNotifier)
    {
        _friendshipRepository = friendshipRepository
            ?? throw new ArgumentNullException(nameof(friendshipRepository));
        _presenceUseCases = presenceUseCases ?? throw new ArgumentNullException(nameof(presenceUseCases));
        _socialNotifier = socialNotifier ?? throw new ArgumentNullException(nameof(socialNotifier));
    }

    public void PublishStatus(int playerId)
    {
        try
        {
            IReadOnlyList<FriendSummary> friends = _friendshipRepository.GetFriends(playerId);
            List<int> friendPlayerIds = friends.Select(friend => friend.PlayerId).ToList();
            PlayerStatus status = _presenceUseCases.GetStatus(playerId);

            _socialNotifier.NotifyFriendStatusChanged(new FriendStatusNotice(friendPlayerIds, playerId, status));
        }
        catch (DatabaseUnavailableException exception)
        {
            _logger.Error("A status change was not published because the database is unavailable.", exception);
        }
    }
}
