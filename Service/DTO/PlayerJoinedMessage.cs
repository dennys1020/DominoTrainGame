using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "PlayerJoinedMessage", Namespace = "http://dominotraingame/services")]
public sealed class PlayerJoinedMessage
{
    [DataMember]
    public long Version { get; set; }

    [DataMember]
    public LobbyPlayer Player { get; set; }
}
