using System;
using System.Transactions;
using DominoTrainGame.Server.Persistence.Data;
using DominoTrainGame.Server.Persistence.Data.Repositories;
using NUnit.Framework;

namespace DominoTrainGame.Server.Persistence.Tests.Integration.Data.Repositories;

[TestFixture]
public sealed class ConnectionRepositoryTests
{
    private TransactionScope _transaction;
    private DatabaseExecutor _databaseExecutor;
    private ConnectionRepository _connectionRepository;

    [SetUp]
    public void SetUp()
    {
        _transaction = new TransactionScope();
        _databaseExecutor = new DatabaseExecutor();
        CreateConnectionRepository();
    }

    [TearDown]
    public void TearDown()
    {
        _transaction.Dispose();
    }

    [Test]
    public void Test_Constructor_NullDatabaseExecutor_ThrowsArgumentNullException()
    {
        _databaseExecutor = null;

        Assert.Throws<ArgumentNullException>(CreateConnectionRepository);
    }

    [Test]
    public void Test_CanConnect_TestDatabaseAvailable_ReturnsTrue()
    {
        bool canConnect = _connectionRepository.CanConnect();

        Assert.That(canConnect, Is.True);
    }

    private void CreateConnectionRepository()
    {
        _connectionRepository = new ConnectionRepository(_databaseExecutor);
    }
}
