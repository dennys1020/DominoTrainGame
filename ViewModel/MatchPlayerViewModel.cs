using System;
using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.ViewModel;

public sealed class MatchPlayerViewModel
{
    public MatchPlayerViewModel(MatchPlayerSummary player, int turnPlayerId)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        Username = player.Player.Username;
        HandCount = player.HandCount;
        Score = player.Score;
        HasTurn = player.Player.PlayerId == turnPlayerId;
    }

    public string Username { get; }

    public int HandCount { get; }

    public int Score { get; }

    public bool HasTurn { get; }
}
