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
    public bool TryCreatePlayer(PlayerRegistration registration, out PlayerCredentials credentials)
    {
        PlayerCredentials createdCredentials = DatabaseExecutor.Execute(
            context => CreatePlayer(context, registration));
        bool isCreated = createdCredentials.PlayerId != PlayerCredentials.Empty.PlayerId;

        credentials = createdCredentials;
        return isCreated;
    }

    private static PlayerCredentials CreatePlayer(DominoGameDBEntities context, PlayerRegistration registration)
    {
        PlayerCredentials credentials = PlayerCredentials.Empty;
        bool isTaken = ExistsPlayer(context, registration);

        if (!isTaken)
        {
            credentials = InsertPlayer(context, registration);
        }

        return credentials;
    }

    private static PlayerCredentials InsertPlayer(DominoGameDBEntities context, PlayerRegistration registration)
    {
        PlayerEntity player = new PlayerEntity
        {
            Username = registration.Username,
            Email = registration.Email,
            PasswordHash = registration.PasswordHash,
            IsGuest = RegisteredPlayerFlag
        };

        context.Players.Add(player);
        context.SaveChanges();

        PlayerCredentials credentials = new PlayerCredentials(
            player.PlayerId,
            player.Username,
            player.PasswordHash);
        return credentials;
    }
    private static bool ExistsPlayer(DominoGameDBEntities context, PlayerRegistration registration)
    {
        bool exists = context.Players
            .AsNoTracking()
            .Any(player => player.Username == registration.Username
                || player.Email == registration.Email);
        return exists;
    }
}
