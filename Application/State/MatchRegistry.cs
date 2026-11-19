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

    public bool TryAdd(MatchState match)
    {
        if (match == null)
        {
            throw new ArgumentNullException(nameof(match));
        }

        if (string.IsNullOrWhiteSpace(match.Code))
        {
            throw new ArgumentException("The match code cannot be empty.", nameof(match));
        }

        bool wasAdded = false;

        lock (_matchesLock)
        {
            if (CanAdd(match))
            {
                AddMatch(match);
                wasAdded = true;
            }
        }

        return wasAdded;
    }

    public MatchState FindByPlayer(int playerId)
    {
        MatchState match;

        lock (_matchesLock)
        {
            match = _matchesByPlayerId.ContainsKey(playerId) ? _matchesByPlayerId[playerId] : MatchState.Empty;
        }

        return match;
    }

    public void RemovePlayer(int playerId)
    {
        lock (_matchesLock)
        {
            _matchesByPlayerId.Remove(playerId);
        }
    }

    public void RemoveMatch(MatchState matchState)
    {
        if (matchState == null)
        {
            throw new ArgumentNullException(nameof(matchState));
        }

        lock (_matchesLock)
        {
            bool isRegisteredMatch = _matchesByCode.ContainsKey(matchState.Code)
                && ReferenceEquals(_matchesByCode[matchState.Code], matchState);

            if (isRegisteredMatch)
            {
                _matchesByCode.Remove(matchState.Code);
            }

            foreach(int playerId in matchState.PlayerIds)
            {
                _matchesByPlayerId.Remove(playerId);
            }
        }
    }

    private bool CanAdd(MatchState match)
    {
        bool isCodeAvailable = !_matchesByCode.ContainsKey(match.Code);
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

    private void AddMatch(MatchState match)
    {
        _matchesByCode.Add(match.Code, match);

        foreach (int playerId in match.PlayerIds)
        {
            _matchesByPlayerId.Add(playerId, match);
        }
    }
}
