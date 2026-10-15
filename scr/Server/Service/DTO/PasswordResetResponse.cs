using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class PasswordResetResponse
{
    [DataMember]
    public RecoverPasswordValidationStatus Status { get; set; }
}
