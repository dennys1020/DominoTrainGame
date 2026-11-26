using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "HandMessage", Namespace =
    "http://dominotraingame/services")]
public sealed class HandMessage
{
    [DataMember]
    public List<TileData> Tiles { get; set; }
}
