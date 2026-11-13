using System;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Service.Faults;

namespace DominoTrainGame.Server.Service.Sessions;

public sealed class PlayerSessionManager<TCallback>
    where TCallback : class
{
    private static readonly ITraceLog _logger;
    private static readonly string _channelName;

    private readonly PlayerSessionRegistry<TCallback> _sessions;
    private readonly SessionTokenRegistry _sessionTokens;

    static PlayerSessionManager()
    {
        _logger = TraceLogManager.GetLogger(typeof(PlayerSessionManager<TCallback>));
        _channelName = typeof(TCallback).Name;
    }

    public PlayerSessionManager(PlayerSessionRegistry<TCallback> sessions, SessionTokenRegistry sessionTokens)
    {
        _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        _sessionTokens = sessionTokens ?? throw new ArgumentNullException(nameof(sessionTokens));
    }

    public event EventHandler<PlayerSessionEventArgs> PlayerConnected;

    public event EventHandler<PlayerSessionEventArgs> PlayerDisconnected;

    public string IssueToken(PlayerIdentity player)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        bool isAlreadyConnected = _sessions.IsOnline(player.PlayerId);

        if (isAlreadyConnected)
        {
            _logger.WarnFormat(
                "A login was rejected because the player is already connected. PlayerId: {0}.",
                player.PlayerId);
            throw GameFaultFactory.Create(GameServiceFaultCode.AlreadyConnected);
        }

        string sessionToken = _sessionTokens.Issue(player);
        return sessionToken;
    }

    public void Connect(string sessionToken)
    {
        PlayerIdentity player = ResolveToken(sessionToken);

        ReplacePreviousConnection(player.PlayerId);
        Register(player, sessionToken);
        OnPlayerConnected(new PlayerSessionEventArgs(player.PlayerId));
    }

    public PlayerIdentity ResolveCaller()
    {
        string sessionId = OperationContext.Current.SessionId;
        PlayerIdentity player = _sessions.FindPlayer(sessionId);

        if (player.IsEmpty)
        {
            throw GameFaultFactory.Create(GameServiceFaultCode.SessionNotFound);
        }

        return player;
    }

    private PlayerIdentity ResolveToken(string sessionToken)
    {
        PlayerIdentity player = _sessionTokens.Resolve(sessionToken);

        if (player.IsEmpty)
        {
            _logger.WarnFormat(
                "A connection was rejected because the session token is invalid or expired. Channel: {0}.",
                _channelName);
            throw GameFaultFactory.Create(GameServiceFaultCode.InvalidSessionToken);
        }

        return player;
    }

    private void ReplacePreviousConnection(int playerId)
    {
        bool wasConnected = _sessions.TryDisconnect(playerId);

        if (wasConnected)
        {
            _logger.InfoFormat("A stale connection was replaced. Channel: {0}. PlayerId: {1}.", _channelName, playerId);
            OnPlayerDisconnected(new PlayerSessionEventArgs(playerId));
        }
    }

    private void Register(PlayerIdentity player, string sessionToken)
    {
        OperationContext context = OperationContext.Current;
        TCallback callback = context.GetCallbackChannel<TCallback>();
        PlayerSession<TCallback> session = new PlayerSession<TCallback>(context.SessionId, player, callback);
        bool wasAdded = _sessions.TryAdd(session);

        if (!wasAdded)
        {
            _logger.WarnFormat(
                "A connection was rejected because the player is already connected. Channel: {0}. PlayerId: {1}.",
                _channelName,
                player.PlayerId);
            throw GameFaultFactory.Create(GameServiceFaultCode.AlreadyConnected);
        }

        ICommunicationObject channel = context.Channel;

        channel.Closed += (sender, eventArgs) => HandleDisconnection(session, sessionToken);
        channel.Faulted += (sender, eventArgs) => HandleDisconnection(session, sessionToken);
        _logger.InfoFormat("A player connected. Channel: {0}. PlayerId: {1}.", _channelName, player.PlayerId);
    }

    private void HandleDisconnection(PlayerSession<TCallback> session, string sessionToken)
    {
        bool wasConnected = _sessions.Remove(session);

        if (wasConnected)
        {
            _logger.InfoFormat(
                "A player disconnected. Channel: {0}. PlayerId: {1}.",
                _channelName,
                session.Player.PlayerId);
            _sessionTokens.Renew(sessionToken, session.Player);
            OnPlayerDisconnected(new PlayerSessionEventArgs(session.Player.PlayerId));
        }
    }

    private void OnPlayerConnected(PlayerSessionEventArgs eventArgs)
    {
        PlayerConnected?.Invoke(this, eventArgs);
    }

    private void OnPlayerDisconnected(PlayerSessionEventArgs eventArgs)
    {
        PlayerDisconnected?.Invoke(this, eventArgs);
    }
}
