using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Guards;
using DominoTrainGame.Server.Service.Faults;

namespace DominoTrainGame.Server.Service.Sessions;

public static class TokenCallerResolver
{
    private static readonly ITraceLog _logger;

    static TokenCallerResolver()
    {
        _logger = TraceLogManager.GetLogger(typeof(TokenCallerResolver));
    }

    public static PlayerIdentity ResolveSignedInCaller(SessionTokenRegistry sessionTokens, string sessionToken)
    {
        ArgumentGuard.ThrowIfNull(sessionTokens, nameof(sessionTokens), _logger);
        PlayerIdentity caller = sessionTokens.Resolve(sessionToken);

        if (caller.IsEmpty)
        {
            throw GameFaultFactory.Create(GameServiceFaultCode.SessionNotFound);
        }

        sessionTokens.Renew(sessionToken);

        return caller;
    }

    public static PlayerIdentity ResolveRegisteredCaller(SessionTokenRegistry sessionTokens, string sessionToken)
    {
        PlayerIdentity caller = ResolveSignedInCaller(sessionTokens, sessionToken);

        if (caller.IsGuest)
        {
            _logger.InfoFormat(
                "An account-only request was rejected because the player is a guest. PlayerId: {0}.",
                caller.PlayerId);
            throw GameFaultFactory.Create(GameServiceFaultCode.GuestNotAllowed);
        }

        return caller;
    }
}
