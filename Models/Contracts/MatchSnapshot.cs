using System;
using System.Collections.Generic;
using DominoTrainGame.Shared.Common;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class MatchSnapshot
{
    private const int EmptyTableValue = 0;

    static MatchSnapshot()
    {
        MatchTurnSummary emptyTurn = new MatchTurnSummary(StateIdentifiers.EmptyVersion, StateIdentifiers.EmptyPlayerId);
        MatchTableSummary emptyTable = new MatchTableSummary(
            EmptyTableValue,
            EmptyTableValue,
            new List<TrainSummary>());

        Empty = new MatchSnapshot(emptyTurn, emptyTable, new List<MatchPlayerSummary>());
    }

    public MatchSnapshot(
        MatchTurnSummary turn,
        MatchTableSummary table,
        IReadOnlyList<MatchPlayerSummary> players)
    {
        if (players == null)
        {
            throw new ArgumentNullException(nameof(players));
        }

        Turn = turn ?? throw new ArgumentNullException(nameof(turn));
        Table = table ?? throw new ArgumentNullException(nameof(table));
        Players = new List<MatchPlayerSummary>(players).AsReadOnly();
    }

    public static MatchSnapshot Empty { get; }

    public MatchTurnSummary Turn { get; }

    public MatchTableSummary Table { get; }

    public IReadOnlyList<MatchPlayerSummary> Players { get; }

    public long Version => Turn.Version;

    public int CurrentPlayerId => Turn.CurrentPlayerId;

    public int EnginePips => Table.EnginePips;

    public int BoneyardCount => Table.BoneyardCount;

    public IReadOnlyList<TrainSummary> Trains => Table.Trains;

    public bool IsEmpty => ReferenceEquals(this, Empty);
}
