using System;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class VerificationNotification
{
    [DataMember]
    public Guid RequestId { get; set; }
    [DataMember]
    public OperationState State { get; set; }
    [DataMember]
    public string Message { get; set; } = string.Empty;
    [DataMember]
    public VerificationSessionDto Result { get; set; }
}
