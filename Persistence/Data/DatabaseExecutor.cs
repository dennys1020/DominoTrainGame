using System;
using System.Configuration;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using DominoTrainGame.Server.Persistence.Errors;

namespace DominoTrainGame.Server.Persistence.Data;

internal static class DatabaseExecutor
{
    public static TResult Execute<TResult>(Func<DominoGameDBEntities, TResult> operation)
    {
        if (operation == null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        TResult result;

        try
        {
            using (DominoGameDBEntities context = DatabaseContextFactory.Create())
            {
                result = operation(context);
            }
        }
        catch (SqlException exception)
        {
            throw new DatabaseOperationException(
                "The database server could not complete the request.",
                exception);
        }
        catch (EntityException exception)
        {
            throw new DatabaseOperationException(
                "The Entity Framework model could not complete the request.",
                exception);
        }
        catch (ConfigurationErrorsException exception)
        {
            throw new DatabaseOperationException(
                "The database configuration is missing or invalid.",
                exception);
        }
        catch (ArgumentException exception)
        {
            throw new DatabaseOperationException(
                "The configured connection string has an invalid format.",
                exception);
        }
        catch (InvalidOperationException exception)
        {
            throw new DatabaseOperationException(
                "The database request could not be completed in the current state.",
                exception);
        }

        return result;
    }
}
