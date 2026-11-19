using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "LeaderboardEntryData", Namespace =
    "http://dominotraingame/services")]
public sealed class LeaderboardEntryData
{
    [DataMember]
    public int Rank { get; set; }
    [DataMember]
    public string Username { get; set; }
    [DataMember]
    public int Wins { get; set; }
    [DataMember]
    public int GamesPlayed { get; set; }
}
