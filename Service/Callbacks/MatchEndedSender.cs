using System;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DataContracts;

namespace DominoTrainGame.Server.Service.Callbacks;

public sealed class MatchEndedSender
{
    private readonly MatchEndedMessage _message;

    public MatchEndedSender(MatchEndedMessage message)
    {
        _message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public void Send(IGameClientCallback callback)
    {
        if (callback == null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        callback.ReceiveMatchEnded(_message);
    }
}
