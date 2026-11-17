using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "SelectDominoSetRequest", Namespace = "http://dominotraingame/services")]
public sealed class SelectDominoSetRequest
{
    [DataMember]
    public int HighestDouble { get; set; }
}
