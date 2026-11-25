using System;
using System.Transactions;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Persistence.Data;
using DominoTrainGame.Server.Persistence.Data.Repositories;
using NUnit.Framework;

namespace DominoTrainGame.Server.Persistence.Tests.Integration.Data.Repositories;

[TestFixture]
public sealed class SanctionRepositoryTests
{
    private const int SanctionYear = 2026;
    private const int SanctionMonth = 11;
    private const int SanctionDay = 24;
    private const int MinutesUntilCheck = 5;
    private const int SanctionMinutes = 10;
    private const int GameBanMinutes = 30;
    private const int ExpectedSanctionCount = 2;
    private const int UnknownPlayerId = -1;
    private const string Username = "samuel";
    private const string Email = "samuel@uv.mx";
    private const string PasswordHash = "stored-hash";

    private static readonly DateTime _startedAt = new DateTime(SanctionYear, SanctionMonth, SanctionDay);
    private static readonly DateTime _checkedAt = _startedAt.AddMinutes(MinutesUntilCheck);
    private static readonly DateTime _chatMuteExpiresAt = _startedAt.AddMinutes(SanctionMinutes);
    private static readonly DateTime _gameBanExpiresAt = _startedAt.AddMinutes(GameBanMinutes);
    private static readonly DateTime _expiredSanctionExpiresAt = _startedAt;

    private TransactionScope _transaction;
    private DatabaseExecutor _databaseExecutor;
    private SanctionRepository _sanctionRepository;
    private SanctionRecord _sanctionRecord;
    private int _playerId;

    [SetUp]
    public void SetUp()
    {
        _transaction = new TransactionScope();
        _databaseExecutor = new DatabaseExecutor();
        _sanctionRepository = new SanctionRepository(_databaseExecutor);
        PlayerRepository playerRepository = new PlayerRepository(_databaseExecutor);

        playerRepository.CreatePlayer(new PlayerRegistration(Username, Email, PasswordHash));
        _playerId = playerRepository.FindCredentials(Username).PlayerId;
        _sanctionRecord = CreateSanctionRecord(SanctionType.ChatMute, _chatMuteExpiresAt);
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

        Assert.Throws<ArgumentNullException>(CreateSanctionRepository);
    }

    [Test]
    public void Test_AddSanction_ChatMute_ReturnsTrue()
    {
        bool wasSaved = _sanctionRepository.AddSanction(_sanctionRecord);

        Assert.That(wasSaved, Is.True);
    }

    [Test]
    public void Test_AddSanction_GameBan_ReturnsTrue()
    {
        _sanctionRecord = CreateSanctionRecord(SanctionType.GameBan, _gameBanExpiresAt);

        bool wasSaved = _sanctionRepository.AddSanction(_sanctionRecord);

        Assert.That(wasSaved, Is.True);
    }

    [Test]
    public void Test_AddSanction_NullRecord_ThrowsArgumentNullException()
    {
        _sanctionRecord = null;

        Assert.Throws<ArgumentNullException>(AddArrangedSanction);
    }

    [Test]
    public void Test_AddSanction_UnknownPlayer_ThrowsDataIntegrityException()
    {
        _playerId = UnknownPlayerId;
        _sanctionRecord = CreateSanctionRecord(SanctionType.ChatMute, _chatMuteExpiresAt);

        Assert.Throws<DataIntegrityException>(AddArrangedSanction);
    }

    [Test]
    public void Test_CountSanctions_NoSanctions_ReturnsZero()
    {
        int sanctionCount = _sanctionRepository.CountSanctions(_playerId);

        Assert.That(sanctionCount, Is.Zero);
    }

    [Test]
    public void Test_CountSanctions_ChatMuteAndGameBan_CountsBoth()
    {
        AddChatMuteAndGameBan();

        int sanctionCount = _sanctionRepository.CountSanctions(_playerId);

        Assert.That(sanctionCount, Is.EqualTo(ExpectedSanctionCount));
    }

    [Test]
    public void Test_FindActiveSanction_NoSanctions_ReturnsEmptySanction()
    {
        PlayerSanction sanction = _sanctionRepository.FindActiveSanction(_playerId, _checkedAt);

        Assert.That(sanction.IsEmpty, Is.True);
    }

    [Test]
    public void Test_FindActiveSanction_ActiveChatMute_ReturnsChatMute()
    {
        _sanctionRepository.AddSanction(_sanctionRecord);

        PlayerSanction sanction = _sanctionRepository.FindActiveSanction(_playerId, _checkedAt);

        Assert.That(sanction.Type, Is.EqualTo(SanctionType.ChatMute));
    }

    [Test]
    public void Test_FindActiveSanction_ActiveChatMute_ReturnsExpiration()
    {
        _sanctionRepository.AddSanction(_sanctionRecord);

        PlayerSanction sanction = _sanctionRepository.FindActiveSanction(_playerId, _checkedAt);

        Assert.That(sanction.ExpiresAt, Is.EqualTo(_chatMuteExpiresAt));
    }

    [Test]
    public void Test_FindActiveSanction_ExpiredSanction_ReturnsEmptySanction()
    {
        _sanctionRecord = CreateSanctionRecord(SanctionType.ChatMute, _expiredSanctionExpiresAt);
        _sanctionRepository.AddSanction(_sanctionRecord);

        PlayerSanction sanction = _sanctionRepository.FindActiveSanction(_playerId, _checkedAt);

        Assert.That(sanction.IsEmpty, Is.True);
    }

    [Test]
    public void Test_FindActiveSanction_ChatMuteAndGameBan_ReturnsGameBan()
    {
        AddChatMuteAndGameBan();

        PlayerSanction sanction = _sanctionRepository.FindActiveSanction(_playerId, _checkedAt);

        Assert.That(sanction.Type, Is.EqualTo(SanctionType.GameBan));
    }

    [Test]
    public void Test_FindActiveSanction_ChatMuteAndGameBan_ReturnsLatestExpiration()
    {
        AddChatMuteAndGameBan();

        PlayerSanction sanction = _sanctionRepository.FindActiveSanction(_playerId, _checkedAt);

        Assert.That(sanction.ExpiresAt, Is.EqualTo(_gameBanExpiresAt));
    }

    private SanctionRecord CreateSanctionRecord(SanctionType sanctionType, DateTime expiresAt)
    {
        SanctionRecord sanctionRecord = new SanctionRecord(
            _playerId,
            new PlayerSanction(sanctionType, expiresAt),
            _startedAt);
        return sanctionRecord;
    }

    private void AddChatMuteAndGameBan()
    {
        _sanctionRepository.AddSanction(_sanctionRecord);
        _sanctionRepository.AddSanction(CreateSanctionRecord(SanctionType.GameBan, _gameBanExpiresAt));
    }

    private void CreateSanctionRepository()
    {
        _sanctionRepository = new SanctionRepository(_databaseExecutor);
    }

    private void AddArrangedSanction()
    {
        _sanctionRepository.AddSanction(_sanctionRecord);
    }
}
