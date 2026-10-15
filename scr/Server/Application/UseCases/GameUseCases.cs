using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Server.Application.Authentication;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.Verification;
using DominoTrainGame.Server.Persistence.Contracts;
using DominoTrainGame.Server.Persistence.Errors;
using DominoTrainGame.Server.Persistence.Models;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class GameUseCases
{
    private readonly IAuthenticationStore _accounts;
    private readonly LoginValidator _login;
    private readonly SignUpValidator _signUp;
    private readonly RecoverPasswordValidator _recovery;
    private readonly ILeaderboardSource _leaderboard;
    private readonly VerificationUseCases _verification;

    public GameUseCases(IAuthenticationStore accounts, IEmailSender email, ILeaderboardSource leaderboard)
    {
        _accounts = accounts ?? throw new ArgumentNullException(nameof(accounts));
        _login = new LoginValidator(accounts);
        _signUp = new SignUpValidator(accounts);
        _recovery = new RecoverPasswordValidator(accounts);
        _leaderboard = leaderboard ?? throw new ArgumentNullException(nameof(leaderboard));
        _verification = new VerificationUseCases(email ?? throw new ArgumentNullException(nameof(email)));
    }

    public LoginValidationStatus LogInUser(string identifier, string password) => _login.LogInUser(identifier, password);
    public SignUpValidationStatus CheckAvailability(string username, string email, string password) =>
        _signUp.CheckAvailability(username, email, password);
    public RecoveryAccountResult FindAccount(string identifier) => _recovery.FindAccount(identifier);

    public SignUpValidationStatus RegisterUser(string username, string email, string password, string language, Guid verificationId) =>
        _verification.ExecuteVerified(verificationId, email,
            () => _signUp.RegisterUser(username, email, password, language),
            status => status == SignUpValidationStatus.Success, SignUpValidationStatus.VerificationRequired);

    public RecoverPasswordValidationStatus UpdatePassword(int playerId, string password, string confirmation, Guid verificationId)
    {
        try
        {
            AccountRecord account = _accounts.FindAccount(playerId);
            if (account is null)
            {
                return RecoverPasswordValidationStatus.UserNotFound;
            }
            return _verification.ExecuteVerified(verificationId, account.Email,
                () => _recovery.UpdatePassword(playerId, password, confirmation),
                status => status == RecoverPasswordValidationStatus.Success, RecoverPasswordValidationStatus.VerificationRequired);
        }
        catch (DatabaseOperationException)
        {
            return RecoverPasswordValidationStatus.DatabaseError;
        }
    }

    public Task<Guid> BeginVerificationAsync(string email, string language) => _verification.BeginAsync(email, language);
    public Task<bool> ResendVerificationAsync(Guid verificationId) => _verification.ResendAsync(verificationId);
    public VerificationCodeStatus VerifyCode(Guid verificationId, string code) => _verification.Verify(verificationId, code);

    public async Task<LeaderboardResultSet> GetLeaderboardAsync()
    {
        try
        {
            List<LeaderboardRecord> entries = await _leaderboard.GetEntriesAsync(CancellationToken.None).ConfigureAwait(false);
            return new LeaderboardResultSet
            {
                Succeeded = true,
                Entries = entries.Select(entry => new LeaderboardResult
                {
                    Rank = entry.Rank, PlayerId = entry.PlayerId, Username = entry.Username,
                    Wins = entry.Wins, GamesPlayed = entry.GamesPlayed, TotalScore = entry.TotalScore
                }).ToList()
            };
        }
        catch (DatabaseOperationException)
        {
            return new LeaderboardResultSet { Succeeded = false };
        }
    }
}
