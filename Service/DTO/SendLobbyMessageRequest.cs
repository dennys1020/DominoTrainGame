using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "SendLobbyMessageRequest", Namespace = "http://dominotraingame/services")]
public sealed class SendLobbyMessageRequest
{
    [DataMember]
    public string Text { get; set; }
}
