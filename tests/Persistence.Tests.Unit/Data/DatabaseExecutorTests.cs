using System;
using System.Configuration;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Persistence.Data;
using NUnit.Framework;

namespace DominoTrainGame.Server.Persistence.Tests.Unit.Data;

[TestFixture]
public sealed class DatabaseExecutorTests
{
    private const int ExpectedOperationResult = 42;
    private const int OperationData = 7;
    private const string ValidationErrorMessage = "The player entity is not valid.";
    private const string UpdateErrorMessage = "The changes could not be saved.";
    private const string EntityErrorMessage = "The underlying provider failed on Open.";
    private const string ConfigurationErrorMessage = "The connection string is missing.";
    private const string UnexpectedErrorMessage = "The operation is not valid in this state.";

    private DatabaseExecutor _databaseExecutor;
    private Func<DominoGameDBEntities, int> _operation;
    private Func<DominoGameDBEntities, int, int> _operationWithData;
    private Exception _operationException;

    [SetUp]
    public void SetUp()
    {
        _databaseExecutor = new DatabaseExecutor();
        _operation = ReturnExpectedResult;
        _operationWithData = ReturnOperationData;
        _operationException = new InvalidOperationException(UnexpectedErrorMessage);
    }

    [Test]
    public void Test_Execute_OperationSucceeds_ReturnsOperationResult()
    {
        int result = _databaseExecutor.Execute(_operation);

        Assert.That(result, Is.EqualTo(ExpectedOperationResult));
    }

    [Test]
    public void Test_Execute_OperationWithData_PassesDataToOperation()
    {
        int result = _databaseExecutor.Execute(_operationWithData, OperationData);

        Assert.That(result, Is.EqualTo(OperationData));
    }

    [Test]
    public void Test_Execute_NullOperation_ThrowsArgumentNullException()
    {
        _operation = null;

        Assert.Throws<ArgumentNullException>(ExecuteArrangedOperation);
    }

    [Test]
    public void Test_Execute_NullOperationWithData_ThrowsArgumentNullException()
    {
        _operationWithData = null;

        Assert.Throws<ArgumentNullException>(ExecuteArrangedOperationWithData);
    }

    [Test]
    public void Test_Execute_EntityValidationFails_ThrowsDataIntegrityException()
    {
        _operationException = new DbEntityValidationException(ValidationErrorMessage);

        Assert.Throws<DataIntegrityException>(ExecuteFailingOperation);
    }

    [Test]
    public void Test_Execute_UpdateFailsWithoutSqlError_ThrowsDatabaseUnavailableException()
    {
        _operationException = new DbUpdateException(UpdateErrorMessage);

        Assert.Throws<DatabaseUnavailableException>(ExecuteFailingOperation);
    }

    [Test]
    public void Test_Execute_EntityFrameworkFails_ThrowsDatabaseUnavailableException()
    {
        _operationException = new EntityException(EntityErrorMessage);

        Assert.Throws<DatabaseUnavailableException>(ExecuteFailingOperation);
    }

    [Test]
    public void Test_Execute_ConfigurationInvalid_ThrowsDatabaseUnavailableException()
    {
        _operationException = new ConfigurationErrorsException(ConfigurationErrorMessage);

        Assert.Throws<DatabaseUnavailableException>(ExecuteFailingOperation);
    }

    [Test]
    public void Test_Execute_EntityFrameworkFails_KeepsOriginalExceptionAsInner()
    {
        _operationException = new EntityException(EntityErrorMessage);

        Assert.That(
            ExecuteFailingOperation,
            Throws.TypeOf<DatabaseUnavailableException>().With.InnerException.SameAs(_operationException));
    }

    [Test]
    public void Test_Execute_EntityValidationFails_KeepsOriginalExceptionAsInner()
    {
        _operationException = new DbEntityValidationException(ValidationErrorMessage);

        Assert.That(
            ExecuteFailingOperation,
            Throws.TypeOf<DataIntegrityException>().With.InnerException.SameAs(_operationException));
    }

    [Test]
    public void Test_Execute_UnexpectedException_PropagatesOriginalException()
    {
        Assert.Throws<InvalidOperationException>(ExecuteFailingOperation);
    }

    private static int ReturnExpectedResult(DominoGameDBEntities context)
    {
        int result = ExpectedOperationResult;
        return result;
    }

    private static int ReturnOperationData(DominoGameDBEntities context, int operationData)
    {
        int result = operationData;
        return result;
    }

    private static int ThrowOperationException(DominoGameDBEntities context, Exception operationException)
    {
        throw operationException;
    }

    private void ExecuteArrangedOperation()
    {
        _databaseExecutor.Execute(_operation);
    }

    private void ExecuteArrangedOperationWithData()
    {
        _databaseExecutor.Execute(_operationWithData, OperationData);
    }

    private void ExecuteFailingOperation()
    {
        _databaseExecutor.Execute<Exception, int>(ThrowOperationException, _operationException);
    }
}
