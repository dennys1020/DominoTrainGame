using System;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Callbacks;

[CallbackBehavior(UseSynchronizationContext = true)]
public sealed class GameClientCallback : ILobbyServiceCallback
{
    private static readonly ITraceLog _logger;

    static GameClientCallback()
    {
        _logger = TraceLogManager.GetLogger(typeof(GameClientCallback));
    }

    public event EventHandler<PlayerJoinedEventArgs> PlayerJoined;

    public event EventHandler<PlayerLeftEventArgs> PlayerLeft;

    public event EventHandler<PlayerReadyChangedEventArgs> PlayerReadyChanged;

    public event EventHandler<LobbyMessageReceivedEventArgs> LobbyMessageReceived;

    public event EventHandler<MatchStartingEventArgs> MatchStarting;

    public event EventHandler<LobbyInvitationReceivedEventArgs> LobbyInvitationReceived;

    public void ReceivePlayerJoined(PlayerJoinedMessage message)
    {
        if (message?.Player == null)
        {
            _logger.Warn("A player join notice without player data was ignored.");
        }
        else
        {
            LobbyMember member = new LobbyMember(message.Player.PlayerId, message.Player.Username);
            LobbyPlayerResult player = new LobbyPlayerResult(
                member,
                message.Player.IsHost,
                message.Player.IsReady);
            PlayerJoinedEventArgs eventArgs = new PlayerJoinedEventArgs(
                player,
                message.Version);

            OnPlayerJoined(eventArgs);
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
            PlayerLeftEventArgs eventArgs = new PlayerLeftEventArgs(
                message.PlayerId,
                message.Version,
                message.HostPlayerId);

            OnPlayerLeft(eventArgs);
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
            PlayerReadyChangedEventArgs eventArgs = new PlayerReadyChangedEventArgs(
                message.PlayerId,
                message.IsReady,
                message.Version);

            OnPlayerReadyChanged(eventArgs);
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
            LobbyMember sender = new LobbyMember(message.PlayerId, message.Username);
            LobbyMessageReceivedEventArgs eventArgs = new LobbyMessageReceivedEventArgs(sender, message.Text);

            OnLobbyMessageReceived(eventArgs);
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
            MatchStartingEventArgs eventArgs = new MatchStartingEventArgs(message.Code);

            OnMatchStarting(eventArgs);
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

            OnLobbyInvitationReceived(eventArgs);
        }
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
}
