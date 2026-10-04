namespace DominoTrainGame;

public enum RecoverPasswordValidationStatus
{
    Success,
    EmptyFields,
    UserNotFound,
    PasswordTooShort,
    PasswordsDoNotMatch,
    DatabaseError
}
