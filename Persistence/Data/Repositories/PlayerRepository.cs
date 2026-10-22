using System.Collections.Generic;
using System.Linq;
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

    public bool TryCreatePlayer(PlayerRegistration registration, out PlayerCredentials credentials)
    {
        PlayerCredentials createdCredentials = DatabaseExecutor.Execute(
            context => CreatePlayer(context, registration));
        bool isCreated = createdCredentials.PlayerId != PlayerCredentials.Empty.PlayerId;

        credentials = createdCredentials;
        return isCreated;
    }

    public bool IsUsernameTaken(string username)
    {
        bool isTaken = DatabaseExecutor.Execute(
            context => context.Players
                .AsNoTracking()
                .Any(player => player.Username == username));
        return isTaken;
    }

    public bool IsEmailTaken(string email)
    {
        bool isTaken = DatabaseExecutor.Execute(
            context => context.Players
                .AsNoTracking()
                .Any(player => player.Email == email));
        return isTaken;
    }

    public void AddPlayer(string username, string email, string passwordHash)
    {
        DatabaseExecutor.Execute(
            context => InsertPlayer(context, username, email, passwordHash));
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

    private static PlayerCredentials CreatePlayer(DominoGameDBEntities context, PlayerRegistration registration)
    {
        PlayerCredentials credentials = PlayerCredentials.Empty;
        bool isTaken = ExistsPlayer(context, registration);

        if (!isTaken)
        {
            credentials = InsertPlayer(
                context,
                registration.Username,
                registration.Email,
                registration.PasswordHash);
        }

        return credentials;
    }

    private static PlayerCredentials InsertPlayer(
        DominoGameDBEntities context,
        string username,
        string email,
        string passwordHash)
    {
        Player player = new Player
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
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
