using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using DominoTrainGame.Server.Persistence.Contracts;
using DominoTrainGame.Server.Persistence.Errors;
using DominoTrainGame.Server.Persistence.Models;

namespace DominoTrainGame.Server.Persistence.Repositories;

public sealed class SqlAuthenticationStore : IAuthenticationStore
{
    private const byte RegisteredPlayerFlag = 0;
    private const string DatabaseErrorMessage =
        "The account operation failed.";

    public AccountRecord FindAccount(string identifier)
    {
        AccountRecord accountRecord;
        accountRecord = Execute(database => database.Players
            .AsNoTracking()
            .Where(player =>
                player.Username == identifier ||
                player.Email == identifier)
            .Select(player => new AccountRecord
            {
                PlayerId = player.PlayerId,
                Email = player.Email,
                PasswordHash = player.PasswordHash
            })
            .FirstOrDefault());
        return accountRecord;
    }

    public bool PlayerExists(string username, string email)
    {
        bool isExistent;
        isExistent = Execute(database => database.Players
            .Any(player =>
                player.Username == username ||
                player.Email == email));
        return isExistent;
    }

    public bool SaveNewPlayer(Player player, string preferredLanguage)
    {
        bool isSaved = false;

        if (player != null)
        {
            isSaved = Execute(database =>
                SavePlayer(database, player, preferredLanguage));
        }

        return isSaved;
    }

    private static bool SavePlayer(
        DominoGameDBEntities database,
        Player player,
        string preferredLanguage)
    {
        bool isSaved = false;

        bool playerExists = database.Players.Any(existingPlayer =>
            existingPlayer.Username == player.Username ||
            existingPlayer.Email == player.Email);

        if (!playerExists)
        {
            DateTime registeredAt = DateTime.UtcNow;

            player.IsGuest = RegisteredPlayerFlag;

            Account account = new Account
            {
                Biography = string.Empty,
                PreferredLanguage = preferredLanguage,
                CreatedAt = registeredAt,
                LastSeenAt = registeredAt
            };

            player.Accounts.Add(account);

            database.Players.Add(player);
            database.SaveChanges();

            isSaved = true;
        }

        return isSaved;
    }

    private static T Execute<T>(Func<DominoGameDBEntities, T> query)
    {
        T result;

        try
        {
            using (DominoGameDBEntities database =
                DatabaseContextFactory.Create())
            {
                result = query(database);
            }
        }
        catch (DataException exception)
        {
            throw new DatabaseOperationException(
                DatabaseErrorMessage, exception);
        }
        catch (SqlException exception)
        {
            throw new DatabaseOperationException(
                DatabaseErrorMessage, exception);
        }

        return result;
    }
}
