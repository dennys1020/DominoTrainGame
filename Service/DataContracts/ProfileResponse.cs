using System;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "ProfileResponse", Namespace = "http://dominotraingame/services")]
public sealed class ProfileResponse
{
    [DataMember]
    public string Username { get; set; }

    [DataMember]
    public string Biography { get; set; }

    [DataMember]
    public byte[] AvatarImage { get; set; }

    [DataMember]
    public string InstagramUsername { get; set; }

    [DataMember]
    public string FacebookUsername { get; set; }

    [DataMember]
    public string TwitterUsername { get; set; }

    [DataMember]
    public DateTime MemberSince { get; set; }

    [DataMember]
    public int GamesPlayed { get; set; }

    [DataMember]
    public int Wins { get; set; }

    [DataMember]
    public int TotalPoints { get; set; }
}
