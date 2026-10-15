namespace DominoTrainGame.Server.Application.Results;

public enum VerificationCodeStatus
{
    Valid,
    Invalid,
    Expired,
    TooManyAttempts,
    ServiceUnavailable
}
