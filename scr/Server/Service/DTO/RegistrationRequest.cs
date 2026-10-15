using System;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class RegistrationRequest
{
    [DataMember]
    public string Username { get; set; } = string.Empty;
    [DataMember]
    public string Email { get; set; } = string.Empty;
    [DataMember]
    public string Password { get; set; } = string.Empty;
    [DataMember]
    public string Language { get; set; } = "en";
    [DataMember]
    public Guid VerificationId { get; set; }
}
