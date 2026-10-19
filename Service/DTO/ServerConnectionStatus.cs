using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "ServerConnectionStatus", Namespace = "http://dominotraingame/services")]
public enum ServerConnectionStatus
{
    [EnumMember]
    Connected,

    [EnumMember]
    DatabaseUnavailable
}
