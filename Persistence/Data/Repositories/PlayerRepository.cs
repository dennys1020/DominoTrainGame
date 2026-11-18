using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;
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

    public PlayerCredentials FindCredentials(string identifier)
    {
        List<PlayerCredentials> matches = _databaseExecutor.Execute(
            context => QueryCredentials(context, identifier));
        bool isFound = matches.Count == MaximumMatches;
        PlayerCredentials credentials = isFound ? matches[FirstMatchIndex] : PlayerCredentials.Empty;

        return credentials;
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

    public PlayerIdentity CreateGuest(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("The guest username cannot be empty.", nameof(username));
        }

        GuestPlayerCreation guestCreation = new GuestPlayerCreation(username);
        PlayerIdentity guest = _databaseExecutor.Execute(guestCreation.Create);
        return guest;
    }

    public bool IsRegistrationTaken(string username, string email)
    {
        bool isTaken = _databaseExecutor.Execute(context => IsUsernameOrEmailInUse(context, username, email));
        return isTaken;
    }

    private static List<PlayerCredentials> QueryCredentials(DominoGameDBEntities context, string identifier)
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
            isRegistered = !IsUsernameOrEmailInUse(context, registration.Username, registration.Email);

            if (isRegistered)
            {
                InsertPlayer(context, registration);
            }

            transaction.Commit();
        }

        return isRegistered;
    }

    private static bool IsUsernameOrEmailInUse(DominoGameDBEntities context, string username, string email)
    {
        bool isInUse = context.Players
            .AsNoTracking()
            .Any(player => player.Username == username || player.Email == email);
        return isInUse;
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
