using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "ReconnectRequest", Namespace = "http://dominotraingame/services")]
public sealed class ReconnectRequest
{
    [DataMember]
    public string MatchCode { get; set; }
}
