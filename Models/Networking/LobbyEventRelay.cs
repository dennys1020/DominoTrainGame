using System;
using System.Threading;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class LobbyEventRelay
{
    private readonly ClientSession _session;

    public LobbyEventRelay(ClientSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public event EventHandler<PlayerJoinedEventArgs> PlayerJoined;

    public event EventHandler<PlayerLeftEventArgs> PlayerLeft;

    public event EventHandler<PlayerReadyChangedEventArgs> PlayerReadyChanged;

    public event EventHandler<LobbyMessageReceivedEventArgs> LobbyMessageReceived;

    public event EventHandler<MatchStartingEventArgs> MatchStarting;

    public event EventHandler<LobbyInvitationReceivedEventArgs> LobbyInvitationReceived;

    public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

    public GameClientCallback CreateCallback()
    {
        GameClientCallback callback = new GameClientCallback();
        callback.PlayerJoined += HandlePlayerJoined;
        callback.PlayerLeft += HandlePlayerLeft;
        callback.PlayerReadyChanged += HandlePlayerReadyChanged;
        callback.LobbyMessageReceived += HandleLobbyMessageReceived;
        callback.MatchStarting += HandleMatchStarting;
        callback.LobbyInvitationReceived += HandleLobbyInvitationReceived;
        return callback;
    }

    public void ReportConnectionState(ConnectionState connectionState)
    {
        OnConnectionStateChanged(new ConnectionStateChangedEventArgs(connectionState));
    }

    private void HandlePlayerJoined(object sender, PlayerJoinedEventArgs eventArgs)
    {
        bool wasApplied = _session.TryApplyLobbyUpdate(
            eventArgs.Version,
            lobby => LobbyRosterUpdates.CreateLobbyWithPlayer(lobby, eventArgs));

        if (wasApplied)
        {
            OnPlayerJoined(eventArgs);
        }
    }

    private void HandlePlayerLeft(object sender, PlayerLeftEventArgs eventArgs)
    {
        bool wasApplied = _session.TryApplyLobbyUpdate(
            eventArgs.Version,
            lobby => LobbyRosterUpdates.CreateLobbyWithoutPlayer(lobby, eventArgs));

        if (wasApplied)
        {
            OnPlayerLeft(eventArgs);
        }
    }

    private void HandlePlayerReadyChanged(object sender, PlayerReadyChangedEventArgs eventArgs)
    {
        bool wasApplied = _session.TryApplyLobbyUpdate(
            eventArgs.Version,
            lobby => LobbyRosterUpdates.CreateLobbyWithReadyState(lobby, eventArgs));

        if (wasApplied)
        {
            OnPlayerReadyChanged(eventArgs);
        }
    }

    private void HandleLobbyMessageReceived(object sender, LobbyMessageReceivedEventArgs eventArgs)
    {
        OnLobbyMessageReceived(eventArgs);
    }

    private void HandleMatchStarting(object sender, MatchStartingEventArgs eventArgs)
    {
        OnMatchStarting(eventArgs);
    }

    private void HandleLobbyInvitationReceived(object sender, LobbyInvitationReceivedEventArgs eventArgs)
    {
        SynchronizationContext callerContext = _session.CallerContext;
        callerContext.Post(state => OnLobbyInvitationReceived(eventArgs), eventArgs);
    }

    private void OnPlayerJoined(PlayerJoinedEventArgs eventArgs)
    {
        PlayerJoined?.Invoke(this, eventArgs);
    }

    private void OnPlayerLeft(PlayerLeftEventArgs eventArgs)
    {
        PlayerLeft?.Invoke(this, eventArgs);
    }

    private void OnPlayerReadyChanged(PlayerReadyChangedEventArgs eventArgs)
    {
        PlayerReadyChanged?.Invoke(this, eventArgs);
    }

    private void OnLobbyMessageReceived(LobbyMessageReceivedEventArgs eventArgs)
    {
        LobbyMessageReceived?.Invoke(this, eventArgs);
    }

    private void OnMatchStarting(MatchStartingEventArgs eventArgs)
    {
        MatchStarting?.Invoke(this, eventArgs);
    }

    private void OnLobbyInvitationReceived(LobbyInvitationReceivedEventArgs eventArgs)
    {
        LobbyInvitationReceived?.Invoke(this, eventArgs);
    }

    private void OnConnectionStateChanged(ConnectionStateChangedEventArgs eventArgs)
    {
        ConnectionStateChanged?.Invoke(this, eventArgs);
    }
}
