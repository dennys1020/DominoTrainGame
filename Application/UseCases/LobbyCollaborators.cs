using System;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class LobbyCollaborators
{
    public LobbyCollaborators(
        IChatMessageFilter messageFilter,
        IChatSanctionUseCases chatSanctions,
        IFriendStatusUseCases friendStatus)
    {
        MessageFilter = messageFilter ?? throw new ArgumentNullException(nameof(messageFilter));
        ChatSanctions = chatSanctions ?? throw new ArgumentNullException(nameof(chatSanctions));
        FriendStatus = friendStatus ?? throw new ArgumentNullException(nameof(friendStatus));
    }

    public IChatMessageFilter MessageFilter { get; }

    public IChatSanctionUseCases ChatSanctions { get; }

    public IFriendStatusUseCases FriendStatus { get; }
}
