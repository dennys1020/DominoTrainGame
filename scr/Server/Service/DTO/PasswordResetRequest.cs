using System;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class PasswordResetRequest
{
    [DataMember]
    public int PlayerId { get; set; }
    [DataMember]
    public string Password { get; set; } = string.Empty;
    [DataMember]
    public string Confirmation { get; set; } = string.Empty;
    [DataMember]
    public Guid VerificationId { get; set; }
}
