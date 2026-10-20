using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "GameServiceFault", Namespace = "http://dominotraingame/services")]
public sealed class GameServiceFault
{
    [DataMember]
    public GameServiceFaultCode Code { get; set; }
}
