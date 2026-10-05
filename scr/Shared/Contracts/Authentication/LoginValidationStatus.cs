namespace DominoTrainGame.Validator;

public enum LoginValidationStatus
{
    Success,
    EmptyFields,
    UserNotFound,
    IncorrectPassword,
    DatabaseError
}
