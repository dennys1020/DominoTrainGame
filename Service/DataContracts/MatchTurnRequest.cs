using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "MatchTurnRequest", Namespace =
    "http://dominotraingame/services")]
public sealed class MatchTurnRequest
{
    [DataMember]
    public string SessionToken { get; set; }
}
