using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "FriendStatusMessage", Namespace = "http://dominotraingame/services")]
public sealed class FriendStatusMessage
{
    [DataMember]
    public int PlayerId { get; set; }

    [DataMember]
    public PlayerStatusCode Status { get; set; }
}
