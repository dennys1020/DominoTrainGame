namespace DominoTrainGame.Client.Models.Contracts;

public enum LoginResult
{
    Success,
    InvalidInput,
    InvalidCredentials,
    ServerError,
    ServerUnavailable
}
