using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "RoundEndReasonCode", Namespace = "http://dominotraingame/services")]
public enum RoundEndReasonCode
{
    [EnumMember]
    EmptyHand,

    [EnumMember]
    Blocked
}
