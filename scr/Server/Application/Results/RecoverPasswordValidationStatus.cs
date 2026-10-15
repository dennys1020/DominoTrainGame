namespace DominoTrainGame.Server.Application.Results;

public enum RecoverPasswordValidationStatus
{
    Success,
    EmptyFields,
    UserNotFound,
    PasswordTooShort,
    PasswordsDoNotMatch,
    VerificationRequired,
    DatabaseError
}
