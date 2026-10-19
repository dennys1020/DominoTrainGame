using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Server.Persistence.Contracts;
using DominoTrainGame.Server.Persistence.Data;
using DominoTrainGame.Server.Persistence.Models;

namespace DominoTrainGame.Server.Persistence.Repositories;

public sealed class PlayerRepository : IPlayerRepository
{
    private const byte RegisteredPlayerFlag = 0;
    private const int MaximumMatches = 1;
    private const int FirstMatchIndex = 0;

    public bool TryFindCredentials(string identifier, out PlayerCredentials credentials)
    {
        List<PlayerCredentials> matches = DatabaseExecutor.Execute(
            context => FindCredentials(context, identifier));
        bool isFound = matches.Count == MaximumMatches;

        credentials = isFound ? matches[FirstMatchIndex] : PlayerCredentials.Empty;
        return isFound;
    }

    private static List<PlayerCredentials> FindCredentials(DominoGameDBEntities context, string identifier)
    {
        List<PlayerCredentials> matches = context.Players
            .AsNoTracking()
            .Where(player => player.IsGuest == RegisteredPlayerFlag)
            .Where(player => player.Username == identifier || player.Email == identifier)
            .Select(player => new { player.PlayerId, player.Username, player.PasswordHash })
            .Take(MaximumMatches)
            .AsEnumerable()
            .Select(row => new PlayerCredentials(row.PlayerId, row.Username, row.PasswordHash))
            .ToList();
        return matches;
    }
}
