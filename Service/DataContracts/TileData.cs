using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "TileData", Namespace =
    "http://dominotraingame/services")]
public sealed class TileData
{
    [DataMember]
    public int RightPips { get; set; }
    [DataMember]
    public int LeftPips { get; set; }
}
