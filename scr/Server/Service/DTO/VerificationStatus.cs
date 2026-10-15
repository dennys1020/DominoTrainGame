using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public enum VerificationStatus
{
    [EnumMember] SystemError,
    [EnumMember] Valid,
    [EnumMember] Invalid,
    [EnumMember] Expired,
    [EnumMember] TooManyAttempts
}
