using System;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class ResendVerificationRequest
{
    [DataMember]
    public Guid RequestId { get; set; }
    [DataMember]
    public Guid SessionId { get; set; }
}
