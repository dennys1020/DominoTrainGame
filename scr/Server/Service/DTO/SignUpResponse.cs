using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class SignUpResponse
{
    [DataMember]
    public SignUpValidationStatus Status { get; set; }
}
