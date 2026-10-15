using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public enum SignUpValidationStatus
{
    [EnumMember]
    Success,
    [EnumMember]
    EmptyFields,
    [EnumMember]
    InvalidEmail,
    [EnumMember]
    PasswordTooShort,
    [EnumMember]
    UserAlreadyExists,
    [EnumMember]
    VerificationRequired,
    [EnumMember]
    DatabaseError
}
