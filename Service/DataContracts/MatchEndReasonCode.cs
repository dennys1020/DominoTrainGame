using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "MatchEndReasonCode", Namespace = "http://dominotraingame/services")]
public enum MatchEndReasonCode
{
    [EnumMember]
    AllRoundsPlayed,

    [EnumMember]
    NotEnoughPlayers
}
