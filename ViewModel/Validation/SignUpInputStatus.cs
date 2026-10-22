namespace DominoTrainGame.ViewModels.Validation;

public enum SignUpInputStatus
{
    Valid,
    EmptyFields,
    UsernameTooLong,
    EmailTooLong,
    EmailInvalid,
    PasswordTooShort,
    PasswordTooLong
}
