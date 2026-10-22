namespace DominoTrainGame.Client.Models.Contracts;

public enum SignUpResult
{
    Success,
    InvalidInput,
    AlreadyExists,
    DatabaseUnavailable,
    ServerError,
    ServerUnavailable
}
