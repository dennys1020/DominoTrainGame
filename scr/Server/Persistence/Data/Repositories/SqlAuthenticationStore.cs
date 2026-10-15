using System;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using DominoTrainGame.Server.Persistence.Contracts;
using DominoTrainGame.Server.Persistence.Errors;
using DominoTrainGame.Server.Persistence.Models;

namespace DominoTrainGame.Server.Persistence.Repositories;

public sealed class SqlAuthenticationStore : IAuthenticationStore
{
    public AccountRecord FindAccount(string identifier) => Execute(database => database.Players
        .AsNoTracking()
        .Where(player => player.Username == identifier || player.Email == identifier)
        .Select(player => new AccountRecord
        {
            PlayerId = player.PlayerId,
            Email = player.Email,
            PasswordHash = player.PasswordHash
        }).FirstOrDefault());

    public AccountRecord FindAccount(int playerId) => Execute(database => database.Players
        .AsNoTracking()
        .Where(player => player.PlayerId == playerId)
        .Select(player => new AccountRecord
        {
            PlayerId = player.PlayerId,
            Email = player.Email,
            PasswordHash = player.PasswordHash
        }).FirstOrDefault());

    public bool PlayerExists(string username, string email) => Execute(database => database.Players
        .Any(player => player.Username == username || player.Email == email));

    public bool SaveNewPlayer(string username, string email, string passwordHash, string preferredLanguage) =>
        Execute(database =>
        {
            if (database.Players.Any(player => player.Username == username || player.Email == email))
            {
                return false;
            }

            DateTime registeredAt = DateTime.UtcNow;
            Player player = new Player
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                IsGuest = 0
            };
            player.Accounts.Add(new Account
            {
                Biography = string.Empty,
                PreferredLanguage = preferredLanguage,
                CreatedAt = registeredAt,
                LastSeenAt = registeredAt
            });
            database.Players.Add(player);
            database.SaveChanges();
            return true;
        });

    public bool SaveNewPassword(int playerId, string passwordHash) => Execute(database =>
    {
        Player player = database.Players.FirstOrDefault(candidate => candidate.PlayerId == playerId);
        if (player is null)
        {
            return false;
        }

        player.PasswordHash = passwordHash;
        database.SaveChanges();
        return true;
    });

    private static T Execute<T>(Func<DominoGameDBEntities, T> query)
    {
        try
        {
            using (DominoGameDBEntities database = DatabaseContextFactory.Create())
            {
                return query(database);
            }
        }
        catch (Exception exception) when (exception is EntityException || exception is DbUpdateException
            || exception is DbEntityValidationException || exception is SqlException)
        {
            throw new DatabaseOperationException("The account database operation failed.", exception);
        }
    }
}
