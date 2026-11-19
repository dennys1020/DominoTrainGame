using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Persistence.Data.Repositories;

public sealed class MatchResultRepository : IMatchResultRepository
{
    private const int NoSavedRows = 0;
    private const string PrivateAccessType = "Private";
    private const string FinishedStatus = "Finished";

    private readonly DatabaseExecutor _databaseExecutor;

    public MatchResultRepository(DatabaseExecutor databaseExecutor)
    {
        _databaseExecutor = databaseExecutor ?? throw new ArgumentNullException(nameof(databaseExecutor));
    }

    public bool SaveMatchResults(FinishedMatchRecord finishedMatch)
    {
        if (finishedMatch == null)
        {
            throw new ArgumentNullException(nameof(finishedMatch));
        }

        bool wasSaved = _databaseExecutor.Execute(InsertFinishedMatch, finishedMatch);
        return wasSaved;
    }

    private static bool InsertFinishedMatch(DominoGameDBEntities context, FinishedMatchRecord finishedMatch)
    {
        bool wasSaved;

        using (DbContextTransaction transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
        {
            Match match = new Match
            {
                Lobby = FindOrCreateLobby(context, finishedMatch),
                Status = FinishedStatus
            };

            foreach (PlayerMatchResult playerResult in finishedMatch.PlayerResults)
            {
                match.MatchResults.Add(CreateMatchResult(playerResult));
            }

            context.Matches.Add(match);
            wasSaved = context.SaveChanges() > NoSavedRows;
            transaction.Commit();
        }

        return wasSaved;
    }

    private static Lobby FindOrCreateLobby(DominoGameDBEntities context, FinishedMatchRecord finishedMatch)
    {
        List<Lobby> savedLobbies = (from lobby in context.Lobbies
                                    where lobby.Token == finishedMatch.LobbyCode
                                    select lobby).ToList();
        Lobby matchLobby = savedLobbies.Any() ? savedLobbies.First() : CreateLobby(finishedMatch.LobbyCode);
        matchLobby.MaxPlayers = Convert.ToByte(finishedMatch.MaximumPlayers);
        matchLobby.Status = FinishedStatus;

        return matchLobby;
    }

    private static Lobby CreateLobby(string lobbyCode)
    {
        Lobby lobby = new Lobby
        {
            AccessType = PrivateAccessType,
            Token = lobbyCode
        };
        return lobby;
    }

    private static MatchResult CreateMatchResult(PlayerMatchResult playerResult)
    {
        MatchResult matchResult = new MatchResult
        {
            PlayerId = playerResult.PlayerId,
            FinalRank = playerResult.FinalRank,
            TotalScore = playerResult.TotalScore
        };
        return matchResult;
    }
}
