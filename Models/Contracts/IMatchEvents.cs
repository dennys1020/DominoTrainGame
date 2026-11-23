using System;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IMatchEvents
{
    event EventHandler<EventArgs> MatchChanged;

    event EventHandler<EventArgs> HandChanged;

    event EventHandler<EventArgs> RoundEnded;

    event EventHandler<EventArgs> MatchFinished;

    event EventHandler<EventArgs> MatchAbandoned;

    event EventHandler<EventArgs> MatchLost;
}
