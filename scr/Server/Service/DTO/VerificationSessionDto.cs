using System;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class VerificationSessionDto
{
    [DataMember]
    public Guid SessionId { get; set; }
    [DataMember]
    public bool WasSent { get; set; }
}
