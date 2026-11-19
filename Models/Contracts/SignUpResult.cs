namespace DominoTrainGame.Client.Models.Contracts;

public enum SignUpResult
{
    Success,
    InvalidInput,
    AlreadyExists,
    EmailUnavailable,
    DatabaseUnavailable,
    ServerError,
    ServerUnavailable
}
