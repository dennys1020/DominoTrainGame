using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.DominoTrainService;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IGameServices
{
    Task<LoginValidationStatus> LogInAsync(string usernameOrEmail, string password);
    Task<SignUpValidationStatus> CheckAvailabilityAsync(string username, string email, string password);
    Task<SignUpValidationStatus> RegisterUserAsync(string username, string email, string password, Guid verificationId);
    Task<RecoveryAccountDto> FindAccountAsync(string identifier);
    Task<RecoverPasswordValidationStatus> UpdatePasswordAsync(int playerId, string password, string confirmation, Guid verificationId);
    Task<List<LeaderboardEntry>> GetEntriesAsync(CancellationToken cancellationToken);
    Task<VerificationSessionDto> BeginVerificationAsync(string email);
    Task<bool> ResendVerificationAsync(Guid verificationId);
    Task<VerificationCodeStatus> VerifyCodeAsync(Guid verificationId, string enteredCode);
}
