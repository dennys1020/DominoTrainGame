using System;
using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;
using DominoTrainGame.Client.Models.Networking;

namespace DominoTrainGame.Client.Models.Callbacks;

[CallbackBehavior(UseSynchronizationContext = true, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class SocialClientCallback : IFriendServiceCallback
{
    private static readonly ITraceLog _logger;

    static SocialClientCallback()
    {
        _logger = TraceLogManager.GetLogger(typeof(SocialClientCallback));
    }

    public event EventHandler<FriendStatusChangedEventArgs> FriendStatusChanged;

    public event EventHandler<EventArgs> FriendsChanged;

    public void ReceiveFriendStatusChanged(FriendStatusMessage message)
    {
        if (message == null)
        {
            _logger.Warn("A friend status notice without content was ignored.");
        }
        else
        {
            FriendStatus status = FriendStatusCodes.ToFriendStatus(message.Status);
            OnFriendStatusChanged(new FriendStatusChangedEventArgs(message.PlayerId, status));
        }
    }

    public void ReceiveFriendsChanged()
    {
        OnFriendsChanged(EventArgs.Empty);
    }

    private void OnFriendStatusChanged(FriendStatusChangedEventArgs eventArgs)
    {
        FriendStatusChanged?.Invoke(this, eventArgs);
    }

    private void OnFriendsChanged(EventArgs eventArgs)
    {
        FriendsChanged?.Invoke(this, eventArgs);
    }
}
