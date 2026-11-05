using System;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.UseCases;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.Faults;

namespace DominoTrainGame.Server.Service.Sessions;

public sealed class SocialSessionManager
{
    private static readonly ITraceLog _logger;

    private readonly SessionTokenRegistry _sessionTokens;
    private readonly SocialSessionRegistry _socialSessions;
    private readonly IFriendStatusUseCases _friendStatusUseCases;

    static SocialSessionManager()
    {
        _logger = TraceLogManager.GetLogger(typeof(SocialSessionManager));
    }

    public SocialSessionManager(
        SessionTokenRegistry sessionTokens,
        SocialSessionRegistry socialSessions,
        IFriendStatusUseCases friendStatusUseCases)
    {
        _sessionTokens = sessionTokens ?? throw new ArgumentNullException(nameof(sessionTokens));
        _socialSessions = socialSessions ?? throw new ArgumentNullException(nameof(socialSessions));
        _friendStatusUseCases = friendStatusUseCases
            ?? throw new ArgumentNullException(nameof(friendStatusUseCases));
    }

    public void Connect(string sessionToken)
    {
        bool isValid = _sessionTokens.TryResolve(sessionToken, out AuthenticatedPlayer player);

        if (!isValid)
        {
            _logger.Warn("A social connection was rejected because the session token is invalid or expired.");
            throw GameFaultFactory.Create(GameServiceFaultCode.InvalidSessionToken);
        }

        OperationContext context = OperationContext.Current;
        ISocialClientCallback callback = context.GetCallbackChannel<ISocialClientCallback>();
        SocialSession session = new SocialSession(context.SessionId, player, callback);
        ICommunicationObject channel = context.Channel;

        _socialSessions.Register(session);
        channel.Closed += (sender, e) => HandleDisconnection(session, sessionToken);
        channel.Faulted += (sender, e) => HandleDisconnection(session, sessionToken);
        _logger.InfoFormat("A player connected to the social service. PlayerId: {0}.", player.PlayerId);
        _friendStatusUseCases.PublishStatus(player.PlayerId);
    }

    public int ResolveCallerId()
    {
        string sessionId = OperationContext.Current.SessionId;
        bool isConnected = _socialSessions.TryGetPlayer(sessionId, out AuthenticatedPlayer player);

        if (!isConnected)
        {
            throw GameFaultFactory.Create(GameServiceFaultCode.SessionNotFound);
        }

        return player.PlayerId;
    }

    private void HandleDisconnection(SocialSession session, string sessionToken)
    {
        bool wasRemoved = _socialSessions.Remove(session);

        if (wasRemoved)
        {
            _sessionTokens.Renew(sessionToken, session.Player);
            _logger.InfoFormat(
                "A player disconnected from the social service. PlayerId: {0}.",
                session.Player.PlayerId);
            _friendStatusUseCases.PublishStatus(session.Player.PlayerId);
        }
    }
}
