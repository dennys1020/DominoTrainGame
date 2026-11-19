using System;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class PlayerStatistics
{
    public PlayerStatistics(string username, int wins, int gamesPlayed)
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Wins = wins;
        GamesPlayed = gamesPlayed;
    }

    public string Username { get; }

    public int Wins { get; }

    public int GamesPlayed { get; }
}
