using System;
using System.ServiceModel;
using System.Threading;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;
using DominoTrainGame.Client.Models.Networking;

namespace DominoTrainGame.Client.Models.Callbacks;

[CallbackBehavior(UseSynchronizationContext = true)]
public sealed class GameClientCallback : ILobbyServiceCallback, ILobbyEvents, IMatchEvents
{
    private static readonly ITraceLog _logger;

    private readonly ClientSession _session;

    static GameClientCallback()
    {
        _logger = TraceLogManager.GetLogger(typeof(GameClientCallback));
    }

    public GameClientCallback(ClientSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public event EventHandler<EventArgs> LobbyChanged;

    public event EventHandler<LobbyMessageReceivedEventArgs> LobbyMessageReceived;

    public event EventHandler<MatchStartingEventArgs> MatchStarting;

    public event EventHandler<LobbyInvitationReceivedEventArgs> LobbyInvitationReceived;

    public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

    public event EventHandler<EventArgs> MatchChanged;

    public event EventHandler<EventArgs> HandChanged;

    public void ReceivePlayerJoined(PlayerJoinedMessage message)
    {
        if (message?.Player == null)
        {
            _logger.Warn("A player join notice without player data was ignored.");
        }
        else
        {
            ApplyLobbyUpdate(message.Version, lobby => LobbyRosterUpdates.CreateLobbyWithPlayer(lobby, message));
        }
    }

    public void ReceivePlayerLeft(PlayerLeftMessage message)
    {
        if (message == null)
        {
            _logger.Warn("A player leave notice without content was ignored.");
        }
        else
        {
            ApplyLobbyUpdate(message.Version, lobby => LobbyRosterUpdates.CreateLobbyWithoutPlayer(lobby, message));
        }
    }

    public void ReceivePlayerReadyChanged(PlayerReadyChangedMessage message)
    {
        if (message == null)
        {
            _logger.Warn("A ready state notice without content was ignored.");
        }
        else
        {
            ApplyLobbyUpdate(message.Version, lobby => LobbyRosterUpdates.CreateLobbyWithReadyState(lobby, message));
        }
    }

    public void ReceiveLobbyMessage(LobbyChatMessage message)
    {
        if (message == null)
        {
            _logger.Warn("A lobby message notice without content was ignored.");
        }
        else
        {
            PlayerSummary sender = new PlayerSummary(message.PlayerId, message.Username);
            OnLobbyMessageReceived(new LobbyMessageReceivedEventArgs(sender, message.Text));
        }
    }

    public void ReceiveMatchStarting(MatchStartingMessage message)
    {
        if (message == null)
        {
            _logger.Warn("A match start notice without content was ignored.");
        }
        else
        {
            OnMatchStarting(new MatchStartingEventArgs(message.Code));
        }
    }

    public void ReceiveLobbyInvitation(LobbyInvitationMessage message)
    {
        if (message == null)
        {
            _logger.Warn("A lobby invitation notice without content was ignored.");
        }
        else
        {
            LobbyInvitationReceivedEventArgs eventArgs = new LobbyInvitationReceivedEventArgs(
                message.InviterUsername,
                message.Code);
            SynchronizationContext callerContext = _session.CallerContext;

            callerContext.Post(state => OnLobbyInvitationReceived(eventArgs), eventArgs);
        }
    }

    public void ReceiveMatchUpdate(MatchSnapshotMessage message)
    {
        if (message == null)
        {
            _logger.Warn("A match update notice without content was ignored.");
        }
        else
        {
            MatchSnapshot snapshot = MatchSnapshotMapper.ToMatchSnapshot(message, _session.CurrentLobby);

            _session.StoreMatch(snapshot);
            OnMatchChanged(EventArgs.Empty);
        }
    }

    public void ReceiveHand(HandMessage message)
    {
        if (message == null)
        {
            _logger.Warn("A hand notice without context was ignored");
        }
        else
        {
            PlayerHand hand = MatchSnapshotMapper.ToPlayerHand(message);

            _session.StoreHand(hand);
            OnHandChanged(EventArgs.Empty);
        }
    }

    public void ReportConnectionState(ConnectionState connectionState)
    {
        OnConnectionStateChanged(new ConnectionStateChangedEventArgs(connectionState));
    }

    private void ApplyLobbyUpdate(long version, Func<LobbyResult, LobbyResult> update)
    {
        bool wasApplied = _session.TryApplyLobbyUpdate(version, update);

        if (wasApplied)
        {
            OnLobbyChanged(EventArgs.Empty);
        }
    }

    private void OnLobbyChanged(EventArgs eventArgs)
    {
        LobbyChanged?.Invoke(this, eventArgs);
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

    private void OnMatchChanged(EventArgs eventArgs)
    {
        MatchChanged?.Invoke(this, eventArgs);
    }

    private void OnHandChanged(EventArgs eventArgs)
    {
        HandChanged?.Invoke(this, eventArgs);
    }
}
