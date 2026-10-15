using System;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class LeaderboardRequest
{
    [DataMember]
    public Guid RequestId { get; set; }
}
