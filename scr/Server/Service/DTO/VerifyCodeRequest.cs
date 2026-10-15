using System;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class VerifyCodeRequest
{
    [DataMember]
    public Guid SessionId { get; set; }
    [DataMember]
    public string Code { get; set; } = string.Empty;
}
