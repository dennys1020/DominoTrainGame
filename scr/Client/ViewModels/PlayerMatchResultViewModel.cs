using System.Collections.Generic;
using System.Linq;

namespace DominoTrainGame.ViewModels;

public sealed class PlayerMatchResultViewModel
{
    public PlayerMatchResultViewModel(RoomPlayerViewModel player, IReadOnlyList<RoundScoreViewModel> scores)
    {
        Player = player;
        Scores = scores;

        if (scores.Any() && scores.All(score => score.Points.HasValue))
        {
            TotalScore = scores.Sum(score => score.Points.Value);
        }
    }

    public RoomPlayerViewModel Player
    {
        get;
    }

    public IReadOnlyList<RoundScoreViewModel> Scores
    {
        get;
    }

    public int? TotalScore
    {
        get;
    }

    public bool IsWinner
    {
        get;
        internal set;
    }
}
