using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.State;
using DominoTrainGame.Server.Application.UseCases;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace DominoTrainGame.Server.Application.Tests.Unit.UseCases;

[TestFixture]
public sealed class ChatSanctionUseCasesTests
{
    private const int PlayerId = 21;
    private const int OtherPlayerId = 22;
    private const int NoPreviousSanctions = 0;
    private const int OnePreviousSanction = 1;
    private const int ManyPreviousSanctions = 3;
    private const int InfractionsBeforeLastSuccess = 5;
    private const int InfractionsBeforeWarning = 6;
    private const int InfractionsBeforeLastWarning = 8;
    private const int InfractionsBeforeSanction = 9;
    private const int FirstChatMuteMinutes = 2;
    private const int SecondChatMuteMinutes = 15;
    private const int GameBanMinutes = 30;
    private const string DatabaseErrorMessage = "The database is unavailable.";

    private ISanctionRepository _sanctionRepository;
    private ChatInfractionRegistry _infractionRegistry;
    private IClock _clock;
    private ChatSanctionUseCases _chatSanctionUseCases;
    private DateTime _now;

    [SetUp]
    public void SetUp()
    {
        _sanctionRepository = Substitute.For<ISanctionRepository>();
        _infractionRegistry = new ChatInfractionRegistry();
        _clock = Substitute.For<IClock>();
        _now = DateTime.UtcNow;

        _clock.UtcNow.Returns(_now);
        _sanctionRepository.FindActiveSanction(Arg.Any<int>(), Arg.Any<DateTime>()).Returns(PlayerSanction.Empty);
        _sanctionRepository.CountSanctions(PlayerId).Returns(NoPreviousSanctions);
        _sanctionRepository.AddSanction(Arg.Any<SanctionRecord>()).Returns(true);

        CreateChatSanctionUseCases();
    }

    [Test]
    public void Test_Constructor_NullSanctionRepository_ThrowsArgumentNullException()
    {
        _sanctionRepository = null;

        Assert.Throws<ArgumentNullException>(CreateChatSanctionUseCases);
    }

    [Test]
    public void Test_Constructor_NullInfractionRegistry_ThrowsArgumentNullException()
    {
        _infractionRegistry = null;

        Assert.Throws<ArgumentNullException>(CreateChatSanctionUseCases);
    }

    [Test]
    public void Test_Constructor_NullClock_ThrowsArgumentNullException()
    {
        _clock = null;

        Assert.Throws<ArgumentNullException>(CreateChatSanctionUseCases);
    }

    [Test]
    public void Test_CanChat_NoActiveSanction_ReturnsTrue()
    {
        bool canChat = _chatSanctionUseCases.CanChat(PlayerId);

        Assert.That(canChat, Is.True);
    }

    [Test]
    public void Test_CanChat_ActiveChatMute_ReturnsFalse()
    {
        ArrangeActiveSanction(SanctionType.ChatMute);

        bool canChat = _chatSanctionUseCases.CanChat(PlayerId);

        Assert.That(canChat, Is.False);
    }

    [Test]
    public void Test_CanChat_ActiveGameBan_ReturnsFalse()
    {
        ArrangeActiveSanction(SanctionType.GameBan);

        bool canChat = _chatSanctionUseCases.CanChat(PlayerId);

        Assert.That(canChat, Is.False);
    }

    [Test]
    public void Test_CanChat_AnyPlayer_SearchesSanctionAtCurrentTime()
    {
        _chatSanctionUseCases.CanChat(PlayerId);

        _sanctionRepository.Received().FindActiveSanction(PlayerId, _now);
    }

    [Test]
    public void Test_CanChat_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        ArrangeUnavailableSanctionSearch();

