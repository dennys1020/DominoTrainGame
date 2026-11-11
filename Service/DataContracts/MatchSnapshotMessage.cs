using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "MatchSnapshotMessage", Namespace =
    "http://dominotraingame/services")]
public sealed class MatchSnapshotMessage
{
    [DataMember]
    public int CurrentPlayerId { get; set; }
    [DataMember]
    public int BoneyardCount { get; set; }
    [DataMember]
    public TileData EngineTile { get; set; }
    [DataMember]
    public List<MatchPlayerData> Players { get; set; }
    [DataMember]
    public List<TrainData> Trains { get; set; }
}
