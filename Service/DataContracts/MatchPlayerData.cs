using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "MatchPlayerData", Namespace =
    "http://dominotraingame/services")]
public sealed class MatchPlayerData
{
    [DataMember]
    public int PlayerId { get; set; }
    [DataMember]
    public int TileCount { get; set; }
}