        Assert.Throws<DatabaseUnavailableException>(CheckArrangedPlayerCanChat);
    }

    [Test]
    public void Test_IsBannedFromPlaying_NoActiveSanction_ReturnsFalse()
    {
        bool isBanned = _chatSanctionUseCases.IsBannedFromPlaying(PlayerId);

        Assert.That(isBanned, Is.False);
    }

    [Test]
    public void Test_IsBannedFromPlaying_ActiveChatMute_ReturnsFalse()
    {
        ArrangeActiveSanction(SanctionType.ChatMute);

        bool isBanned = _chatSanctionUseCases.IsBannedFromPlaying(PlayerId);

        Assert.That(isBanned, Is.False);
    }

    [Test]
    public void Test_IsBannedFromPlaying_ActiveGameBan_ReturnsTrue()
    {
        ArrangeActiveSanction(SanctionType.GameBan);

        bool isBanned = _chatSanctionUseCases.IsBannedFromPlaying(PlayerId);

        Assert.That(isBanned, Is.True);
    }

    [Test]
    public void Test_IsBannedFromPlaying_DatabaseUnavailable_ThrowsDatabaseUnavailableException()
    {
        ArrangeUnavailableSanctionSearch();

        Assert.Throws<DatabaseUnavailableException>(CheckArrangedPlayerIsBanned);
    }

    [Test]
    public void Test_RegisterInfraction_FirstInfraction_ReturnsSuccess()
    {
        ChatInfractionStatus status = _chatSanctionUseCases.RegisterInfraction(PlayerId);

        Assert.That(status, Is.EqualTo(ChatInfractionStatus.Success));
    }

    [Test]
    public void Test_RegisterInfraction_FirstInfraction_DoesNotAddSanction()
    {
        _chatSanctionUseCases.RegisterInfraction(PlayerId);

        _sanctionRepository.DidNotReceive().AddSanction(Arg.Any<SanctionRecord>());
    }

    [Test]
    public void Test_RegisterInfraction_LastInfractionBeforeWarning_ReturnsSuccess()
    {
        RegisterInfractions(PlayerId, InfractionsBeforeLastSuccess);

        ChatInfractionStatus status = _chatSanctionUseCases.RegisterInfraction(PlayerId);

        Assert.That(status, Is.EqualTo(ChatInfractionStatus.Success));
    }

    [Test]
    public void Test_RegisterInfraction_FirstWarningInfraction_ReturnsWarned()
    {
        RegisterInfractions(PlayerId, InfractionsBeforeWarning);

        ChatInfractionStatus status = _chatSanctionUseCases.RegisterInfraction(PlayerId);

        Assert.That(status, Is.EqualTo(ChatInfractionStatus.Warned));
    }

    [Test]
    public void Test_RegisterInfraction_LastInfractionBeforeSanction_ReturnsWarned()
    {
        RegisterInfractions(PlayerId, InfractionsBeforeLastWarning);

        ChatInfractionStatus status = _chatSanctionUseCases.RegisterInfraction(PlayerId);

        Assert.That(status, Is.EqualTo(ChatInfractionStatus.Warned));
    }

    [Test]
    public void Test_RegisterInfraction_InfractionsBeforeSanction_DoNotAddSanction()
    {
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);

        _sanctionRepository.DidNotReceive().AddSanction(Arg.Any<SanctionRecord>());
    }

    [Test]
    public void Test_RegisterInfraction_SanctionThresholdWithoutPreviousSanctions_ReturnsChatBlocked()
    {
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);

        ChatInfractionStatus status = _chatSanctionUseCases.RegisterInfraction(PlayerId);

        Assert.That(status, Is.EqualTo(ChatInfractionStatus.ChatBlocked));
    }

    [Test]
    public void Test_RegisterInfraction_SanctionThresholdWithoutPreviousSanctions_AddsOneSanction()
    {
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);

        _chatSanctionUseCases.RegisterInfraction(PlayerId);

        Assert.That(GetAddedSanctions(), Has.One.Items);
    }

    [Test]
    public void Test_RegisterInfraction_SanctionThresholdWithoutPreviousSanctions_AddsChatMute()
    {
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);

        _chatSanctionUseCases.RegisterInfraction(PlayerId);

        Assert.That(GetAddedSanctions().Single().Sanction.Type, Is.EqualTo(SanctionType.ChatMute));
    }

    [Test]
    public void Test_RegisterInfraction_SanctionThresholdWithoutPreviousSanctions_ExpiresAfterFirstMute()
    {
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);

        _chatSanctionUseCases.RegisterInfraction(PlayerId);
        DateTime expiresAt = GetAddedSanctions().Single().Sanction.ExpiresAt;

        Assert.That(expiresAt, Is.EqualTo(_now.AddMinutes(FirstChatMuteMinutes)));
    }

    [Test]
    public void Test_RegisterInfraction_SanctionThreshold_StoresSanctionedPlayer()
    {
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);

        _chatSanctionUseCases.RegisterInfraction(PlayerId);

        Assert.That(GetAddedSanctions().Single().PlayerId, Is.EqualTo(PlayerId));
    }

    [Test]
    public void Test_RegisterInfraction_SanctionThreshold_StoresCurrentTimeAsStart()
    {
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);

        _chatSanctionUseCases.RegisterInfraction(PlayerId);

        Assert.That(GetAddedSanctions().Single().StartedAt, Is.EqualTo(_now));
    }

    [Test]
    public void Test_RegisterInfraction_SanctionThresholdWithOnePreviousSanction_ReturnsChatBlocked()
    {
        _sanctionRepository.CountSanctions(PlayerId).Returns(OnePreviousSanction);
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);

        ChatInfractionStatus status = _chatSanctionUseCases.RegisterInfraction(PlayerId);

        Assert.That(status, Is.EqualTo(ChatInfractionStatus.ChatBlocked));
    }

    [Test]
    public void Test_RegisterInfraction_SanctionThresholdWithOnePreviousSanction_ExpiresAfterSecondMute()
    {
        _sanctionRepository.CountSanctions(PlayerId).Returns(OnePreviousSanction);
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);

        _chatSanctionUseCases.RegisterInfraction(PlayerId);
        DateTime expiresAt = GetAddedSanctions().Single().Sanction.ExpiresAt;

        Assert.That(expiresAt, Is.EqualTo(_now.AddMinutes(SecondChatMuteMinutes)));
    }

    [Test]
    public void Test_RegisterInfraction_SanctionThresholdWithManyPreviousSanctions_ReturnsGameBanned()
    {
        _sanctionRepository.CountSanctions(PlayerId).Returns(ManyPreviousSanctions);
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);

        ChatInfractionStatus status = _chatSanctionUseCases.RegisterInfraction(PlayerId);

        Assert.That(status, Is.EqualTo(ChatInfractionStatus.GameBanned));
    }

    [Test]
    public void Test_RegisterInfraction_SanctionThresholdWithManyPreviousSanctions_AddsGameBan()
    {
        _sanctionRepository.CountSanctions(PlayerId).Returns(ManyPreviousSanctions);
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);

        _chatSanctionUseCases.RegisterInfraction(PlayerId);

        Assert.That(GetAddedSanctions().Single().Sanction.Type, Is.EqualTo(SanctionType.GameBan));
    }

    [Test]
    public void Test_RegisterInfraction_SanctionThresholdWithManyPreviousSanctions_ExpiresAfterGameBan()
    {
        _sanctionRepository.CountSanctions(PlayerId).Returns(ManyPreviousSanctions);
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);

        _chatSanctionUseCases.RegisterInfraction(PlayerId);
        DateTime expiresAt = GetAddedSanctions().Single().Sanction.ExpiresAt;

        Assert.That(expiresAt, Is.EqualTo(_now.AddMinutes(GameBanMinutes)));
    }

    [Test]
    public void Test_RegisterInfraction_AfterSanction_RestartsCount()
    {
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);
        _chatSanctionUseCases.RegisterInfraction(PlayerId);

        ChatInfractionStatus status = _chatSanctionUseCases.RegisterInfraction(PlayerId);

        Assert.That(status, Is.EqualTo(ChatInfractionStatus.Success));
    }

    [Test]
    public void Test_RegisterInfraction_OtherPlayerInfractions_DoNotCount()
    {
        RegisterInfractions(OtherPlayerId, InfractionsBeforeSanction);

        ChatInfractionStatus status = _chatSanctionUseCases.RegisterInfraction(PlayerId);

        Assert.That(status, Is.EqualTo(ChatInfractionStatus.Success));
    }

    [Test]
    public void Test_RegisterInfraction_DatabaseUnavailableOnCount_ThrowsDatabaseUnavailableException()
    {
        _sanctionRepository.CountSanctions(PlayerId).Throws(new DatabaseUnavailableException(DatabaseErrorMessage));
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);

        Assert.Throws<DatabaseUnavailableException>(RegisterPlayerInfraction);
    }

    [Test]
    public void Test_RegisterInfraction_DatabaseUnavailableOnAdd_ThrowsDatabaseUnavailableException()
    {
        _sanctionRepository.AddSanction(Arg.Any<SanctionRecord>())
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));
        RegisterInfractions(PlayerId, InfractionsBeforeSanction);

        Assert.Throws<DatabaseUnavailableException>(RegisterPlayerInfraction);
    }

    private void CreateChatSanctionUseCases()
    {
        _chatSanctionUseCases = new ChatSanctionUseCases(_sanctionRepository, _infractionRegistry, _clock);
    }

    private void ArrangeActiveSanction(SanctionType sanctionType)
    {
        PlayerSanction sanction = new PlayerSanction(sanctionType, DateTime.MaxValue);
        _sanctionRepository.FindActiveSanction(PlayerId, _now).Returns(sanction);
    }

    private void ArrangeUnavailableSanctionSearch()
    {
        _sanctionRepository.FindActiveSanction(PlayerId, _now)
            .Throws(new DatabaseUnavailableException(DatabaseErrorMessage));
    }

    private void CheckArrangedPlayerCanChat()
    {
        _chatSanctionUseCases.CanChat(PlayerId);
    }

    private void CheckArrangedPlayerIsBanned()
    {
        _chatSanctionUseCases.IsBannedFromPlaying(PlayerId);
    }

    private void RegisterInfractions(int playerId, int infractionCount)
    {
        foreach (int infractorPlayerId in Enumerable.Repeat(playerId, infractionCount))
        {
            _chatSanctionUseCases.RegisterInfraction(infractorPlayerId);
        }
    }

    private void RegisterPlayerInfraction()
    {
        _chatSanctionUseCases.RegisterInfraction(PlayerId);
    }

    private List<SanctionRecord> GetAddedSanctions()
    {
        List<SanctionRecord> addedSanctions = new List<SanctionRecord>();

        foreach (ICall repositoryCall in _sanctionRepository.ReceivedCalls())
        {
            if (repositoryCall.GetMethodInfo().Name == nameof(ISanctionRepository.AddSanction))
            {
                addedSanctions.Add((SanctionRecord)repositoryCall.GetArguments().Single());
            }
        }

        return addedSanctions;
    }
}
