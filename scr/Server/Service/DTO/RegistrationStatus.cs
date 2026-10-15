using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public enum RegistrationStatus
{
    [EnumMember] SystemError,
    [EnumMember] Success,
    [EnumMember] EmptyFields,
    [EnumMember] InvalidEmail,
    [EnumMember] PasswordTooShort,
    [EnumMember] UserAlreadyExists,
    [EnumMember] VerificationRequired
}
