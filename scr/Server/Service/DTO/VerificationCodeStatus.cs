using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public enum VerificationCodeStatus
{
    [EnumMember]
    Valid,
    [EnumMember]
    Invalid,
    [EnumMember]
    Expired,
    [EnumMember]
    TooManyAttempts,
    [EnumMember]
    ServiceUnavailable
}
