using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class BeginVerificationRequest
{
    [DataMember]
    public string Email { get; set; } = string.Empty;
    [DataMember]
    public string Language { get; set; } = "en";
}
