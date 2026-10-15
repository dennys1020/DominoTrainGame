using System.Runtime.Serialization;
#nullable enable

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class LeaderboardEntry
{
    [DataMember]
    public int Rank
    {
        get;
        set;
    }

    [DataMember]
    public int PlayerId
    {
        get;
        set;
    }

    [DataMember]
    public string Username
    {
        get;
        set;
    } = string.Empty;

    [DataMember]
    public int Wins
    {
        get;
        set;
    }

    [DataMember]
    public int GamesPlayed
    {
        get;
        set;
    }

    [DataMember]
    public long TotalScore
    {
        get;
        set;
    }
}
