namespace DominoTrainGame.Client.Models.Contracts;

public enum EmailVerificationResult
{
    Success,
    InvalidCode,
    CodeExpired,
    TooManyAttempts,
    EmailUnavailable,
    RegistrationNotFound,
    ResendTooSoon,
    AlreadyExists,
    DatabaseUnavailable,
    ServerError,
    ServerUnavailable
}
