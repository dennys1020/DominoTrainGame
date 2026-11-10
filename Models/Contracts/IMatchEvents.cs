using System;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IMatchEvents
{
    event EventHandler<EventArgs> MatchChanged;

    event EventHandler<EventArgs> HandChanged;
}
