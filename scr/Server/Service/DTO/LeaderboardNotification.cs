using System;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class LeaderboardNotification
{
    [DataMember]
    public Guid RequestId { get; set; }
    [DataMember]
    public OperationState State { get; set; }
    [DataMember]
    public string Message { get; set; } = string.Empty;
    [DataMember]
    public LeaderboardResponse Result { get; set; }
}
