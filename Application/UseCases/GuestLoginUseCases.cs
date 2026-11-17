using System;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class GuestLoginUseCases : IGuestLoginUseCases
{
    private const int MaximumUsernameAttempts = 5;
    private const int NoAttempts = 0;

    private static readonly ITraceLog _logger;

    private readonly IPlayerRepository _playerRepository;
    private readonly IGuestUsernameGenerator _guestUsernameGenerator;

    static GuestLoginUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(GuestLoginUseCases));
    }

    public GuestLoginUseCases(IPlayerRepository playerRepository, IGuestUsernameGenerator guestUsernameGenerator)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _guestUsernameGenerator = guestUsernameGenerator
            ?? throw new ArgumentNullException(nameof(guestUsernameGenerator));
    }

    public PlayerIdentity LogInAsGuest()
    {
        PlayerIdentity guest = PlayerIdentity.Empty;
        int attemptCount = NoAttempts;

        while (guest.IsEmpty && attemptCount < MaximumUsernameAttempts)
        {
            string guestUsername = _guestUsernameGenerator.Generate();
            guest = _playerRepository.CreateGuest(guestUsername);
            attemptCount++;
        }

        LogGuestLogin(guest, attemptCount);
        return guest;
    }

    private static void LogGuestLogin(PlayerIdentity guest, int attemptCount)
    {
        if (guest.IsEmpty)
        {
            _logger.WarnFormat(
                "A guest login was rejected because no free guest username was found. Attempts: {0}.",
                attemptCount);
        }
        else
        {
            _logger.InfoFormat("A guest logged in. PlayerId: {0}.", guest.PlayerId);
        }
    }
}
