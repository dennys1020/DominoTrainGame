using System.Linq;

namespace DominoTrainGame.Server.Persistence.Data.Repositories;

internal static class PlayerTypeFlags
{
    public const byte Registered = 0;
    public const byte Guest = 1;

    public static IQueryable<Player> OnlyRegistered(IQueryable<Player> players)
    {
        IQueryable<Player> registeredPlayers = players.Where(player => player.IsGuest == Registered);
        return registeredPlayers;
    }

    public static IQueryable<MatchResult> WithoutGuests(IQueryable<MatchResult> matchResults)
    {
        IQueryable<MatchResult> registeredResults = matchResults
            .Where(matchResult => matchResult.Player.IsGuest == Registered);
        return registeredResults;
    }
}
