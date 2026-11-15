using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "MatchEndedMessage", Namespace =
    "http://dominotraingame/services")]
public sealed class MatchEndedMessage
{
    [DataMember]
    public MatchEndReasonCode EndReason { get; set; }
}
