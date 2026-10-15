namespace DominoTrainGame.Server.Application.Results;

public enum LoginValidationStatus
{
    Success,
    EmptyFields,
    UserNotFound,
    IncorrectPassword,
    DatabaseError
}
