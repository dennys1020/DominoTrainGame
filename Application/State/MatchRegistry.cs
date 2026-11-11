using System;
using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.State;

public sealed class MatchRegistry
{
    private readonly Dictionary<string, MatchState> _matchesByCode =
        new Dictionary<string, MatchState>(StringComparer.Ordinal);
    private readonly Dictionary<int, MatchState> _matchesByPlayerId = new Dictionary<int, MatchState>();
    private readonly object _matchesLock = new object();

    public bool TryAdd(string code, MatchState match)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("The match code cannot be empty.", nameof(code));
        }

        if (match == null)
        {
            throw new ArgumentNullException(nameof(match));
        }

        bool wasAdded = false;

        lock (_matchesLock)
        {
            if (CanAdd(code, match))
            {
                AddMatch(code, match);
                wasAdded = true;
            }
        }

        return wasAdded;
    }

    public bool TryFindByPlayer(int playerId, out MatchState match)
    {
        bool wasFound;
        MatchState foundMatch;

        lock (_matchesLock)
        {
            wasFound = _matchesByPlayerId.TryGetValue(playerId, out foundMatch);
        }

        match = wasFound ? foundMatch : MatchState.Empty;
        return wasFound;
    }

    private bool CanAdd(string code, MatchState match)
    {
        bool isCodeAvailable = !_matchesByCode.ContainsKey(code);
        bool arePlayersAvailable = true;

        foreach (int playerId in match.PlayerIds)
        {
            if (_matchesByPlayerId.ContainsKey(playerId))
            {
                arePlayersAvailable = false;
            }
        }

        bool canAdd = isCodeAvailable && arePlayersAvailable;
        return canAdd;
    }

    private void AddMatch(string code, MatchState match)
    {
        _matchesByCode.Add(code, match);

        foreach (int playerId in match.PlayerIds)
        {
            _matchesByPlayerId.Add(playerId, match);
        }
    }
}
