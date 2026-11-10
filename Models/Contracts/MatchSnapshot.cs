using System;
using System.Collections.Generic;

namespace DominoTrainGame.Client.Models.Contracts;

public sealed class MatchSnapshot
{
    static MatchSnapshot()
    {
        MatchTurnSummary emptyTurn = new MatchTurnSummary(0, 0);
        MatchTableSummary emptyTable = new MatchTableSummary(0, 0, new List<TrainSummary>());

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
