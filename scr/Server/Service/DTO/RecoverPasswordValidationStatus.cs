using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public enum RecoverPasswordValidationStatus
{
    [EnumMember]
    Success,
    [EnumMember]
    EmptyFields,
    [EnumMember]
    UserNotFound,
    [EnumMember]
    PasswordTooShort,
    [EnumMember]
    PasswordsDoNotMatch,
    [EnumMember]
    VerificationRequired,
    [EnumMember]
    DatabaseError
}
