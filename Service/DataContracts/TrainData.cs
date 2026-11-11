using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "TrainData", Namespace =
    "http://dominotraingame/services")]
public sealed class TrainData
{
    [DataMember]
    public int OwnerId { get; set; }
    [DataMember]
    public bool IsOpen { get; set; }
    [DataMember]
    public List<TileData> Tiles { get; set; }
}
