using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class VerificationCodeResponse
{
    [DataMember]
    public VerificationCodeStatus Status { get; set; }
}
