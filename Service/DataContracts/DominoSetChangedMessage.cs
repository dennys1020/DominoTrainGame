using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "DominoSetChangedMessage", Namespace = "http://dominotraingame/services")]
public sealed class DominoSetChangedMessage
{
    [DataMember]
    public long Version { get; set; }

    [DataMember]
    public int HighestDouble { get; set; }

    [DataMember]
    public int MaximumPlayers { get; set; }
}
