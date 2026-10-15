using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public enum LoginStatus
{
    [EnumMember]
    SystemError,

    [EnumMember]
    Success,

    [EnumMember]
    EmptyFields,

    [EnumMember]
    UserNotFound,

    [EnumMember]
    IncorrectPassword
}
