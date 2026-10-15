using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class LoginRequest
{
    [DataMember]
    public string Identifier { get; set; } = string.Empty;
    [DataMember]
    public string Password { get; set; } = string.Empty;
}
