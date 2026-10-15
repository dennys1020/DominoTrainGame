namespace DominoTrainGame.Server.Application.Results;

public enum SignUpValidationStatus
{
    Success,
    EmptyFields,
    InvalidEmail,
    PasswordTooShort,
    UserAlreadyExists,
    VerificationRequired,
    DatabaseError
}
