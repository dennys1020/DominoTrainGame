using System;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IFriendEvents
{
    event EventHandler<FriendStatusChangedEventArgs> FriendStatusChanged;

    event EventHandler<EventArgs> FriendsChanged;
}
