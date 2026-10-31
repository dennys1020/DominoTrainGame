using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "PlayerLeftMessage", Namespace = "http://dominotraingame/services")]
public sealed class PlayerLeftMessage
{
    [DataMember]
    public long Version { get; set; }

    [DataMember]
    public int PlayerId { get; set; }

    [DataMember]
    public int HostPlayerId { get; set; }
}
