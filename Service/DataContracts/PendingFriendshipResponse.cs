using System;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "PendingFriendshipResponse", Namespace = "http://dominotraingame/services")]
public sealed class PendingFriendshipResponse
{
    [DataMember]
    public int RequestId { get; set; }

    [DataMember]
    public string RequesterUsername { get; set; }

    [DataMember]
    public DateTime RequestedAt { get; set; }
}
