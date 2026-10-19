using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "LoginStatus", Namespace = "http://dominotraingame/services")]
public enum LoginStatus
{
    [EnumMember]
    Success,

    [EnumMember]
    InvalidInput,

    [EnumMember]
    InvalidCredentials,

    [EnumMember]
    ServerError
}
