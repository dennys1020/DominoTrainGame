using System;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Authentication;
using DominoTrainGame.Server.Application.Connection;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DTO;

namespace DominoTrainGame.Server.Service.Networking;

public sealed class SessionManager
{
    private static readonly ITraceLog _logger;

    private readonly ConnectedPlayerRegistry _connectedPlayers;
    private readonly ILobbyUseCases _lobbyUseCases;
    private readonly SessionTokenRegistry _sessionTokens;

    static SessionManager()
    {
        _logger = TraceLogManager.GetLogger(typeof(SessionManager));
    }

    public SessionManager(
        ConnectedPlayerRegistry connectedPlayers,
        ILobbyUseCases lobbyUseCases,
        SessionTokenRegistry sessionTokens)
    {
        _connectedPlayers = connectedPlayers ?? throw new ArgumentNullException(nameof(connectedPlayers));
        _lobbyUseCases = lobbyUseCases ?? throw new ArgumentNullException(nameof(lobbyUseCases));
        _sessionTokens = sessionTokens ?? throw new ArgumentNullException(nameof(sessionTokens));
    }

    public string IssueToken(AuthenticatedPlayer player)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        bool isAlreadyConnected = _connectedPlayers.IsConnected(player.PlayerId);

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
        AuthenticatedPlayer player = ResolveToken(sessionToken);

        // The server may not have noticed that the previous channel dropped yet.
        // The token proves it is the same player, so the new channel replaces the old one.
        ReplacePreviousConnection(player.PlayerId);
        Register(player, sessionToken);
    }

    public AuthenticatedPlayer ResolveCaller()
    {
        string sessionId = OperationContext.Current.SessionId;
        bool isConnected = _connectedPlayers.TryGetPlayer(sessionId, out AuthenticatedPlayer player);

        if (!isConnected)
        {
            throw GameFaultFactory.Create(GameServiceFaultCode.SessionNotFound);
        }

        return player;
    }

    private AuthenticatedPlayer ResolveToken(string sessionToken)
    {
        bool isValid = _sessionTokens.TryResolve(sessionToken, out AuthenticatedPlayer player);

        if (!isValid)
        {
            _logger.Warn("A connection was rejected because the session token is invalid or expired.");
            throw GameFaultFactory.Create(GameServiceFaultCode.InvalidSessionToken);
        }

        return player;
    }

    private void ReplacePreviousConnection(int playerId)
    {
        bool wasConnected = _connectedPlayers.TryDisconnect(playerId);

        if (wasConnected)
        {
            _logger.InfoFormat("A stale connection was replaced. PlayerId: {0}.", playerId);
            _lobbyUseCases.LeaveLobby(playerId);
        }
    }

    private void Register(AuthenticatedPlayer player, string sessionToken)
    {
        OperationContext context = OperationContext.Current;
        string sessionId = context.SessionId;
        IGameClientCallback callback = context.GetCallbackChannel<IGameClientCallback>();
        ConnectedPlayer connectedPlayer = new ConnectedPlayer(sessionId, player, callback);
        bool wasAdded = _connectedPlayers.TryAdd(connectedPlayer);

        if (!wasAdded)
        {
            _logger.WarnFormat(
                "A connection was rejected because the player is already connected. PlayerId: {0}.",
                player.PlayerId);
            throw GameFaultFactory.Create(GameServiceFaultCode.AlreadyConnected);
        }

        ICommunicationObject channel = context.Channel;

        channel.Closed += (sender, eventArgs) => HandleDisconnection(sessionId, sessionToken);
        channel.Faulted += (sender, eventArgs) => HandleDisconnection(sessionId, sessionToken);
        _logger.InfoFormat("A player connected. PlayerId: {0}.", player.PlayerId);
    }

    private void HandleDisconnection(string sessionId, string sessionToken)
    {
        bool wasConnected = _connectedPlayers.Remove(sessionId, out AuthenticatedPlayer player);

        if (wasConnected)
        {
            _logger.InfoFormat("A player disconnected. PlayerId: {0}.", player.PlayerId);
            _sessionTokens.Renew(sessionToken, player);
            _lobbyUseCases.LeaveLobby(player.PlayerId);
        }
    }
}
