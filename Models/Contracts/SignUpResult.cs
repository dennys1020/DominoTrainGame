namespace DominoTrainGame.Client.Models.Contracts;

public enum SignupResult
{
    Success,
    InvalidInput,
    AlreadyExists,
    DatabaseUnavailable,
    ServerError,
    ServerUnavailable
}
