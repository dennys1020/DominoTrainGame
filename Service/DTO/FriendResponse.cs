using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "FriendResponse", Namespace = "http://dominotraingame/services")]
public sealed class FriendResponse
{
    [DataMember]
    public int PlayerId { get; set; }

    [DataMember]
    public string Username { get; set; }
}
