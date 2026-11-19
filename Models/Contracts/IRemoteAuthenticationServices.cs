using System.Threading.Tasks;

namespace DominoTrainGame.Client.Models.Contracts;

public interface IRemoteAuthenticationServices
{
    string PendingVerificationEmail { get; }

    bool IsGuest { get; }

    Task<ConnectionCheckResult> CheckConnectionAsync();

    Task<LoginResult> LogInAsync(string identifier, string password);

    Task<LoginResult> LogInAsGuestAsync();

    void LogOut();

    Task<SignUpResult> SignUpAsync(string username, string email, string password);

    Task<EmailVerificationResult> VerifyEmailAsync(string verificationCode);

    Task<EmailVerificationResult> ResendVerificationCodeAsync();
}
