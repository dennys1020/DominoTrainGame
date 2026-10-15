using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class RecoveryAccountDto
{
    [DataMember]
    public RecoverPasswordValidationStatus Status { get; set; }
    [DataMember]
    public int PlayerId { get; set; }
    [DataMember]
    public string Email { get; set; } = string.Empty;
}
