using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class LoginResponse
{
    [DataMember]
    public LoginValidationStatus Status { get; set; }
}
