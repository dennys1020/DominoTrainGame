using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Persistence.Data.Repositories;

public sealed class PlayerRepository : IPlayerRepository
{
    private const byte RegisteredPlayerFlag = 0;
    private const int MaximumMatches = 1;
    private const int FirstMatchIndex = 0;

    private readonly DatabaseExecutor _databaseExecutor;

    public PlayerRepository(DatabaseExecutor databaseExecutor)
    {
        _databaseExecutor = databaseExecutor ?? throw new ArgumentNullException(nameof(databaseExecutor));
    }

    public bool TryFindCredentials(string identifier, out PlayerCredentials credentials)
    {
        List<PlayerCredentials> matches = _databaseExecutor.Execute(
            context => FindCredentials(context, identifier));
        bool isFound = matches.Count == MaximumMatches;

        credentials = isFound ? matches[FirstMatchIndex] : PlayerCredentials.Empty;
        return isFound;
    }

    public bool CreatePlayer(PlayerRegistration registration)
    {
        if (registration == null)
        {
            throw new ArgumentNullException(nameof(registration));
        }

        bool isRegistered = _databaseExecutor.Execute(
            context => RegisterPlayer(context, registration));
        return isRegistered;
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

    private static bool RegisterPlayer(DominoGameDBEntities context, PlayerRegistration registration)
    {
        bool isRegistered;

        using (DbContextTransaction transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
        {
            isRegistered = !IsRegistrationTaken(context, registration);

            if (isRegistered)
            {
                InsertPlayer(context, registration);
            }

            transaction.Commit();
        }

        return isRegistered;
    }

    private static bool IsRegistrationTaken(DominoGameDBEntities context, PlayerRegistration registration)
    {
        bool isTaken = context.Players
            .AsNoTracking()
            .Any(player => player.Username == registration.Username || player.Email == registration.Email);
        return isTaken;
    }

    private static void InsertPlayer(DominoGameDBEntities context, PlayerRegistration registration)
    {
        Player player = new Player
        {
            Username = registration.Username,
            Email = registration.Email,
            PasswordHash = registration.PasswordHash,
            IsGuest = RegisteredPlayerFlag
        };

        context.Players.Add(player);
        context.SaveChanges();
    }
}
