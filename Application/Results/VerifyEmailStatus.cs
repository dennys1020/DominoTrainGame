namespace DominoTrainGame.Server.Application.Results;

public enum VerifyEmailStatus
{
    Success,
    RegistrationNotFound,
    InvalidCode,
    CodeExpired,
    TooManyAttempts,
    AlreadyRegistered
}
