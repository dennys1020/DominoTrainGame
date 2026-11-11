using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "HandMessage", Namespace =
    "http://dominotraingame/services")]
public sealed class HandMessage
{
    [DataMember]
    public List<TileData> Tiles { get; set; }
}
