using System;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "PendingFriendRequestResponse", Namespace = "http://dominotraingame/services")]
public sealed class PendingFriendRequestResponse
{
    [DataMember]
    public int RequestId { get; set; }

    [DataMember]
    public string RequesterUsername { get; set; }

    [DataMember]
    public DateTime RequestedAt { get; set; }
}
