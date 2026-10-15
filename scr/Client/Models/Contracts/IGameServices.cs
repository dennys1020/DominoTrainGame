using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.DominoTrainService;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IGameServices
{
    Task<LoginStatus> LogInAsync(string identifier, string password);
    Task<RegistrationStatus> RegisterUserAsync(User user);
    Task<RegistrationStatus> CheckAvailabilityAsync(User user);
    Task<VerificationSessionDto> BeginVerificationAsync(BeginVerificationRequest request);
    Task<bool> ResendVerificationAsync(Guid sessionId);
    Task<VerificationStatus> VerifyCodeAsync(Guid sessionId, string code);
    Task<List<LeaderboardEntry>> GetEntriesAsync(CancellationToken cancellationToken);
}
