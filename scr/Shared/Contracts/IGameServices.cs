using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Models;
using DominoTrainGame.Utils;
using DominoTrainGame.Validator;

namespace DominoTrainGame.Contracts;

public interface IGameServices
{
    LoginValidationStatus LogInUser(string usernameOrEmail, string password);
    SignUpValidationStatus CheckAvailability(string username, string email, string password);
    SignUpValidationStatus RegisterUser(string username, string email, string password);
    RecoverPasswordValidationStatus FindAccount(string identifier, out int playerId, out string email);
    RecoverPasswordValidationStatus UpdatePassword(int playerId, string password, string confirmation);
    Task<List<LeaderboardEntry>> GetEntriesAsync(CancellationToken cancellationToken);
    IVerificationSession CreateVerificationSession();
    Task<bool> SendVerificationCodeAsync(string email, string code);
}
