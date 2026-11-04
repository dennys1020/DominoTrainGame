using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "PlayerReadyChangedMessage", Namespace = "http://dominotraingame/services")]
public sealed class PlayerReadyChangedMessage
{
    [DataMember]
    public long Version { get; set; }

    [DataMember]
    public int PlayerId { get; set; }

    [DataMember]
    public bool IsReady { get; set; }
}
