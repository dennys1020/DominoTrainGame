using System;

namespace DominoTrainGame.Client.Models.Contracts;

public interface ILobbyEvents
{
    event EventHandler<EventArgs> LobbyChanged;

    event EventHandler<LobbyMessageReceivedEventArgs> LobbyMessageReceived;

    event EventHandler<MatchStartingEventArgs> MatchStarting;

    event EventHandler<LobbyInvitationReceivedEventArgs> LobbyInvitationReceived;

    event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;
}
