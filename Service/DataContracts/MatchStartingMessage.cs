using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "MatchStartingMessage", Namespace = "http://dominotraingame/services")]
public sealed class MatchStartingMessage
{
    [DataMember]
    public string Code { get; set; }
}
