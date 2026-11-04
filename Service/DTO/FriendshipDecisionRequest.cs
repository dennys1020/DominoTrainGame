using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "FriendshipDecisionRequest", Namespace = "http://dominotraingame/services")]
public sealed class FriendshipDecisionRequest
{
    [DataMember]
    public int RequestId { get; set; }
}
