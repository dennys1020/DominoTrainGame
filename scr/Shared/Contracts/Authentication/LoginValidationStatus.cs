namespace DominoTrainGame.Validator;

/// <summary>
/// Possible outcomes when validating a login attempt.
/// The GUI layer (LogInWindow) maps each value to a localized message from UiStrings.
/// </summary>
public enum LoginValidationStatus
{
    Success,
    EmptyFields,
    UserNotFound,
    IncorrectPassword,
    DatabaseError
}
