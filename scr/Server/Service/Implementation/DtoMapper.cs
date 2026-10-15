using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Service.Implementation;

internal static class DtoMapper
{
    public static DTO.LoginValidationStatus Map(LoginValidationStatus status) => status switch
    {
        LoginValidationStatus.Success => DTO.LoginValidationStatus.Success,
        LoginValidationStatus.EmptyFields => DTO.LoginValidationStatus.EmptyFields,
        LoginValidationStatus.UserNotFound => DTO.LoginValidationStatus.UserNotFound,
        LoginValidationStatus.IncorrectPassword => DTO.LoginValidationStatus.IncorrectPassword,
        _ => DTO.LoginValidationStatus.DatabaseError
    };

    public static DTO.SignUpValidationStatus Map(SignUpValidationStatus status) => status switch
    {
        SignUpValidationStatus.Success => DTO.SignUpValidationStatus.Success,
        SignUpValidationStatus.EmptyFields => DTO.SignUpValidationStatus.EmptyFields,
        SignUpValidationStatus.InvalidEmail => DTO.SignUpValidationStatus.InvalidEmail,
        SignUpValidationStatus.PasswordTooShort => DTO.SignUpValidationStatus.PasswordTooShort,
        SignUpValidationStatus.UserAlreadyExists => DTO.SignUpValidationStatus.UserAlreadyExists,
        SignUpValidationStatus.VerificationRequired => DTO.SignUpValidationStatus.VerificationRequired,
        _ => DTO.SignUpValidationStatus.DatabaseError
    };

    public static DTO.VerificationCodeStatus Map(VerificationCodeStatus status) => status switch
    {
        VerificationCodeStatus.Valid => DTO.VerificationCodeStatus.Valid,
        VerificationCodeStatus.Invalid => DTO.VerificationCodeStatus.Invalid,
        VerificationCodeStatus.Expired => DTO.VerificationCodeStatus.Expired,
        VerificationCodeStatus.TooManyAttempts => DTO.VerificationCodeStatus.TooManyAttempts,
        _ => DTO.VerificationCodeStatus.ServiceUnavailable
    };
}
