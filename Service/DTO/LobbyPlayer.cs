using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "LobbyPlayer", Namespace = "http://dominotraingame/services")]
public sealed class LobbyPlayer
{
    [DataMember]
    public int PlayerId { get; set; }

    [DataMember]
    public string Username { get; set; }

    [DataMember]
    public bool IsHost { get; set; }
}
