using System;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.UseCases;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace DominoTrainGame.Server.Application.Tests.Unit.UseCases;

[TestFixture]
public sealed class ConnectionUseCasesTests
{
    private const int ExpectedCallCount = 1;
    private const string DatabaseErrorMessage = "The database is unavailable.";

    private IConnectionRepository _connectionRepository;
    private ConnectionUseCases _connectionUseCases;

    [SetUp]
    public void SetUp()
    {
        _connectionRepository = Substitute.For<IConnectionRepository>();
        _connectionRepository.CanConnect().Returns(true);
        CreateConnectionUseCases();
    }

    [Test]
    public void Test_Constructor_NullConnectionRepository_ThrowsArgumentNullException()
    {
        _connectionRepository = null;

        Assert.Throws<ArgumentNullException>(CreateConnectionUseCases);
    }

    [Test]
    public void Test_VerifyDatabaseConnection_DatabaseReachable_DoesNotThrow()
    {
        Assert.DoesNotThrow(VerifyDatabaseConnection);
    }

    [Test]
    public void Test_VerifyDatabaseConnection_DatabaseUnreachable_ThrowsDatabaseUnavailableException()
    {
        _connectionRepository.CanConnect().Returns(false);

        Assert.Throws<DatabaseUnavailableException>(VerifyDatabaseConnection);
    }

    [Test]
    public void Test_VerifyDatabaseConnection_RepositoryThrows_PropagatesDatabaseUnavailableException()
    {
        _connectionRepository.CanConnect().Throws(new DatabaseUnavailableException(DatabaseErrorMessage));

        Assert.Throws<DatabaseUnavailableException>(VerifyDatabaseConnection);
    }

    [Test]
    public void Test_VerifyDatabaseConnection_DatabaseReachable_ChecksConnectionOnce()
    {
        VerifyDatabaseConnection();

        _connectionRepository.Received(ExpectedCallCount).CanConnect();
    }

    private void CreateConnectionUseCases()
    {
        _connectionUseCases = new ConnectionUseCases(_connectionRepository);
    }

    private void VerifyDatabaseConnection()
    {
        _connectionUseCases.VerifyDatabaseConnection();
    }
}
