using System;
using System.Configuration;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using DominoTrainGame.Server.Application.Errors;

namespace DominoTrainGame.Server.Persistence.Data;

public sealed class DatabaseExecutor
{
    public TResult Execute<TResult>(Func<DominoGameDBEntities, TResult> operation)
    {
        if (operation == null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        TResult result = Execute<Func<DominoGameDBEntities, TResult>, TResult>(RunOperation, operation);
        return result;
    }

    public TResult Execute<TData, TResult>(Func<DominoGameDBEntities, TData, TResult> operation, TData data)
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
                result = operation(context, data);
            }
        }
        catch (DbEntityValidationException exception)
        {
            throw new DatabaseUnavailableException(
                "The data does not satisfy the database model.",
                exception);
        }
        catch (DbUpdateException exception)
        {
            throw new DatabaseUnavailableException(
                "The changes could not be saved in the database.",
                exception);
        }
        catch (SqlException exception)
        {
            throw new DatabaseUnavailableException(
                "The database server could not complete the request.",
                exception);
        }
        catch (EntityException exception)
        {
            throw new DatabaseUnavailableException(
                "The Entity Framework model could not complete the request.",
                exception);
        }
        catch (ConfigurationErrorsException exception)
        {
            throw new DatabaseUnavailableException(
                "The database configuration is missing or invalid.",
                exception);
        }

        return result;
    }

    private static TResult RunOperation<TResult>(
        DominoGameDBEntities context,
        Func<DominoGameDBEntities, TResult> operation)
    {
        TResult result = operation(context);
        return result;
    }
}
