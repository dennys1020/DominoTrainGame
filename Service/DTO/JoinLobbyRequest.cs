using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "JoinLobbyRequest", Namespace = "http://dominotraingame/services")]
public sealed class JoinLobbyRequest
{
    [DataMember]
    public string Code { get; set; }
}
