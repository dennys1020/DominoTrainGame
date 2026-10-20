using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "GameServiceFaultCode", Namespace = "http://dominotraingame/services")]
public enum GameServiceFaultCode
{
    [EnumMember]
    InvalidInput,

    [EnumMember]
    DatabaseUnavailable
}
