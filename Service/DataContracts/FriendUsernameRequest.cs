using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "FriendUsernameRequest", Namespace = "http://dominotraingame/services")]
public sealed class FriendUsernameRequest
{
    [DataMember]
    public string Username { get; set; }
}
