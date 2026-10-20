namespace DominoTrainGame.Client.Models.Contracts;

public enum LoginResult
{
    Success,
    InvalidInput,
    InvalidCredentials,
    DatabaseUnavailable,
    ServerError,
    ServerUnavailable
}
