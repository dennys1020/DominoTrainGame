using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.ViewModel;
public sealed class PlayerResultViewModel
{
    public PlayerResultViewModel(PlayerSummary player, IReadOnlyList<int> roundScores, int winningTotal)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        Username = player.Username;
        Scores = roundScores ?? new List<int>();
        TotalScore = Scores.Sum();
        IsWinner = TotalScore == winningTotal;
    }

    public string Username { get; }

    public IReadOnlyList<int> Scores { get; }

    public int TotalScore { get; }

    public bool IsWinner { get; }
}
