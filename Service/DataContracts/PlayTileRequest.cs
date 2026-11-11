using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "PlayTileRequest", Namespace =
    "http://dominotraingame/services")]
public sealed class PlayTileRequest
{
    [DataMember]
    public int TrainOwnerId { get; set; }
    [DataMember]
    public string SessionToken { get; set; }
    [DataMember]
    public TileData Tile { get; set; }
}
