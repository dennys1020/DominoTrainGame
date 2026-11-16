using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "RoundEndedMessage", Namespace =
    "http://dominotraingame/services")]
public sealed class RoundEndedMessage
{
    [DataMember]
    public int EnginePips { get; set; }
    [DataMember]
    public RoundEndReasonCode EndReason { get; set; }
    [DataMember]
    public List<PlayerScoreData> RoundScores { get; set; }
}
