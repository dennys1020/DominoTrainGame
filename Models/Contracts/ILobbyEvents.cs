using System;

namespace DominoTrainGame.Client.Models.Contracts;

public interface ILobbyEvents
{
    event EventHandler<PlayerJoinedEventArgs> PlayerJoined;

    event EventHandler<PlayerLeftEventArgs> PlayerLeft;

    event EventHandler<PlayerReadyChangedEventArgs> PlayerReadyChanged;

    event EventHandler<LobbyMessageReceivedEventArgs> LobbyMessageReceived;

    event EventHandler<MatchStartingEventArgs> MatchStarting;

    event EventHandler<LobbyInvitationReceivedEventArgs> LobbyInvitationReceived;

    event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;
}
