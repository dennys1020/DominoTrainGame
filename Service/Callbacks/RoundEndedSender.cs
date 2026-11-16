using System;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DataContracts;

namespace DominoTrainGame.Server.Service.Callbacks;

public sealed class RoundEndedSender
{
    private readonly RoundEndedMessage _message;

    public RoundEndedSender(RoundEndedMessage message)
    {
        _message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public void Send(IGameClientCallback callback)
    {
        if (callback == null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        callback.ReceiveRoundEnded(_message);
    }
}
