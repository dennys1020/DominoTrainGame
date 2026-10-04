namespace DominoTrainGame.Utils;

public enum SignUpValidationStatus
{
    Success,
    EmptyFields,
    InvalidEmail,
    PasswordTooShort,
    UserAlreadyExists,
    DatabaseError
}
