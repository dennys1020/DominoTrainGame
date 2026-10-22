namespace DominoTrainGame.Client.ViewModel.Validation;

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
