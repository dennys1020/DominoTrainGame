using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class LeaderboardResponse
{
    [DataMember]
    public bool Succeeded { get; set; }
    [DataMember]
    public List<LeaderboardEntry> Entries { get; set; } = new List<LeaderboardEntry>();
}
