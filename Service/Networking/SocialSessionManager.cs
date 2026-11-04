using System;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Authentication;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Networking;

public sealed class SocialSessionManager
{
    private static readonly ITraceLog _logger;

    private readonly SessionTokenRegistry _sessionTokens;
    private readonly SocialSessionRegistry _socialSessions;

    static SocialSessionManager()
    {
        _logger = TraceLogManager.GetLogger(typeof(SocialSessionManager));
    }

    public SocialSessionManager(SessionTokenRegistry sessionTokens, SocialSessionRegistry socialSessions)
    {
        _sessionTokens = sessionTokens ?? throw new ArgumentNullException(nameof(sessionTokens));
        _socialSessions = socialSessions ?? throw new ArgumentNullException(nameof(socialSessions));
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
        SocialSession session = new SocialSession(context.SessionId, sessionToken, player);
        ICommunicationObject channel = context.Channel;

        _socialSessions.Register(session);
        channel.Closed += (sender, e) => HandleDisconnection(session);
        channel.Faulted += (sender, e) => HandleDisconnection(session);
        _logger.InfoFormat("A player connected to the social service. PlayerId: {0}.", player.PlayerId);
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

    private void HandleDisconnection(SocialSession session)
    {
        bool wasRemoved = _socialSessions.Remove(session);

        if (wasRemoved)
        {
            _sessionTokens.Renew(session.SessionToken, session.Player);
            _logger.InfoFormat(
                "A player disconnected from the social service. PlayerId: {0}.",
                session.Player.PlayerId);
        }
    }
}
